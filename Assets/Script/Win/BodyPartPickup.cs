using UnityEngine;

public class BodyPartPickup : MonoBehaviour, IInteractable
{
    [Header("Body Part")]
    [SerializeField] private BodyPartType bodyPartType;

    [Header("Prompt")]
    [SerializeField] private string pickupText = "Premi E per raccogliere";
    [SerializeField] private string alreadyCollectedText = "Già raccolto";

    public string GetInteractionText(PlayerItemHolder itemHolder)
    {
        if (BodyPartCollectionManager.Instance == null)
            return pickupText;

        if (BodyPartCollectionManager.Instance.HasCollected(bodyPartType))
            return alreadyCollectedText;

        return pickupText;
    }

    public void Interact(PlayerItemHolder itemHolder)
    {
        if (BodyPartCollectionManager.Instance == null)
            return;

        bool collected = BodyPartCollectionManager.Instance.TryCollectPart(bodyPartType);

        if (collected)
            gameObject.SetActive(false);
    }
}