using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField]
    private Image targetImage;

    [Header("Settings")]
    [SerializeField]
    private Color normalColor = Color.white;
    [SerializeField]
    private Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField]
    private float pressedScale = 0.94f;

    private Vector3 originalScale;

    private void Awake()
    {
        Debug.Assert(targetImage != null);

        originalScale = transform.localScale;
        targetImage.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetImage.color = pressedColor;
        transform.localScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetVisual();
    }

    private void ResetVisual()
    {
        targetImage.color = normalColor;
        transform.localScale = originalScale;
    }
}