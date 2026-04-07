using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private HoldableItemData itemData;

    [Header("Prompt")]
    [SerializeField] private string pickupText = "Premi E per raccogliere";

    public string GetInteractionText(PlayerItemHolder itemHolder)
    {
        if (itemHolder == null)
            return pickupText;

        if (itemHolder.HasItem)
            return "Hai già un oggetto in mano";

        return pickupText;
    }

    public void Interact(PlayerItemHolder itemHolder)
    {
        if (itemHolder == null || itemData == null)
            return;

        if (!itemHolder.TrySetItem(itemData))
            return;

        gameObject.SetActive(false);
    }
}