using Unity.Netcode;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private PlayerMovement playerMovement;
    [SerializeField]
    private Transform visualRoot;

    private Vector3 respawnPosition;
    private Quaternion respawnRotation;

    private NetworkObject networkObject;

    private void Awake()
    {
        Debug.Assert(characterController != null);
        Debug.Assert(playerMovement != null);
        Debug.Assert(visualRoot != null);

        networkObject = GetComponent<NetworkObject>();

        Debug.Assert(networkObject != null);
    }

    private void Start()
    {
        respawnPosition = transform.position;
        respawnRotation = visualRoot.rotation;
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        if (!networkObject.IsOwner)
            return;

        respawnPosition = position;
        respawnRotation = rotation;
    }

    public void Respawn()
    {
        if (!networkObject.IsOwner)
            return;

        GameRecords.RecordRespawn();

        playerMovement.ResetForRespawn();

        characterController.enabled = false;
        transform.position = respawnPosition;
        characterController.enabled = true;

        visualRoot.rotation = respawnRotation;
    }
}