using UnityEngine;

public class GoalLine : MonoBehaviour
{
    private GameFlowManager gameFlowManager;
    private bool finished;

    private void Awake()
    {
        gameFlowManager = FindAnyObjectByType<GameFlowManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished)
        {
            return;
        }

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        finished = true;

        if (gameFlowManager != null)
        {
            gameFlowManager.FinishGame();
        }
    }
}