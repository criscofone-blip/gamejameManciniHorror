using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private InputActionReference interactAction;

    [Header("UI")]
    [SerializeField] private GameObject interactUI;

    private IInteractable currentInteractable;

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
        CheckInteraction();

        if (interactAction.action.WasPressedThisFrame() && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void CheckInteraction()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;

                if (!interactUI.activeSelf)
                    interactUI.SetActive(true);

                return;
            }
        }

        currentInteractable = null;

        if (interactUI.activeSelf)
            interactUI.SetActive(false);
    }
}