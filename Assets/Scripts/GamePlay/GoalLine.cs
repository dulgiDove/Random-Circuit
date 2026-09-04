using Unity.Netcode;
using UnityEngine;

public class GoalLine : MonoBehaviour
{
    private bool localPlayerFinished;

    private void OnTriggerEnter(Collider other)
    {
        if (localPlayerFinished)
        {
            return;
        }

        NetworkObject playerNetworkObject = other.GetComponentInParent<NetworkObject>();

        if (playerNetworkObject == null || !playerNetworkObject.IsOwner)
        {
            return;
        }

        GameFlowManager gameFlowManager = FindAnyObjectByType<GameFlowManager>();

        if (gameFlowManager == null)
        {
            return;
        }

        localPlayerFinished = true;
        gameFlowManager.ReportFinish();
    }
}