using Unity.Netcode;
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

    private void Awake()
    {
        Debug.Assert(mapActivity != null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        NetworkObject playerNetworkObject = other.GetComponentInParent<NetworkObject>();

        if (playerNetworkObject == null || !playerNetworkObject.IsOwner)
            return;

        triggered = true;

        if (triggerType == TriggerType.Enter)
        {
            mapActivity.RequestEnter();
        }
        else
        {
            mapActivity.RequestExit();
        }
    }
}