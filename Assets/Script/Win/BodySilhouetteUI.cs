using UnityEngine;
using UnityEngine.UI;

public class BodySilhouetteUI : MonoBehaviour
{
    [Header("Body Part Images")]
    [SerializeField] private Image headImage;
    [SerializeField] private Image torsoImage;
    [SerializeField] private Image armsImage;
    [SerializeField] private Image legsImage;

    [Header("Colors")]
    [SerializeField] private Color missingColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color collectedColor = Color.white;

    private BodyPartCollectionManager collectionManager;

    private void Start()
    {
        collectionManager = BodyPartCollectionManager.Instance;

        if (collectionManager == null)
            collectionManager = FindFirstObjectByType<BodyPartCollectionManager>();

        if (collectionManager != null)
            collectionManager.OnBodyPartCollected += UpdateSinglePart;

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (collectionManager != null)
            collectionManager.OnBodyPartCollected -= UpdateSinglePart;
    }

    private void RefreshAll()
    {
        SetPartColor(BodyPartType.Head);
        SetPartColor(BodyPartType.Torso);
        SetPartColor(BodyPartType.Arms);
        SetPartColor(BodyPartType.Legs);
    }

    private void UpdateSinglePart(BodyPartType partType)
    {
        SetPartColor(partType);
    }

    private void SetPartColor(BodyPartType partType)
    {
        bool collected = collectionManager != null && collectionManager.HasCollected(partType);
        Color targetColor = collected ? collectedColor : missingColor;

        switch (partType)
        {
            case BodyPartType.Head:
                if (headImage != null) headImage.color = targetColor;
                break;

            case BodyPartType.Torso:
                if (torsoImage != null) torsoImage.color = targetColor;
                break;

            case BodyPartType.Arms:
                if (armsImage != null) armsImage.color = targetColor;
                break;

            case BodyPartType.Legs:
                if (legsImage != null) legsImage.color = targetColor;
                break;
        }
    }
}