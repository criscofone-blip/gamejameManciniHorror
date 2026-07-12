using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuInGame : MonoBehaviour
{
    public static bool OpenedMenu { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference openMenuAction;

    [Header("UI")]
    [SerializeField] private GameObject MenuInGamePanel;
    [SerializeField] private GameObject comandiPanel;

    [Header("Scenes")]
    [SerializeField] private string menuSceneName = "MainMenu";

    private void OnEnable()
    {
        openMenuAction.action.Enable();
    }

    private void OnDisable()
    {
        openMenuAction.action.Disable();
    }

    private void Start()
    {
        OpenMenuInGame(false);
    }

    private void Update()
    {
        // A partita persa il menù non si può aprire.
        if (GameOverManager.IsGameOver)
            return;

        if (openMenuAction.action.WasPressedThisFrame())
        {
            if (OpenedMenu)
            {
                OpenMenuInGame(false);
            }
            else
            {
                OpenMenuInGame(true);
                CursorInMenu();
            }
        }
    }

    public void OpenMenuInGame(bool opened)
    {
        OpenedMenu = opened;

        if (MenuInGamePanel != null)
            MenuInGamePanel.SetActive(opened);

        // Il pannello Comandi parte sempre chiuso.
        if (comandiPanel != null)
            comandiPanel.SetActive(false);

        if (!OpenedMenu)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CursorInMenu()
    {
        if (OpenedMenu)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ▶️ CONTINUA – riprende il gioco
    public void Continua()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OpenMenuInGame(false);
    }

    // 🔁 RICOMINCIA – ricarica il livello corrente
    public void Ricomincia()
    {
        Time.timeScale = 1f;
        OpenMenuInGame(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 🎮 COMANDI – apre il pannello con l'immagine dei comandi
    public void ApriComandi()
    {
        if (comandiPanel != null)
            comandiPanel.SetActive(true);
    }

    // ↩️ Indietro dal pannello Comandi
    public void ChiudiComandi()
    {
        if (comandiPanel != null)
            comandiPanel.SetActive(false);
    }

    // 🏠 ESCI – torna al menù principale
    public void Esci()
    {
        Time.timeScale = 1f;
        OpenMenuInGame(false);
        SceneManager.LoadScene(menuSceneName);
    }
}
