using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NetworkDebugTeleport : NetworkBehaviour
{
    private CharacterController characterController;
    private MapManager mapManager;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Debug.Assert(characterController != null);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        BindMapManager();
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
        BindMapManager();
    }

    private void BindMapManager()
    {
        mapManager = FindAnyObjectByType<MapManager>();
    }

    private void Update()
    {
        if (!IsOwner || Keyboard.current == null)
            return;

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            TeleportNearGoal();
        }
    }

    private void TeleportNearGoal()
    {
        if (mapManager == null || mapManager.GoalTransform == null)
            return;

        Transform goal = mapManager.GoalTransform;

        Vector3 targetPosition = goal.position - goal.forward * 2f + Vector3.up * 1f;

        characterController.enabled = false;

        transform.SetPositionAndRotation(targetPosition, goal.rotation);

        characterController.enabled = true;
    }
}