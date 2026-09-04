using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private InputActionReference interactAction;

    [Header("Detection")]
    [SerializeField]
    private float interactionRange = 2f;
    [SerializeField]
    private LayerMask interactableLayer;

    private readonly Collider[] detectedColliders = new Collider[16];

    private Interactable currentTarget;
    private Interactable activeInteraction;

    public Interactable ActiveInteraction => activeInteraction;

    public Interactable DisplayTarget => activeInteraction != null ? activeInteraction : currentTarget;

    private void Awake()
    {
        Debug.Assert(interactAction != null);
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
    }

    private void Update()
    {
        FindTarget();
        HandleInteraction();
    }

    private void FindTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            interactionRange,
            detectedColliders,
            interactableLayer
        );

        Interactable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider detectedCollider = detectedColliders[i];
            Interactable interactable = detectedCollider.GetComponentInParent<Interactable>();

            if (interactable == null)
            {
                continue;
            }

            if (!interactable.CanInteract(this))
            {
                continue;
            }

            float distance = (interactable.transform.position - transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        currentTarget = closest;
    }

    private void HandleInteraction()
    {
        if (interactAction.action.WasPressedThisFrame())
        {
            if (currentTarget != null)
            {
                if (currentTarget.RequiresServerApproval)
                {
                    // 서버에 사용 요청만 보냄.
                    currentTarget.InteractStart(this);
                }
                else
                {
                    activeInteraction = currentTarget;
                    activeInteraction.InteractStart(this);
                }
            }
        }

        if (interactAction.action.IsPressed())
        {
            if (activeInteraction != null)
            {
                activeInteraction.InteractHold(this);
            }
        }

        if (interactAction.action.WasReleasedThisFrame())
        {
            if (activeInteraction != null)
            {
                activeInteraction.InteractEnd(this);
                activeInteraction = null;
            }
        }
    }

    public void SetActiveInteraction(Interactable interaction)
    {
        activeInteraction = interaction;
    }

    public void ClearActiveInteraction(Interactable interaction)
    {
        if (activeInteraction == interaction)
        {
            activeInteraction = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}