using UnityEngine;

public class HideHole : Interactable
{
    [Header("Hide")]
    [SerializeField]
    private Vector3 hidePointOffset = new Vector3(0f, 0.2f, 0f);

    [Header("Network")]
    [SerializeField]
    private int networkId;

    private MapActivity mapActivity;
    private bool occupied;

    public int NetworkId => networkId;
    public override bool RequiresServerApproval => true;

    private Vector3 HidePoint => transform.position + hidePointOffset;

    private void Awake()
    {
        mapActivity = GetComponentInParent<MapActivity>();
        Debug.Assert(mapActivity != null);
    }

    public override bool CanInteract(PlayerInteraction player)
    {
        return !occupied;
    }

    public override void InteractStart(PlayerInteraction player)
    {
        if (mapActivity.Manager == null)
            return;

        mapActivity.Manager.RequestHideHoleEnterServerRpc(mapActivity.MapIndex, networkId);
    }

    public override void InteractEnd(PlayerInteraction player)
    {
        if (mapActivity.Manager == null)
            return;

        mapActivity.Manager.RequestHideHoleExitServerRpc(mapActivity.MapIndex, networkId);
    }

    public void ApplyNetworkEnter(PlayerInteraction player)
    {
        occupied = true;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.EnterHide();
        }
    }

    public void ApplyNetworkExit(PlayerInteraction player)
    {
        occupied = false;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.ExitHide(HidePoint);
        }
    }

    public void InitializeNetworkId(int id)
    {
        networkId = id;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position + hidePointOffset, 0.15f);
    }
}