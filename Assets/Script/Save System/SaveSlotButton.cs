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
            slotText.text = $"Nemici: {NumberToWords(enemyCount)}";
        }
        else
        {
            slotText.text = "Slot Vuoto";
        }

        loadButton.gameObject.SetActive(hasSave);
        deleteButton.gameObject.SetActive(hasSave);
        newGameButton.gameObject.SetActive(!hasSave);
    }

    // Converte il numero di nemici in parole (il font usato non ha le cifre).
    private string NumberToWords(int number)
    {
        string[] words =
        {
            "zero", "uno", "due", "tre", "quattro",
            "cinque", "sei", "sette", "otto", "nove", "dieci"
        };

        if (number >= 0 && number < words.Length)
            return words[number];

        return number.ToString();
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