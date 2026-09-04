using Unity.Netcode;
using UnityEngine;

public class MovementModeTrigger : MonoBehaviour
{
    [SerializeField]
    private PlayerMovementMode mode;

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();

        if (networkObject == null || !networkObject.IsOwner)
            return;

        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        movement.SetMovementMode(mode);
    }
}
