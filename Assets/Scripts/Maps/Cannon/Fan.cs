using UnityEngine;

public class Fan : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform targetHeight;

    [Header("Capture")]
    [SerializeField]
    private float captureDuration = 0.2f;

    [Header("Vertical Wind")]
    [SerializeField]
    private float response = 5f;
    [SerializeField]
    private float maxAcceleration = 150f;
    [SerializeField]
    private float stopThreshold = 0.1f;
    [SerializeField]
    private float minimumRiseSpeed = 3f;

    private PlayerMovement currentPlayer;
    private Collider currentPlayerCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (currentPlayer != null)
        {
            return;
        }

        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        currentPlayer = movement;
        currentPlayerCollider = other;
        currentPlayer.EnterFan(captureDuration);
    }


    private void OnTriggerStay(Collider other)
    {
        if (currentPlayer == null || other != currentPlayerCollider)
        {
            return;
        }

        if (!currentPlayer.IsFanCaptureComplete)
        {
            return;
        }

        float targetY = targetHeight.position.y;
        float currentY = currentPlayer.transform.position.y;
        float velocityY = currentPlayer.FanVerticalVelocity;
        float heightError = targetY - currentY;

        if (heightError <= stopThreshold)
        {
            currentPlayer.SetFanVerticalVelocity(0f);
            currentPlayer.EnableFanHorizontalControl();
            return;
        }

        // a = w^2(H - y) - 2wv
        float acceleration =
            response *
            response *
            heightError
            -
            2f *
            response *
            velocityY;

        acceleration = Mathf.Clamp(acceleration, -maxAcceleration,maxAcceleration);
        currentPlayer.AddFanVerticalAcceleration(acceleration);

        if (currentPlayer.FanVerticalVelocity < minimumRiseSpeed)
        {
            currentPlayer.SetFanVerticalVelocity(minimumRiseSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentPlayer == null || other != currentPlayerCollider)
        {
            return;
        }

        currentPlayer.ExitFan();
        currentPlayer = null;
        currentPlayerCollider = null;
    }
}