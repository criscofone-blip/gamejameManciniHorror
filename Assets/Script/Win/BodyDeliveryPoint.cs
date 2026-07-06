using UnityEngine;

public class BodyDeliveryPoint : MonoBehaviour, IInteractable
{
    [Header("References")]
    [Tooltip("Se vuoto, viene cercato automaticamente in scena.")]
    [SerializeField] private VictoryManager victoryManager;

    [Header("Prompt")]
    [SerializeField] private string readyText = "Premi E per deporre il cadavere";
    [SerializeField] private string missingPartsText = "Devi trovare tutti i pezzi";

    private void Awake()
    {
        if (victoryManager == null)
            victoryManager = FindFirstObjectByType<VictoryManager>();
    }

    public string GetInteractionText(PlayerItemHolder itemHolder)
    {
        return HasAllParts() ? readyText : missingPartsText;
    }

    public void Interact(PlayerItemHolder itemHolder)
    {
        if (!HasAllParts())
            return;

        if (victoryManager != null)
            victoryManager.CompleteLevel();
    }

    private bool HasAllParts()
    {
        BodyPartCollectionManager manager = BodyPartCollectionManager.Instance;

        if (manager == null)
            return false;

        return manager.CollectedCount >= manager.TotalParts;
    }
}
