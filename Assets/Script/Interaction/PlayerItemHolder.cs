using UnityEngine;

public class PlayerItemHolder : MonoBehaviour
{
    public HoldableItemData CurrentItem { get; private set; }

    public bool HasItem => CurrentItem != null;

    public bool TrySetItem(HoldableItemData item)
    {
        if (HasItem || item == null)
            return false;

        CurrentItem = item;
        return true;
    }

    public void ClearItem()
    {
        CurrentItem = null;
    }

    public bool IsHolding(HoldableItemData item)
    {
        return CurrentItem == item;
    }
}