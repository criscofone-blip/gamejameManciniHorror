using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInGame : MonoBehaviour
{
    public static bool OpenedMenu { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference openMenuAction;

    [Header("UI")]
    [SerializeField] private GameObject MenuInGamePanel;

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
        if (openMenuAction.action.WasPressedThisFrame())
        {
            if(OpenedMenu)
            {
                OpenMenuInGame(false);
            }
            else
            {
                OpenMenuInGame(true);
            }

            CursorInMenu();
        }
          
    }

    public void OpenMenuInGame(bool opened)
    {
        OpenedMenu = opened;

        if (MenuInGamePanel != null)
            MenuInGamePanel.SetActive(opened);
    }

    public void CursorInMenu()
    {
        if (OpenedMenu)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            return;
        }
        
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        OpenMenuInGame(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();

        OpenMenuInGame(false);
    }


}
