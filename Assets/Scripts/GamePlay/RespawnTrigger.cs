using Unity.Netcode;
using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
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

        playerRespawn.Respawn();
    }
}