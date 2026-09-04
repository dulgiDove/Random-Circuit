using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraModeController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField]
    private GameObject thirdPersonCamera;
    [SerializeField]
    private GameObject sideViewCamera;

    private CinemachineCamera thirdPersonCinemachineCamera;
    private CinemachineCamera sideViewCinemachineCamera;

    private void Awake()
    {
        Debug.Assert(thirdPersonCamera != null);
        Debug.Assert(sideViewCamera != null);

        thirdPersonCinemachineCamera = thirdPersonCamera.GetComponent<CinemachineCamera>();
        sideViewCinemachineCamera = sideViewCamera.GetComponent<CinemachineCamera>();
    }

    public void SetSideView(bool enabled)
    {
        thirdPersonCamera.SetActive(!enabled);
        sideViewCamera.SetActive(enabled);
    }

    public void SetFarClip(float distance)
    {
        SetCameraFarClip(thirdPersonCinemachineCamera, distance);
        SetCameraFarClip(sideViewCinemachineCamera, distance);
    }

    private void SetCameraFarClip(CinemachineCamera camera,float distance)
    {
        var lens = camera.Lens;
        lens.FarClipPlane = distance;
        camera.Lens = lens;
    }
}