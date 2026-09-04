using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class MapManager : NetworkBehaviour
{
    [Header("Maps")]
    [SerializeField]
    private List<MapModule> mapPrefabs;
    [SerializeField]
    private int mapCount = 3;

    [Header("Goal")]
    [SerializeField]
    private GameObject goalLinePrefab;
    [SerializeField]
    private float goalDistance = 3f;

    private NetworkList<int> selectedMapIndices;
    private bool mapsGenerated;
    private readonly List<MapModule> spawnedMaps = new List<MapModule>();
    private readonly Dictionary<int, HashSet<ulong>> activePlayersByMap = new Dictionary<int, HashSet<ulong>>();

    private class CannonServerState
    {
        public ulong clientId;
        public double chargeStartTime;
        public bool returning;
    }

    private readonly Dictionary<Vector2Int, CannonServerState> cannonStates = new Dictionary<Vector2Int, CannonServerState>();
    private readonly Dictionary<Vector2Int, ulong> hideHoleStates = new Dictionary<Vector2Int, ulong>();
    private readonly HashSet<Vector2Int> fallingTileStates = new HashSet<Vector2Int>();

    public Transform GoalTransform { get; private set; }

    private void Awake()
    {
        Debug.Assert(mapPrefabs != null);
        Debug.Assert(goalLinePrefab != null);

        selectedMapIndices = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        selectedMapIndices.OnListChanged += OnMapSelectionChanged;

        if (IsServer && selectedMapIndices.Count == 0)
        {
            SelectMaps();
        }

        TryGenerateMaps();

        if (IsClient)
        {
            StartCoroutine(PlaceLocalPlayerWhenReady());
        }
    }

    public override void OnNetworkDespawn()
    {
        selectedMapIndices.OnListChanged -= OnMapSelectionChanged;
    }

    private void OnMapSelectionChanged(NetworkListEvent<int> changeEvent)
    {
        TryGenerateMaps();
    }

    private void TryGenerateMaps()
    {
        if (mapsGenerated || selectedMapIndices.Count != mapCount)
            return;

        mapsGenerated = true;

        Vector3 nextEntrancePosition = Vector3.zero;

        for (int i = 0; i < selectedMapIndices.Count; i++)
        {
            int mapIndex = selectedMapIndices[i];

            MapModule map = Instantiate(mapPrefabs[mapIndex], nextEntrancePosition, Quaternion.identity, transform);

            MapActivity activity = map.GetComponentInChildren<MapActivity>(true); 

            if (activity == null)
                return;
            
            activity.Initialize(this, i);

            spawnedMaps.Add(map);
            nextEntrancePosition = map.ExitPoint.position;
        }

        CreateGoalLine();
    }

    private void SelectMaps()
    {
        if (mapPrefabs.Count < mapCount)
            return;

        List<int> candidates = new List<int>();

        for (int i = 0; i < mapPrefabs.Count; i++)
        {
            candidates.Add(i);
        }

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            int temp = candidates[i];
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temp;
        }

        selectedMapIndices.Clear();

        for (int i = 0; i < mapCount; i++)
        {
            selectedMapIndices.Add(candidates[i]);
        }
    }

    private IEnumerator PlaceLocalPlayerWhenReady()
    {
        yield return new WaitUntil(() => mapsGenerated && spawnedMaps.Count > 0 && NetworkManager.Singleton.LocalClient.PlayerObject != null);

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

        Transform entrance = spawnedMaps[0].EntrancePoint;

        CharacterController controller = playerObject.GetComponent<CharacterController>();

        if (controller == null)
            yield break;

        controller.enabled = false;

        int playerIndex = (int)playerObject.OwnerClientId;

        int column = playerIndex % 5;
        int row = playerIndex / 5;

        Vector3 startOffset = new Vector3(column - 2f, 0f, row * 2f);
        Vector3 startPosition = entrance.TransformPoint(startOffset);
        playerObject.transform.SetPositionAndRotation(startPosition, entrance.rotation);

        controller.enabled = true;
    }

    private void CreateGoalLine()
    {
        if (spawnedMaps.Count == 0)
            return;

        Transform finalExit = spawnedMaps[spawnedMaps.Count - 1].ExitPoint;
        Vector3 goalPosition = finalExit.position + finalExit.forward * goalDistance + Vector3.up * 0.01f;

        GameObject goal = Instantiate(goalLinePrefab, goalPosition, finalExit.rotation, transform);

        GoalTransform = goal.transform;
    }

    public void RequestMapActivityChange(int mapIndex, bool entering)
    {
        RequestMapActivityChangeServerRpc(mapIndex, entering);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMapActivityChangeServerRpc(int mapIndex, bool entering, ServerRpcParams rpcParams = default)
    {
        if (mapIndex < 0 || mapIndex >= spawnedMaps.Count)
            return;

        if (!activePlayersByMap.TryGetValue(mapIndex, out HashSet<ulong> players))
        {
            players = new HashSet<ulong>();
            activePlayersByMap.Add(mapIndex, players);
        }

        bool wasActive = players.Count > 0;
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (entering)
        {
            players.Add(clientId);
        }
        else
        {
            players.Remove(clientId);
        }

        bool isActive = players.Count > 0;

        if (wasActive == isActive)
            return;

        double activationTime = isActive ? NetworkManager.ServerTime.Time : -1d;

        ApplyMapActivityClientRpc(mapIndex, isActive, activationTime);
    }

    [ClientRpc]
    private void ApplyMapActivityClientRpc(int mapIndex, bool isActive, double activationTime)
    {
        if (mapIndex < 0 || mapIndex >= spawnedMaps.Count)
            return;

        MapActivity activity = spawnedMaps[mapIndex].GetComponentInChildren<MapActivity>(true);

        if (activity == null)
            return;

        activity.ApplyNetworkState(isActive, activationTime);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCannonStartServerRpc(int mapIndex, int cannonId, ServerRpcParams rpcParams = default)
    {
        Vector2Int key = new Vector2Int(mapIndex, cannonId);

        if (cannonStates.ContainsKey(key))
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        cannonStates[key] = new CannonServerState{clientId = clientId, chargeStartTime = NetworkManager.ServerTime.Time, returning = false};

        ApplyCannonStartClientRpc(mapIndex, cannonId, clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCannonFireServerRpc(int mapIndex, int cannonId, ServerRpcParams rpcParams = default)
    {
        Vector2Int key = new Vector2Int(mapIndex, cannonId);

        if (!cannonStates.TryGetValue(key, out CannonServerState state))
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;

        if (state.clientId != clientId || state.returning)
            return;

        Cannon cannon = FindCannon(mapIndex, cannonId);

        if (cannon == null)
            return;

        double chargeDuration = NetworkManager.ServerTime.Time - state.chargeStartTime;
        float launchSpeed = cannon.GetLaunchSpeed(chargeDuration);
        state.returning = true;

        ApplyCannonFireClientRpc(mapIndex, cannonId, clientId, launchSpeed);
    }

    public void CompleteCannonReturnFromServer(int mapIndex, int cannonId)
    {
        if (!IsServer)
            return;

        Vector2Int key = new Vector2Int(mapIndex, cannonId);

        if (!cannonStates.TryGetValue(key, out CannonServerState state))
            return;

        if (!state.returning)
            return;

        cannonStates.Remove(key);

        ApplyCannonIdleClientRpc(mapIndex, cannonId);
    }

    [ClientRpc]
    private void ApplyCannonStartClientRpc(int mapIndex, int cannonId, ulong clientId)
    {
        Cannon cannon = FindCannon(mapIndex, cannonId);

        NetworkObject playerObject = FindPlayerObject(clientId);

        if (cannon == null || playerObject == null)
            return;

        PlayerInteraction interaction = playerObject.GetComponent<PlayerInteraction>();

        cannon.ApplyNetworkStart(interaction);

        if (playerObject.IsOwner)
        {
            interaction.SetActiveInteraction(cannon);
        }
    }

    [ClientRpc]
    private void ApplyCannonFireClientRpc(int mapIndex, int cannonId, ulong clientId, float launchSpeed)
    {
        Cannon cannon = FindCannon(mapIndex, cannonId);

        NetworkObject playerObject = FindPlayerObject(clientId);

        if (cannon == null || playerObject == null)
            return;

        PlayerInteraction interaction = playerObject.GetComponent<PlayerInteraction>();

        cannon.ApplyNetworkFire(interaction, launchSpeed);

        if (playerObject.IsOwner)
        {
            interaction.ClearActiveInteraction(cannon);
        }
    }

    [ClientRpc]
    private void ApplyCannonIdleClientRpc(int mapIndex, int cannonId)
    {
        Cannon cannon = FindCannon(mapIndex, cannonId);

        if (cannon != null)
        {
            cannon.ApplyNetworkIdle();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestHideHoleEnterServerRpc(int mapIndex, int holeId, ServerRpcParams rpcParams = default)
    {
        Vector2Int key = new Vector2Int(mapIndex, holeId);

        if (hideHoleStates.ContainsKey(key))
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        hideHoleStates[key] = clientId;

        ApplyHideHoleEnterClientRpc(mapIndex, holeId, clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestHideHoleExitServerRpc(int mapIndex, int holeId, ServerRpcParams rpcParams = default)
    {
        Vector2Int key = new Vector2Int(mapIndex, holeId);

        if (!hideHoleStates.TryGetValue(key, out ulong ownerId))
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;

        if (ownerId != clientId)
            return;

        hideHoleStates.Remove(key);

        ApplyHideHoleExitClientRpc(mapIndex, holeId, clientId);
    }

    [ClientRpc]
    private void ApplyHideHoleEnterClientRpc(int mapIndex, int holeId, ulong clientId)
    {
        HideHole hole = FindHideHole(mapIndex, holeId);

        NetworkObject playerObject = FindPlayerObject(clientId);

        if (hole == null || playerObject == null)
            return;

        PlayerInteraction interaction = playerObject.GetComponent<PlayerInteraction>();

        hole.ApplyNetworkEnter(interaction);

        if (playerObject.IsOwner)
        {
            interaction.SetActiveInteraction(hole);
        }
    }

    [ClientRpc]
    private void ApplyHideHoleExitClientRpc(int mapIndex, int holeId, ulong clientId)
    {
        HideHole hole = FindHideHole(mapIndex, holeId);

        NetworkObject playerObject = FindPlayerObject(clientId);

        if (hole == null || playerObject == null)
            return;

        PlayerInteraction interaction = playerObject.GetComponent<PlayerInteraction>();

        hole.ApplyNetworkExit(interaction);

        if (playerObject.IsOwner)
        {
            interaction.ClearActiveInteraction(hole);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestFallingTileServerRpc(int mapIndex, int tileId)
    {
        Vector2Int key = new Vector2Int(mapIndex, tileId);

        if (fallingTileStates.Contains(key))
            return;

        FallingTile tile = FindFallingTile(mapIndex, tileId);

        if (tile == null)
            return;

        fallingTileStates.Add(key);

        double startTime = NetworkManager.ServerTime.Time;

        ApplyFallingTileStartClientRpc(mapIndex, tileId, startTime);

        StartCoroutine(ResetFallingTileServer(mapIndex, tileId, startTime + tile.TotalCycleDuration));
    }

    private IEnumerator ResetFallingTileServer(int mapIndex, int tileId, double resetTime)
    {
        while (IsServer && NetworkManager.ServerTime.Time < resetTime)
        {
            yield return null;
        }

        if (!IsServer)
            yield break;

        Vector2Int key = new Vector2Int(mapIndex, tileId);

        fallingTileStates.Remove(key);

        ApplyFallingTileResetClientRpc(mapIndex, tileId);
    }

    [ClientRpc]
    private void ApplyFallingTileStartClientRpc(int mapIndex, int tileId, double startTime)
    {
        FallingTile tile = FindFallingTile(mapIndex, tileId);

        if (tile != null)
        {
            tile.ApplyNetworkFall(startTime);
        }
    }

    [ClientRpc]
    private void ApplyFallingTileResetClientRpc(int mapIndex, int tileId)
    {
        FallingTile tile = FindFallingTile(mapIndex, tileId);

        if (tile != null)
        {
            tile.ApplyNetworkReset();
        }
    }

    private Cannon FindCannon(int mapIndex, int id)
    {
        if (mapIndex < 0 || mapIndex >= spawnedMaps.Count)
        {
            return null;
        }

        Cannon[] objects = spawnedMaps[mapIndex].GetComponentsInChildren<Cannon>(true);

        foreach (Cannon cannon in objects)
        {
            if (cannon.NetworkId == id)
                return cannon;
        }

        return null;
    }

    private HideHole FindHideHole(int mapIndex, int id)
    {
        if (mapIndex < 0 || mapIndex >= spawnedMaps.Count)
            return null;

        HideHole[] objects = spawnedMaps[mapIndex].GetComponentsInChildren<HideHole>(true);

        foreach (HideHole hole in objects)
        {
            if (hole.NetworkId == id)
                return hole;
        }

        return null;
    }

    private FallingTile FindFallingTile(int mapIndex, int id)
    {
        if (mapIndex < 0 || mapIndex >= spawnedMaps.Count)
            return null;

        FallingTile[] objects = spawnedMaps[mapIndex].GetComponentsInChildren<FallingTile>(true);

        foreach (FallingTile tile in objects)
        {
            if (tile.NetworkId == id)
                return tile;
        }

        return null;
    }

    private NetworkObject FindPlayerObject(ulong clientId)
    {
        NetworkObject[] networkObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);

        foreach (NetworkObject networkObject in networkObjects)
        {
            if (networkObject.OwnerClientId != clientId)
                continue;

            if (networkObject.GetComponent<PlayerInteraction>() != null)
            {
                return networkObject;
            }
        }

        return null;
    }
}