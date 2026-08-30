using UnityEngine;

public class OneWayBlockerTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Collider returnBlocker;

    private bool activated;

    private void Awake()
    {
        if (returnBlocker != null)
        {
            returnBlocker.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
        {
            return;
        }

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        activated = true;

        if (returnBlocker != null)
        {
            returnBlocker.enabled = true;
        }
    }
}