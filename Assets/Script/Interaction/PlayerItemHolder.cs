using UnityEngine;

public class PlayerItemHolder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float dropDistance = 1.2f;

    public HoldableItemData CurrentItemData { get; private set; }
    public PickupItem CurrentPickupItem { get; private set; }

    public bool HasItem => CurrentItemData != null && CurrentPickupItem != null;

    public bool TryPickItem(PickupItem pickupItem)
    {
        if (HasItem || pickupItem == null || pickupItem.ItemData == null || holdPoint == null)
            return false;

        CurrentItemData = pickupItem.ItemData;
        CurrentPickupItem = pickupItem;

        pickupItem.OnPickedUp(holdPoint);
        return true;
    }

    public void DropCurrentItem(Transform playerTransform)
    {
        if (!HasItem)
            return;

        Vector3 dropPosition = playerTransform.position + playerTransform.forward * dropDistance;
        dropPosition.y += 0.2f;

        CurrentPickupItem.OnDropped(dropPosition, Quaternion.identity);

        CurrentItemData = null;
        CurrentPickupItem = null;
    }

    public void ConsumeCurrentItem()
    {
        if (!HasItem)
            return;

        CurrentPickupItem.gameObject.SetActive(false);
        CurrentItemData = null;
        CurrentPickupItem = null;
    }

    public bool IsHolding(HoldableItemData itemData)
    {
        return CurrentItemData == itemData;
    }
}