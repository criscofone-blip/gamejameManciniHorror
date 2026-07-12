using UnityEngine;

public class BodyPartPickup : MonoBehaviour, IInteractable
{
    [Header("Body Part")]
    [SerializeField] private BodyPartType bodyPartType;

    [Header("Prompt")]
    [SerializeField] private string pickupText = "Premi E per raccogliere";
    [SerializeField] private string alreadyCollectedText = "Gi� raccolto";

    [Header("Audio")]
    [Tooltip("Opzionale: se assegnata, sovrascrive la clip di default del PlayerPickupAudio.")]
    [SerializeField] private AudioClip pickupSound;

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
        {
            // Suona sull'AudioSource del player (sopravvive alla disattivazione del pezzo).
            if (PlayerPickupAudio.Instance != null)
                PlayerPickupAudio.Instance.PlayPickup(pickupSound);

            gameObject.SetActive(false);
        }
    }
}