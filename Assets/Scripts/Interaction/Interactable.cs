using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private string promptText = "Interact";
    [SerializeField]
    private Vector3 interactionPointOffset = new Vector3(0f, 1f, 0f);

    public virtual bool RequiresServerApproval => false;
    public string PromptText => promptText;

    public Vector3 InteractionPoint
    {
        get { return transform.position + interactionPointOffset; }
    }

    public virtual bool CanInteract(PlayerInteraction player)
    {
        return true;
    }

    public virtual void InteractStart(PlayerInteraction player)
    {
    }

    public virtual void InteractHold(PlayerInteraction player)
    {
    }

    public virtual void InteractEnd(PlayerInteraction player)
    {
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position + interactionPointOffset,0.08f);
    }
}