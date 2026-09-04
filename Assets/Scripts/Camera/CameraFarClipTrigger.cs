using Unity.Netcode;
using UnityEngine;

public class CameraFarClipTrigger : MonoBehaviour
{
    [SerializeField]
    private float farClipDistance = 50f;

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();

        if (networkObject == null || !networkObject.IsOwner)
            return;

        PlayerCameraModeController controller = FindAnyObjectByType<PlayerCameraModeController>();

        if (controller == null)
            return;

        controller.SetFarClip(farClipDistance);
    }
}