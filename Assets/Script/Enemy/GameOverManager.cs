using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Panels (indice = monster index del nemico)")]
    [SerializeField] private GameOverPanel[] panels;

    [Header("Scenes")]
    [SerializeField] private string menuSceneName = "MainMenu";

    private bool isGameOver;

    private void Start()
    {
        Time.timeScale = 1f;

        if (panels != null)
        {
            foreach (GameOverPanel panel in panels)
            {
                if (panel != null)
                    panel.Hide();
            }
        }
    }

    public void TriggerGameOver(int monsterIndex)
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panels != null && panels.Length > 0)
        {
            int index = Mathf.Clamp(monsterIndex, 0, panels.Length - 1);

            if (panels[index] != null)
                panels[index].Show();
        }
    }

    // 🔁 RICOMINCIA
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 🏠 TORNA AL MENÙ
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
