using Unity.Netcode;
using UnityEngine;

public class CameraModeTrigger : MonoBehaviour
{
    [SerializeField]
    private bool useSideView;

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();

        if (networkObject == null || !networkObject.IsOwner)
            return;

        PlayerCameraModeController controller = FindAnyObjectByType<PlayerCameraModeController>();

        if (controller == null)
            return;

        controller.SetSideView(useSideView);
    }
}