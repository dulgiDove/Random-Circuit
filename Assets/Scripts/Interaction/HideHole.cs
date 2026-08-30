using UnityEngine;

public class HideHole : Interactable
{
    [Header("Hide")]
    [SerializeField]
    private Vector3 hidePointOffset = new Vector3(0f, 0.2f, 0f);

    private Vector3 HidePoint => transform.position + hidePointOffset;

    public override void InteractStart(PlayerInteraction player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        movement.EnterHide();
    }

    public override void InteractEnd(PlayerInteraction player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        movement.ExitHide(HidePoint);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position + hidePointOffset, 0.15f);
    }
}