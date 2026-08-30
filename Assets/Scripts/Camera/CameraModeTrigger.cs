using UnityEngine;

public class CameraModeTrigger : MonoBehaviour
{
    [SerializeField]
    private bool useSideView = true;

    private PlayerCameraModeController cameraController;

    private void Awake()
    {
        cameraController = FindAnyObjectByType<PlayerCameraModeController>();

        if (cameraController == null)
        {
            return;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        if (cameraController == null)
        {
            return;
        }

        cameraController.SetSideView(useSideView);
    }
}