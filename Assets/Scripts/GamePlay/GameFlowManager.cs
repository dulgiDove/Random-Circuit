using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameFlowManager : NetworkBehaviour
{
    [Header("Start UI")]
    [SerializeField]
    private GameObject startOverlay;
    [SerializeField]
    private TMP_Text countdownText;

    [Header("Countdown")]
    [SerializeField]
    private float numberDuration = 1f;
    [SerializeField]
    private float goDuration = 0.5f;

    [Header("Clear UI")]
    [SerializeField]
    private GameObject clearOverlay;
    [SerializeField]
    private TMP_Text clearTimeText;
    [SerializeField]
    private GameObject resultGroup;
    [SerializeField]
    private TMP_Text finalRankText;
    [SerializeField]
    private float resultDelay = 0.8f;

    [Header("Network")]
    [SerializeField]
    private int minimumPlayersToStart = 2;

    private readonly NetworkVariable<double> raceStartTime = new NetworkVariable<double>(-1d);

    private readonly HashSet<ulong> finishedClients = new HashSet<ulong>();
    private int finishCount;

    private readonly HashSet<ulong> retryReadyClients = new HashSet<ulong>();
    private bool waitingForRetry;

    private bool gameStarted;
    public bool GameStarted => gameStarted;

    private float elapsedTime;
    public float ElapsedTime => elapsedTime;

    private bool localPlayerFinished;

    private bool gameFinished;

    private bool isExiting;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        Debug.Assert(startOverlay != null);
        Debug.Assert(countdownText != null);

        Debug.Assert(clearOverlay != null);
        Debug.Assert(clearTimeText != null);
        Debug.Assert(resultGroup != null);
        Debug.Assert(finalRankText != null);
    }

    private void Start()
    {
        clearOverlay.SetActive(false);
        startOverlay.SetActive(true);
        countdownText.text = "Waiting...";
    }

    private void Update()
    {
        if (waitingForRetry)
            return;

        if (!IsSpawned || raceStartTime.Value < 0d)
            return;

        double currentTime = NetworkManager.ServerTime.Time;
        double remainingTime = raceStartTime.Value - currentTime;

        if (!gameStarted)
        {
            if (remainingTime > 0d)
            {
                int number = Mathf.Clamp(Mathf.CeilToInt((float)(remainingTime / numberDuration)), 1, 3);
                countdownText.text = number.ToString();
                return;
            }

            countdownText.text = "GO!";

            gameStarted = true;
            gameFinished = false;
            elapsedTime = 0f;

            if (playerMovement != null)
            {
                playerMovement.SetControlsLocked(false);
            }
        }

        if (!gameFinished && !localPlayerFinished)
        {
            elapsedTime = Mathf.Max(0f, (float)(currentTime - raceStartTime.Value));
        }

        if (currentTime >= raceStartTime.Value + goDuration && startOverlay.activeSelf)
        {
            startOverlay.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(BindLocalPlayer());

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        if (IsServer)
        {
            StartCoroutine(ScheduleRaceStart());
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void FinishGame()
    {
        if (!gameStarted || gameFinished)
            return;

        gameFinished = true;
        GameRecords.RecordClear(elapsedTime);

        if (playerMovement != null)
        {
            playerMovement.SetControlsLocked(true);
        }

        StartCoroutine(FinishGameRoutine());
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public void RetryGame()
    {
        if (waitingForRetry)
            return;

        waitingForRetry = true;

        clearOverlay.SetActive(false);
        startOverlay.SetActive(true);
        countdownText.text = "Waiting...";

        if (playerMovement != null)
        {
            playerMovement.SetControlsLocked(true);
        }

        RetryReadyServerRpc();
    }

    public void ReportFinish()
    {
        ReportFinishServerRpc();
    }

    public async void ExitGame()
    {
        if (isExiting)
            return;

        isExiting = true;

        NetworkManager networkManager = NetworkManager.Singleton;

        try
        {
            if (!MultiplayerSessionManager.IsSinglePlayer)
            {
                if (networkManager != null && networkManager.IsHost)
                {
                    await MultiplayerSessionManager.DeleteCurrentSessionAsync();
                }
                else
                {
                    await MultiplayerSessionManager.LeaveCurrentSessionAsync();
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        if (networkManager != null && networkManager.IsListening)
        {
            networkManager.Shutdown();
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (isExiting)
            return;

        if (!IsServer)
        {
            HandleHostDisconnected();
            return;
        }

        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
            return;

        finishedClients.Remove(clientId);
        retryReadyClients.Remove(clientId);

        int remainingPlayers = networkManager.ConnectedClients.Count;

        if (remainingPlayers <= 1 && !MultiplayerSessionManager.IsSinglePlayer)
        {
            ExitGame();
            return;
        }

        CheckFinishCondition();
        CheckRetryCondition();
    }

    private async void HandleHostDisconnected()
    {
        if (isExiting)
            return;

        isExiting = true;

        try
        {
            await MultiplayerSessionManager.LeaveCurrentSessionAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to leave session after host disconnect: {exception.Message}");
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("MainMenu");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RetryReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!retryReadyClients.Add(clientId))
            return;

        CheckRetryCondition();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReportFinishServerRpc(
        ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!finishedClients.Add(clientId))
            return;

        finishCount++;

        float finishTime = Mathf.Max(0f, (float)(NetworkManager.ServerTime.Time - raceStartTime.Value)
        );

        ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };

        ApplyFinishResultClientRpc(finishCount, finishTime, clientRpcParams);

        CheckFinishCondition();
    }

    [ClientRpc]
    private void ApplyFinishResultClientRpc(int rank, float finishTime, ClientRpcParams clientRpcParams = default)
    {
        if (localPlayerFinished)
            return;

        localPlayerFinished = true;
        elapsedTime = finishTime;

        finalRankText.text = $"Rank {rank}";

        GameRecords.RecordClear(finishTime);
    }

    [ClientRpc]
    private void EndRaceClientRpc()
    {
        if (gameFinished)
            return;

        gameFinished = true;

        if (playerMovement != null)
        {
            playerMovement.SetControlsLocked(true);
        }

        StartCoroutine(FinishGameRoutine());
    }

    private void ReloadGameplay()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void CheckFinishCondition()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
            return;

        if (finishedClients.Count >= networkManager.ConnectedClients.Count)
        {
            EndRaceClientRpc();
        }
    }

    private void CheckRetryCondition()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
            return;

        if (retryReadyClients.Count >= networkManager.ConnectedClients.Count)
        {
            ReloadGameplay();
        }
    }

    private IEnumerator BindLocalPlayer()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
            yield break;

        yield return new WaitUntil(() => networkManager.LocalClient.PlayerObject != null);

        playerMovement = networkManager.LocalClient.PlayerObject.GetComponent<PlayerMovement>();

        if (playerMovement == null)
            yield break;

        playerMovement.SetControlsLocked(true);
    }

    private IEnumerator ScheduleRaceStart()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null)
            yield break;

        int requiredPlayers = MultiplayerSessionManager.IsSinglePlayer ? 1 : minimumPlayersToStart;

        while (networkManager.ConnectedClients.Count < requiredPlayers)
        {
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(0.5f);

        raceStartTime.Value = networkManager.ServerTime.Time + numberDuration * 3f;
    }

    private IEnumerator FinishGameRoutine()
    {
        clearOverlay.SetActive(true);
        resultGroup.SetActive(false);
        yield return new WaitForSeconds(resultDelay);

        clearTimeText.text = GetFormattedTime();
        resultGroup.SetActive(true);
    }
}