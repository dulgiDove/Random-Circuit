using Unity.Netcode;
using System.Collections;
using UnityEngine;

public class FallingTile : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject tileModel;
    [SerializeField]
    private Collider solidCollider;


    [Header("Collapse")]
    [SerializeField]
    private float collapseDelay = 0.8f;
    [SerializeField]
    private float sinkDistance = 10f;
    [SerializeField]
    private float sinkDuration = 0.5f;

    [Header("Respawn")]
    [SerializeField]
    private float respawnDelay = 3f;

    [Header("VFX")]
    [SerializeField]
    private GameObject disappearVfxPrefab;
    [SerializeField]
    private GameObject respawnVfxPrefab;
    [SerializeField]
    private float vfxLifetime = 2f;

    [Header("Respawn Effect")]
    [SerializeField]
    private Vector3 respawnVfxOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField]
    private float respawnRevealDelay = 0.2f;

    [Header("Network")]
    [SerializeField]
    private int networkId;

    private MapActivity mapActivity;
    private Coroutine collapseCoroutine;

    public int NetworkId => networkId;

    public float TotalCycleDuration => collapseDelay + sinkDuration + respawnDelay + 0.15f + respawnRevealDelay;

    private Vector3 startPosition;
    private bool isActivated;

    private void Awake()
    {
        Debug.Assert(tileModel != null);
        Debug.Assert(solidCollider != null);
        Debug.Assert(disappearVfxPrefab != null);
        Debug.Assert(respawnVfxPrefab != null);

        mapActivity = GetComponentInParent<MapActivity>();
        Debug.Assert(mapActivity != null);

        startPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated)
            return;

        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();

        if (networkObject == null || !networkObject.IsOwner)
            return;

        if (mapActivity.Manager == null)
            return;

        mapActivity.Manager.RequestFallingTileServerRpc(mapActivity.MapIndex, networkId);
    }

    public void ApplyNetworkFall(double startServerTime)
    {
        if (isActivated)
            return;

        isActivated = true;

        if (collapseCoroutine != null)
        {
            StopCoroutine(collapseCoroutine);
        }

        collapseCoroutine = StartCoroutine(CollapseRoutine(startServerTime));
    }


    private IEnumerator CollapseRoutine(double startServerTime)
    {
        double collapseStartTime = startServerTime + collapseDelay;

        while (NetworkManager.Singleton.ServerTime.Time < collapseStartTime)
        {
            yield return null;
        }

        Vector3 sinkEnd = startPosition + Vector3.down * sinkDistance;
        double sinkEndTime = collapseStartTime + sinkDuration;

        while (NetworkManager.Singleton.ServerTime.Time < sinkEndTime)
        {
            double currentTime = NetworkManager.Singleton.ServerTime.Time;

            float t = sinkDuration <= 0f ? 1f : Mathf.Clamp01((float)((currentTime - collapseStartTime) / sinkDuration));

            transform.position = Vector3.Lerp(startPosition, sinkEnd, t);

            yield return null;
        }

        transform.position = sinkEnd;

        double disappearTime = sinkEndTime + respawnDelay;

        while (NetworkManager.Singleton.ServerTime.Time < disappearTime)
        {
            yield return null;
        }

        PlayVfx(disappearVfxPrefab, transform.position);

        tileModel.SetActive(false);
        solidCollider.enabled = false;

        double restoreTime = disappearTime + 0.15d;

        while (NetworkManager.Singleton.ServerTime.Time < restoreTime)
        {
            yield return null;
        }

        transform.position = startPosition;

        PlayVfx(respawnVfxPrefab, startPosition + respawnVfxOffset);

        double revealTime = restoreTime + respawnRevealDelay;

        while (NetworkManager.Singleton.ServerTime.Time < revealTime)
        {
            yield return null;
        }

        tileModel.SetActive(true);
        solidCollider.enabled = true;

        collapseCoroutine = null;
    }

    public void ApplyNetworkReset()
    {
        if (collapseCoroutine != null)
        {
            StopCoroutine(collapseCoroutine);
            collapseCoroutine = null;
        }

        transform.position = startPosition;

        tileModel.SetActive(true);
        solidCollider.enabled = true;

        isActivated = false;
    }


    private void PlayVfx(GameObject prefab, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        Destroy(instance, vfxLifetime);
    }
}