public interface IItemUseTarget
{
    bool CanUseItem(HoldableItemData item);
    void UseItem(HoldableItemData item, PlayerItemHolder itemHolder);
}