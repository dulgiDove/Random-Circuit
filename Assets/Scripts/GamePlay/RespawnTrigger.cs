using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerRespawn playerRespawn = other.GetComponentInParent<PlayerRespawn>();

        if (playerRespawn == null)
        {
            return;
        }

        playerRespawn.Respawn();
    }
}