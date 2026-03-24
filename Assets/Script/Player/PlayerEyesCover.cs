using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEyesCover : MonoBehaviour
{
    public static bool EyesCovered { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference coverEyesAction;

    [Header("UI")]
    [SerializeField] private GameObject eyesCoveredPanel;

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
        SetEyesCovered(false);
    }

    private void Update()
    {
        if (coverEyesAction.action.WasPressedThisFrame())
            SetEyesCovered(true);

        if (coverEyesAction.action.WasReleasedThisFrame())
            SetEyesCovered(false);
    }

    private void SetEyesCovered(bool covered)
    {
        EyesCovered = covered;

        if (eyesCoveredPanel != null)
            eyesCoveredPanel.SetActive(covered);
    }
}