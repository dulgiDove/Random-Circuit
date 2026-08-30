using UnityEngine;

public class MovementModeTrigger : MonoBehaviour
{
    [SerializeField]
    private PlayerMovementMode mode;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        movement.SetMovementMode(mode);
    }
}
