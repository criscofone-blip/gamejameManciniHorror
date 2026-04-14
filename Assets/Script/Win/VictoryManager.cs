using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;

    private BodyPartCollectionManager collectionManager;
    private bool hasWon;

    private void Start()
    {
        Time.timeScale = 1f;

        collectionManager = BodyPartCollectionManager.Instance;

        if (collectionManager == null)
            collectionManager = FindFirstObjectByType<BodyPartCollectionManager>();

        if (collectionManager != null)
            collectionManager.OnAllBodyPartsCollected += HandleVictory;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (collectionManager != null)
            collectionManager.OnAllBodyPartsCollected -= HandleVictory;
    }

    private void HandleVictory()
    {
        if (hasWon)
            return;

        hasWon = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 🔁 RICOMINCIA
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 🚪 ESCI
    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();
    }
}