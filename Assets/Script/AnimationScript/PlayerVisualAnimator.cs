using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVisualAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputActionReference moveAction;

    [Header("Animation")]
    [SerializeField] private float movementThreshold = 0.1f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }

    private void Update()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        float moveAmount = moveInput.magnitude;

        // Se il player si muove, l'animazione va.
        // Se è fermo, l'animazione si blocca.
        animator.speed = moveAmount > movementThreshold ? 1f : 0f;
    }
}