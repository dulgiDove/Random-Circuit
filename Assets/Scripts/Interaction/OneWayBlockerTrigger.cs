using Unity.Netcode;
using UnityEngine;

public class OneWayBlockerTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Collider returnBlocker;

    private bool activated;

    private void Awake()
    {
        Debug.Assert(returnBlocker != null);
    }

    private void Start()
    {
        returnBlocker.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
        {
            return;
        }

        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();

        if (networkObject == null || !networkObject.IsOwner)
        {
            return;
        }

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        activated = true;
        returnBlocker.enabled = true;
    }
}