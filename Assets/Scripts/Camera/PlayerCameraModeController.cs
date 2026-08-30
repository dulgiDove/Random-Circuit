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
        if (thirdPersonCamera != null)
        {
            thirdPersonCinemachineCamera = thirdPersonCamera.GetComponent<CinemachineCamera>();
        }

        if (sideViewCamera != null)
        {
            sideViewCinemachineCamera = sideViewCamera.GetComponent<CinemachineCamera>();
        }
    }


    public void SetSideView(bool enabled)
    {
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.SetActive(!enabled);
        }

        if (sideViewCamera != null)
        {
            sideViewCamera.SetActive(enabled);
        }
    }


    public void SetFarClip(float distance)
    {
        SetCameraFarClip(thirdPersonCinemachineCamera, distance);
        SetCameraFarClip(sideViewCinemachineCamera, distance);
    }


    private void SetCameraFarClip(CinemachineCamera camera,float distance)
    {
        if (camera == null)
        {
            return;
        }

        var lens = camera.Lens;
        lens.FarClipPlane = distance;
        camera.Lens = lens;
    }
}