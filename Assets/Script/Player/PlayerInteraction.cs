using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionPromptUI interactionPromptUI;
    [SerializeField] private PlayerItemHolder itemHolder;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference dropItemAction;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionMask = ~0;

    private IInteractable currentInteractable;

    private void OnEnable()
    {
        interactAction.action.Enable();
        dropItemAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
        dropItemAction.action.Disable();
    }

    private void Update()
    {
        DetectInteractable();
        HandleInteractionInput();
        HandleDropInput();
    }

    private void DetectInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore))
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                interactionPromptUI.Show(currentInteractable.GetInteractionText(itemHolder));
                return;
            }
        }

        interactionPromptUI.Hide();
    }

    private void HandleInteractionInput()
    {
        if (currentInteractable == null)
            return;

        if (interactAction.action.WasPressedThisFrame())
            currentInteractable.Interact(itemHolder);
    }

    private void HandleDropInput()
    {
        if (itemHolder == null || !itemHolder.HasItem)
            return;

        if (dropItemAction.action.WasPressedThisFrame())
            itemHolder.DropCurrentItem(transform);
    }
}