using UnityEngine;

public class MapActivityTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        Enter,
        Exit
    }

    [SerializeField]
    private TriggerType triggerType;
    [SerializeField]
    private MapActivity mapActivity;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
        {
            return;
        }

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        triggered = true;

        if (triggerType == TriggerType.Enter)
        {
            mapActivity.EnterPlayer();
        }
        else
        {
            mapActivity.ExitPlayer();
        }
    }
}