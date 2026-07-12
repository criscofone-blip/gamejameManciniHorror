using System.Collections.Generic;
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
    [SerializeField] private string menuSceneName = "MainMenu";

    [Header("Victory Cutscenes (indice = numero nemici)")]
    [SerializeField] private VideoClip[] victoryCutscenes;

    [Header("Credits")]
    [Tooltip("Video dei credits, riprodotto (non skippabile) dopo l'ultima cutscene.")]
    [SerializeField] private VideoClip creditsClip;

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

        int currentEnemyCount = 0;

        if (GameManager.Instance != null)
            currentEnemyCount = GameManager.Instance.EnemyCount;

        int lastIndex = (victoryCutscenes != null && victoryCutscenes.Length > 0)
            ? victoryCutscenes.Length - 1
            : 0;

        int index = Mathf.Clamp(currentEnemyCount, 0, lastIndex);

        VideoClip selectedCutscene =
            (victoryCutscenes != null && victoryCutscenes.Length > 0)
                ? victoryCutscenes[index]
                : null;

        bool isFinalVictory =
            victoryCutscenes != null &&
            victoryCutscenes.Length > 0 &&
            currentEnemyCount >= lastIndex;

        if (isFinalVictory)
        {
            // Ultima cutscene (skippabile) + credits (NON skippabili) → poi menù, senza aumentare difficoltà.
            List<CutsceneRequest.Step> steps = new List<CutsceneRequest.Step>
            {
                new CutsceneRequest.Step(selectedCutscene, true),
                new CutsceneRequest.Step(creditsClip, false)
            };

            CutsceneRequest.SetSequence(steps, menuSceneName, false);
        }
        else
        {
            // Cutscene skippabile → aumenta difficoltà (+1 nemico) → torna al gioco.
            CutsceneRequest.Set(selectedCutscene, gameSceneName, true, true);
        }

        SceneManager.LoadScene(cutsceneSceneName);
    }

    // Mantenuto per compatibilità con eventuali bottoni UI già collegati.
    public void RestartGame()
    {
        StartVictoryCutscene();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
