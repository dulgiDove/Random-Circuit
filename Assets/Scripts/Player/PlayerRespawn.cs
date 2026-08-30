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

    private void Start()
    {
        respawnPosition = transform.position;
        respawnRotation = visualRoot.rotation;
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        respawnPosition = position;
        respawnRotation = rotation;
    }

    public void Respawn()
    {
        GameRecords.RecordRespawn();
        playerMovement.ResetForRespawn();
        characterController.enabled = false;
        transform.position = respawnPosition;
        characterController.enabled = true;
        visualRoot.rotation = respawnRotation;
    }
}