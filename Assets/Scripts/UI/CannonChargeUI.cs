using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CannonChargeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerInteraction playerInteraction;
    [SerializeField]
    private GameObject chargeGauge;
    [SerializeField]
    private RectTransform fillRect;
    [SerializeField]
    private TMP_Text powerText;

    private void Awake()
    {
        Debug.Assert(chargeGauge != null);
        Debug.Assert(fillRect != null);
        Debug.Assert(powerText != null);
    }

    private void Start()
    {
        chargeGauge.SetActive(false);
    }

    private void Update()
    {
        if (playerInteraction == null)
            return;

        UpdateChargeGauge();
    }

    public void Bind(PlayerInteraction interaction)
    {
        playerInteraction = interaction;
    }

    private void UpdateChargeGauge()
    {
        //Interactable이 cannon만 있는게 아니기 때문에 형변환할 때 체크
        Cannon cannon = playerInteraction.ActiveInteraction as Cannon;

        if (cannon == null)
        {
            chargeGauge.SetActive(false);
            return;
        }

        chargeGauge.SetActive(true);
        float chargeRatio = cannon.ChargeRatio;
        Vector2 anchorMax = fillRect.anchorMax;
        anchorMax.x = chargeRatio;
        fillRect.anchorMax = anchorMax;
        powerText.text = $"POWER\n {chargeRatio * 100f:0}%";
    }
}