using Unity.Netcode;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField]
    private Vector3 respawnOffset = new Vector3(0f, 0.2f, 0f);

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();

        if (networkObject == null || !networkObject.IsOwner)
            return;

        PlayerRespawn playerRespawn = other.GetComponentInParent<PlayerRespawn>();

        if (playerRespawn == null)
        {
            return;
        }

        Vector3 position = transform.position + respawnOffset;
        Quaternion rotation = Quaternion.identity;
        playerRespawn.SetCheckpoint(position, rotation);
    }
}