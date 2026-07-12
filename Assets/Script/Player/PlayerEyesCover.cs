using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerEyesCover : MonoBehaviour
{
    public static bool EyesCovered { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference coverEyesAction;

    [Header("Eyelids (UI)")]
    [Tooltip("Palpebra superiore: pivot Y = 1, ancorata alla metà alta dello schermo.")]
    [SerializeField] private RectTransform topEyelid;
    [Tooltip("Palpebra inferiore: pivot Y = 0, ancorata alla metà bassa dello schermo.")]
    [SerializeField] private RectTransform bottomEyelid;

    [Header("Animation")]
    [SerializeField] private float closeDuration = 0.12f;
    [SerializeField] private float openDuration = 0.18f;

    [Header("Cooldown")]
    [Tooltip("Secondi di ricarica dopo aver riaperto gli occhi.")]
    [SerializeField] private float cooldownDuration = 10f;
    [Tooltip("Image (Filled / Radial 360) che si riempie durante la ricarica.")]
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color notReadyColor = Color.red;

    private float closeAmount;   // 0 = occhi aperti, 1 = occhi chiusi
    private Coroutine animRoutine;

    private float cooldownTimer;   // secondi rimanenti di ricarica (0 = pronto)
    private Coroutine flashRoutine;

    private void OnEnable()
    {
        coverEyesAction.action.Enable();
    }

    private void OnDisable()
    {
        coverEyesAction.action.Disable();
    }

    private void Start()
    {
        EyesCovered = false;
        closeAmount = 0f;
        cooldownTimer = 0f;
        ApplyCloseAmount(0f); // palpebre aperte (collassate ai bordi)

        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 1f;
            cooldownFillImage.color = readyColor;
        }
    }

    private void Update()
    {
        // A partita persa non si possono chiudere gli occhi: forzali aperti.
        if (GameOverManager.IsGameOver)
        {
            if (EyesCovered)
                SetEyesCovered(false);

            return;
        }

        // Scala la ricarica.
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f)
                cooldownTimer = 0f;
        }

        UpdateCooldownUI();

        if (coverEyesAction.action.WasPressedThisFrame())
        {
            if (cooldownTimer > 0f)
                FlashNotReady();          // ancora in ricarica → lampeggia rosso
            else if (!EyesCovered)
                SetEyesCovered(true);
        }

        if (coverEyesAction.action.WasReleasedThisFrame())
        {
            if (EyesCovered)
            {
                SetEyesCovered(false);
                cooldownTimer = cooldownDuration;   // avvia la ricarica al rilascio
            }
        }
    }

    private void UpdateCooldownUI()
    {
        if (cooldownFillImage == null)
            return;

        float fill = (cooldownDuration > 0f)
            ? 1f - (cooldownTimer / cooldownDuration)
            : 1f;

        cooldownFillImage.fillAmount = Mathf.Clamp01(fill);
    }

    private void FlashNotReady()
    {
        if (cooldownFillImage == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < 2; i++)
        {
            cooldownFillImage.color = notReadyColor;
            yield return new WaitForSecondsRealtime(0.1f);
            cooldownFillImage.color = readyColor;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        cooldownFillImage.color = readyColor;
    }

    private void SetEyesCovered(bool covered)
    {
        // Gameplay: effetto immediato (il nemico si congela senza ritardo).
        EyesCovered = covered;

        // Visivo: animazione palpebre.
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(AnimateEyelids(covered));
    }

    private IEnumerator AnimateEyelids(bool closing)
    {
        float target = closing ? 1f : 0f;
        float start = closeAmount;
        float duration = closing ? closeDuration : openDuration;

        if (duration <= 0f)
        {
            ApplyCloseAmount(target);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            // unscaledDeltaTime: l'animazione gira anche con Time.timeScale = 0.
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            ApplyCloseAmount(Mathf.Lerp(start, target, k));
            yield return null;
        }

        ApplyCloseAmount(target);
    }

    private void ApplyCloseAmount(float amount)
    {
        closeAmount = amount;

        if (topEyelid != null)
        {
            Vector3 s = topEyelid.localScale;
            s.y = amount;
            topEyelid.localScale = s;
        }

        if (bottomEyelid != null)
        {
            Vector3 s = bottomEyelid.localScale;
            s.y = amount;
            bottomEyelid.localScale = s;
        }
    }
}
