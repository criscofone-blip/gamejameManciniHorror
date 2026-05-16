using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button deleteButton;

    private int slotIndex;
    private MainMenuManager menuManager;

    public void Setup(int index, MainMenuManager manager)
    {
        slotIndex = index;
        menuManager = manager;

        Refresh();
    }

    public void Refresh()
    {
        bool hasSave = SaveManager.HasSave(slotIndex);

        if (hasSave)
        {
            int enemyCount = SaveManager.LoadEnemyCount(slotIndex);
            slotText.text = $"Slot {slotIndex + 1} - Nemici: {enemyCount}";
        }
        else
        {
            slotText.text = $"Slot {slotIndex + 1} - Vuoto";
        }

        loadButton.gameObject.SetActive(hasSave);
        deleteButton.gameObject.SetActive(hasSave);
        newGameButton.gameObject.SetActive(!hasSave);
    }

    public void LoadSlot()
    {
        menuManager.LoadGameFromSlot(slotIndex);
    }

    public void NewGameSlot()
    {
        menuManager.NewGameOnSlot(slotIndex);
    }

    public void DeleteSlot()
    {
        SaveManager.DeleteSave(slotIndex);
        Refresh();
    }
}