public interface IInteractable
{
    string GetInteractionText(PlayerItemHolder itemHolder);
    void Interact(PlayerItemHolder itemHolder);
}