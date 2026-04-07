using UnityEngine;

public class LiftActivator : MonoBehaviour, IInteractable, IItemUseTarget
{
    [Header("Required Item")]
    [SerializeField] private HoldableItemData requiredItem;

    [Header("Prompt")]
    [SerializeField] private string missingItemText = "Serve una leva";
    [SerializeField] private string useItemText = "Premi E per inserire la leva";
    [SerializeField] private string alreadyActivatedText = "Il montacarichi è già attivo";

    [Header("Lift")]
    [SerializeField] private GameObject liftToActivate;

    private bool isActivated;

    public string GetInteractionText(PlayerItemHolder itemHolder)
    {
        if (isActivated)
            return alreadyActivatedText;

        if (itemHolder == null || !itemHolder.HasItem)
            return missingItemText;

        if (CanUseItem(itemHolder.CurrentItemData))
            return useItemText;

        return missingItemText;
    }

    public void Interact(PlayerItemHolder itemHolder)
    {
        if (isActivated)
            return;

        if (itemHolder == null || !itemHolder.HasItem)
            return;

        if (!CanUseItem(itemHolder.CurrentItemData))
            return;

        UseItem(itemHolder.CurrentItemData, itemHolder);
    }

    public bool CanUseItem(HoldableItemData item)
    {
        return item == requiredItem;
    }

    public void UseItem(HoldableItemData item, PlayerItemHolder itemHolder)
    {
        isActivated = true;

        if (liftToActivate != null)
            liftToActivate.SetActive(true);

        itemHolder.ConsumeCurrentItem();
    }
}