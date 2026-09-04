using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerMovement playerMovement;
    [SerializeField]
    private Image cooldownFill;
    [SerializeField]
    private Image readyGlowImage;

    [Header("Ready Effect")]
    [SerializeField]
    private float glowDuration = 0.25f;
    [SerializeField]
    private float glowStartScale = 0.9f;
    [SerializeField]
    private float glowEndScale = 1.35f;

    private float previousProgress = 1f;
    private bool dashDisabled;
    private Coroutine glowCoroutine;

    private void Awake()
    {
        Debug.Assert(cooldownFill != null);
        Debug.Assert(readyGlowImage != null);

        SetReadyGlowAlpha(0f);
    }

    private void OnEnable()
    {
        if (playerMovement != null)
        {
            playerMovement.MovementModeChanged += OnMovementModeChanged;
        }
    }

    private void OnDisable()
    {
        if (playerMovement != null)
        {
            playerMovement.MovementModeChanged -= OnMovementModeChanged;
        }
    }

    private void Update()
    {
        if (playerMovement == null)
            return;

        UpdateCooldown();
    }

    public void Bind(PlayerMovement movement)
    {
        if (playerMovement != null)
        {
            playerMovement.MovementModeChanged -= OnMovementModeChanged;
        }

        playerMovement = movement;

        if (playerMovement == null)
            return;

        playerMovement.MovementModeChanged += OnMovementModeChanged;

        previousProgress = playerMovement.DashCooldownProgress;
        UpdateDashMode(playerMovement.MovementMode);

        cooldownFill.fillAmount = previousProgress;
    }

    private void OnMovementModeChanged(PlayerMovementMode mode)
    {
        UpdateDashMode(mode);
    }

    private void UpdateDashMode(PlayerMovementMode mode)
    {
        dashDisabled = mode == PlayerMovementMode.JumpKing || mode == PlayerMovementMode.Dropper;

        cooldownFill.gameObject.SetActive(!dashDisabled);

        if (dashDisabled)
        {
            StopReadyEffect();
        }
    }

    private void UpdateCooldown()
    {
        float progress = playerMovement.DashCooldownProgress;

        cooldownFill.fillAmount = progress;

        if (!dashDisabled && previousProgress < 1f && progress >= 1f)
        {
            PlayReadyEffect();
        }

        previousProgress = progress;
    }

    private void PlayReadyEffect()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }

        glowCoroutine = StartCoroutine(ReadyGlowRoutine());
    }

    private void StopReadyEffect()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        SetReadyGlowAlpha(0f);
        readyGlowImage.rectTransform.localScale = Vector3.one;
    }

    private IEnumerator ReadyGlowRoutine()
    {
        float time = 0f;
        SetReadyGlowAlpha(1f);
        readyGlowImage.rectTransform.localScale = Vector3.one * glowStartScale;

        while (time < glowDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / glowDuration);
            float scale = Mathf.Lerp(glowStartScale, glowEndScale, t);
            
            readyGlowImage.rectTransform.localScale = Vector3.one *scale;
            SetReadyGlowAlpha(1f - t);
            yield return null;
        }

        SetReadyGlowAlpha(0f);
        readyGlowImage.rectTransform.localScale = Vector3.one;
        glowCoroutine = null;
    }

    private void SetReadyGlowAlpha(float alpha)
    {
        Color color =readyGlowImage.color;
        color.a = alpha;
        readyGlowImage.color = color;
    }
}