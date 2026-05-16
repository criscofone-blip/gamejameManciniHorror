using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "MainScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject loadPanel;

    [Header("Save Slots")]
    [SerializeField] private int maxSaveSlots = 4;
    [SerializeField] private Transform saveSlotsContainer;
    [SerializeField] private SaveSlotButton saveSlotButtonPrefab;

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CreateSaveSlots();
        OpenPanel(mainPanel);
    }

    private void CreateSaveSlots()
    {
        foreach (Transform child in saveSlotsContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < maxSaveSlots; i++)
        {
            SaveSlotButton slotButton = Instantiate(saveSlotButtonPrefab, saveSlotsContainer);
            slotButton.Setup(i, this);
        }
    }

    public void OpenLoadPanel()
    {
        CreateSaveSlots();
        OpenPanel(loadPanel);
    }

    public void NewGameOnSlot(int slot)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager non trovato nella scena. Aggiungi un GameObject con GameManager.cs.");
            return;
        }

        GameManager.Instance.StartNewGame(slot);
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadGameFromSlot(int slot)
    {
        if (!SaveManager.HasSave(slot))
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager non trovato nella scena. Aggiungi un GameObject con GameManager.cs.");
            return;
        }

        GameManager.Instance.LoadGame(slot);
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        OpenPanel(optionsPanel);
    }

    public void OpenCredits()
    {
        OpenPanel(creditsPanel);
    }

    public void OpenControls()
    {
        OpenPanel(controlsPanel);
    }

    public void BackToMain()
    {
        OpenPanel(mainPanel);
    }

    private void OpenPanel(GameObject targetPanel)
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        loadPanel.SetActive(false);

        targetPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}