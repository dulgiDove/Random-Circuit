using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerInteraction playerInteraction;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private CanvasGroup interactButtonCanvasGroup;
    [SerializeField]
    private RectTransform interactionPrompt;
    [SerializeField]
    private TMP_Text promptText;

    [Header("Settings")]
    [SerializeField]
    private float inactiveButtonAlpha = 0.25f;
    [SerializeField]
    private Vector2 promptOffset = new Vector2(0f, 50f);

    private bool previousCanInteract;
    private Interactable previousTarget;

    private void Awake()
    {
        Debug.Assert(mainCamera != null);
        Debug.Assert(interactButtonCanvasGroup != null);
        Debug.Assert(interactionPrompt != null);
        Debug.Assert(promptText != null);
    }

    private void Update()
    {
        if (playerInteraction == null)
            return;

        UpdateInteractButton();
    }

    // 프롬프트는 카메라가 먼저 업데이트 된 후에 프롬프트가 반영되어야 하므로 LateUpdate 사용.
    private void LateUpdate()
    {
        if (playerInteraction == null)
            return;

        UpdatePrompt();
    }

    public void Bind(PlayerInteraction interaction)
    {
        playerInteraction = interaction;
        previousTarget = null;
        previousCanInteract = false;

        SetInteractableState(false);
        interactionPrompt.gameObject.SetActive(false);
    }

    private void UpdateInteractButton()
    {
        bool canInteract = playerInteraction.DisplayTarget != null;

        if (canInteract == previousCanInteract)
            return;

        SetInteractableState(canInteract);
        previousCanInteract = canInteract;
    }

    private void SetInteractableState(bool canInteract)
    {
        interactButtonCanvasGroup.alpha = canInteract ? 1f : inactiveButtonAlpha;
        interactButtonCanvasGroup.interactable = canInteract;
        interactButtonCanvasGroup.blocksRaycasts = canInteract;
    }

    private void UpdatePrompt()
    {
        Interactable target = playerInteraction.DisplayTarget;

        if (target == null)
        {
            if (previousTarget != null)
            {
                interactionPrompt.gameObject.SetActive(false);
                previousTarget = null;
            }

            return;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.InteractionPoint);
        bool visible = screenPosition.z > 0f;

        if (!visible)
        {
            if (interactionPrompt.gameObject.activeSelf)
            {
                interactionPrompt.gameObject.SetActive(false);
            }
            
            return;
        }

        if (!interactionPrompt.gameObject.activeSelf)
        {
            interactionPrompt.gameObject.SetActive(true);
        }

        if (target != previousTarget)
        {
            promptText.text = target.PromptText;
            previousTarget = target;
        }

        interactionPrompt.position = (Vector2)screenPosition + promptOffset;
    }
}