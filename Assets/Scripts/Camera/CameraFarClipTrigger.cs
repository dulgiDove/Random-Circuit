using UnityEngine;

public class CameraFarClipTrigger : MonoBehaviour
{
    [SerializeField]
    private float farClipDistance = 50f;

    private PlayerCameraModeController cameraController;

    private void Awake()
    {
        cameraController = FindAnyObjectByType<PlayerCameraModeController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null)
        {
            return;
        }

        if (cameraController == null)
        {
            return;
        }

        cameraController.SetFarClip(farClipDistance);
    }
}