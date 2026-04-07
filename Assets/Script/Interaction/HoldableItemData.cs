using UnityEngine;

[CreateAssetMenu(fileName = "NewHoldableItem", menuName = "Game/Items/Holdable Item")]
public class HoldableItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public string itemDescription;
}