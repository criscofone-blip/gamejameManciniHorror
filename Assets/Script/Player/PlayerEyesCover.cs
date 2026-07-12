using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private float closeAmount;   // 0 = occhi aperti, 1 = occhi chiusi
    private Coroutine animRoutine;

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
        ApplyCloseAmount(0f); // palpebre aperte (collassate ai bordi)
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

        if (coverEyesAction.action.WasPressedThisFrame())
            SetEyesCovered(true);

        if (coverEyesAction.action.WasReleasedThisFrame())
            SetEyesCovered(false);
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
