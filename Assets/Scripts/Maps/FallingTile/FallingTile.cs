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

    private Vector3 startPosition;
    private bool isActivated;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        if (isActivated)
        {
            return;
        }

        isActivated = true;
        StartCoroutine(CollapseRoutine());
    }


    private IEnumerator CollapseRoutine()
    {
        yield return new WaitForSeconds(collapseDelay);

        Vector3 sinkStart = transform.position;
        Vector3 sinkEnd = startPosition + Vector3.down * sinkDistance;
        float timer = 0f;

        while (timer < sinkDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / sinkDuration);
            transform.position = Vector3.Lerp(sinkStart, sinkEnd, t);

            yield return null;
        }

        transform.position = sinkEnd;
        yield return new WaitForSeconds(respawnDelay);

        PlayVfx(disappearVfxPrefab, transform.position);

        tileModel.SetActive(false);
        solidCollider.enabled = false;
        yield return new WaitForSeconds(0.15f);

        transform.position = startPosition;
        PlayVfx(respawnVfxPrefab, startPosition + respawnVfxOffset);
        yield return new WaitForSeconds(respawnRevealDelay);

        tileModel.SetActive(true);
        solidCollider.enabled = true;
        isActivated = false;
    }


    private void PlayVfx(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        Destroy(instance, vfxLifetime);
    }
}