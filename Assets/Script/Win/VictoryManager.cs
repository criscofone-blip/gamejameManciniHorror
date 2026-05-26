using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VictoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Scenes")]
    [SerializeField] private string cutsceneSceneName = "CutsceneScene";
    [SerializeField] private string gameSceneName = "MainScene";

    [Header("Victory Cutscenes")]
    [SerializeField] private VideoClip[] victoryCutscenes;

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

    public void RestartGame()
    {
        Time.timeScale = 1f;

        VideoClip selectedCutscene = GetVictoryCutscene();

        CutsceneRequest.Set(
            selectedCutscene,
            gameSceneName,
            true
        );

        SceneManager.LoadScene(cutsceneSceneName);
    }

    private VideoClip GetVictoryCutscene()
    {
        if (victoryCutscenes == null || victoryCutscenes.Length == 0)
            return null;

        int currentEnemyCount = 0;

        if (GameManager.Instance != null)
            currentEnemyCount = GameManager.Instance.EnemyCount;

        int victoryIndex = currentEnemyCount;

        victoryIndex = Mathf.Clamp(victoryIndex, 0, victoryCutscenes.Length - 1);

        return victoryCutscenes[victoryIndex];
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}