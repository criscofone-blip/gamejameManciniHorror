using UnityEngine;

[RequireComponent(typeof(HeldItem))]
public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private HoldableItemData itemData;

    [Header("Prompt")]
    [SerializeField] private string pickupText = "Premi E per raccogliere";
    [SerializeField] private string alreadyHoldingText = "Hai già un oggetto in mano";

    private HeldItem heldItem;

    public HoldableItemData ItemData => itemData;

    private void Awake()
    {
        heldItem = GetComponent<HeldItem>();
    }

    public string GetInteractionText(PlayerItemHolder itemHolder)
    {
        if (itemHolder == null)
            return pickupText;

        if (itemHolder.HasItem)
            return alreadyHoldingText;

        return pickupText;
    }

    public void Interact(PlayerItemHolder itemHolder)
    {
        if (itemHolder == null)
            return;

        itemHolder.TryPickItem(this);
    }

    public void OnPickedUp(Transform holdPoint)
    {
        heldItem.OnPickedUp(holdPoint);
    }

    public void OnDropped(Vector3 position, Quaternion rotation)
    {
        heldItem.OnDropped(position, rotation);
    }
}