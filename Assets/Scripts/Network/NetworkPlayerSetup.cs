using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayerSetup : NetworkBehaviour
{
    [Header("Local Player Components")]
    [SerializeField]
    private PlayerMovement playerMovement;
    [SerializeField]
    private PlayerInteraction playerInteraction;
    [SerializeField]
    private PlayerRespawn playerRespawn;
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private PlayerAnimation playerAnimation;

    [Header("Camera")]
    [SerializeField]
    private Transform cameraTarget;

    private void Awake()
    {
        Debug.Assert(playerMovement != null);
        Debug.Assert(playerInteraction != null);
        Debug.Assert(playerRespawn != null);
        Debug.Assert(characterController != null);
        Debug.Assert(playerAnimation != null);
        Debug.Assert(cameraTarget != null);
    }

    public override void OnNetworkSpawn()
    {
        bool isLocalPlayer = IsOwner;

        playerMovement.enabled = isLocalPlayer;
        playerInteraction.enabled = isLocalPlayer;
        playerRespawn.enabled = isLocalPlayer;
        characterController.enabled = isLocalPlayer;
        playerAnimation.enabled = isLocalPlayer;

        if (!IsOwner)
            return;

        playerMovement.SetControlsLocked(true);

        SceneManager.sceneLoaded += OnSceneLoaded;

        BindSceneReferences();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSceneReferences();
    }

    private void BindSceneReferences()
    {
        if (Camera.main != null)
        {
            playerMovement.SetCameraTransform(Camera.main.transform);
        }

        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CinemachineCamera cam in cameras)
        {
            cam.Target.TrackingTarget = cameraTarget;
        }
    }
}