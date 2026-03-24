using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;

    private bool isGameOver;

    private void Start()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}