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

    private bool hasWon;

    private void Start()
    {
        Time.timeScale = 1f;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    // Chiamato dal punto di consegna (BodyDeliveryPoint) quando il livello va completato.
    public void CompleteLevel()
    {
        if (hasWon)
            return;

        hasWon = true;

        // Niente panel di vittoria: si va dritti alla cutscene.
        StartVictoryCutscene();
    }

    public void StartVictoryCutscene()
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

    // Mantenuto per compatibilità con eventuali bottoni UI già collegati.
    public void RestartGame()
    {
        StartVictoryCutscene();
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