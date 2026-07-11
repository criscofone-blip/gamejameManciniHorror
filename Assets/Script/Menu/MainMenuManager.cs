using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private string cutsceneSceneName = "CutsceneScene";
    [SerializeField] private VideoClip introVideoClip;

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

    [Header("Background Video")]
    [SerializeField] private VideoPlayer menuVideoPlayer;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private AudioSource videoAudioSource;

    [Header("Buttons Intro")]
    [Tooltip("Gruppo dei bottoni del menù principale che appare dopo il ritardo.")]
    [SerializeField] private GameObject mainButtonsContainer;
    [Tooltip("Secondi prima che compaiano i tasti del menù principale.")]
    [SerializeField] private float buttonsDelay = 5f;

    [Header("Credits Video")]
    [SerializeField] private VideoPlayer creditsVideoPlayer;
    [SerializeField] private RawImage creditsVideoImage;
    [SerializeField] private AudioSource creditsAudioSource;

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartBackgroundVideo();
        CreateSaveSlots();
        OpenPanel(mainPanel);

        // Intro una tantum: nascondi i tasti del menù principale e mostrali dopo il ritardo.
        if (mainButtonsContainer != null)
        {
            mainButtonsContainer.SetActive(false);
            StartCoroutine(RevealButtonsAfterDelay());
        }
    }

    private void StartBackgroundVideo()
    {
        if (menuVideoPlayer == null)
            return;

        menuVideoPlayer.playOnAwake = false;
        menuVideoPlayer.isLooping = true;

        if (videoAudioSource != null)
        {
            menuVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            menuVideoPlayer.SetTargetAudioSource(0, videoAudioSource);
        }

        menuVideoPlayer.prepareCompleted -= OnVideoPrepared;
        menuVideoPlayer.prepareCompleted += OnVideoPrepared;

        // Se il video è già pronto (ritorno dal gioco, scena "calda"),
        // l'evento potrebbe essere già scattato: assegna subito la texture.
        if (menuVideoPlayer.isPrepared)
            OnVideoPrepared(menuVideoPlayer);
        else
            menuVideoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // In modalità Render Texture la RawImage mostra già la Render Texture:
        // NON assegniamo vp.texture (sarebbe null e coprirebbe di bianco).
        vp.Play();

        if (videoAudioSource != null)
            videoAudioSource.Play();
    }

    private IEnumerator RevealButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(buttonsDelay);

        if (mainButtonsContainer != null)
            mainButtonsContainer.SetActive(true);
    }

    private void OnDestroy()
    {
        if (menuVideoPlayer != null)
            menuVideoPlayer.prepareCompleted -= OnVideoPrepared;

        if (creditsVideoPlayer != null)
        {
            creditsVideoPlayer.prepareCompleted -= OnCreditsPrepared;
            creditsVideoPlayer.loopPointReached -= OnCreditsFinished;
        }
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

        CutsceneRequest.Set(
            introVideoClip,
            gameSceneName,
            false
        );

        SceneManager.LoadScene(cutsceneSceneName);
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
        PlayCreditsVideo();
    }

    private void PlayCreditsVideo()
    {
        if (creditsVideoPlayer == null)
            return;

        creditsVideoPlayer.isLooping = false;

        if (creditsAudioSource != null)
        {
            creditsVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            creditsVideoPlayer.SetTargetAudioSource(0, creditsAudioSource);
        }

        creditsVideoPlayer.prepareCompleted -= OnCreditsPrepared;
        creditsVideoPlayer.prepareCompleted += OnCreditsPrepared;
        creditsVideoPlayer.loopPointReached -= OnCreditsFinished;
        creditsVideoPlayer.loopPointReached += OnCreditsFinished;

        creditsVideoPlayer.Prepare();
    }

    private void OnCreditsPrepared(VideoPlayer vp)
    {
        if (creditsVideoImage != null)
            creditsVideoImage.texture = vp.texture;

        vp.Play();

        if (creditsAudioSource != null)
            creditsAudioSource.Play();
    }

    private void OnCreditsFinished(VideoPlayer vp)
    {
        // Fine dei crediti → torna al menù principale (StopCreditsVideo pulisce tutto).
        BackToMain();
    }

    private void StopCreditsVideo()
    {
        if (creditsVideoPlayer == null)
            return;

        creditsVideoPlayer.prepareCompleted -= OnCreditsPrepared;
        creditsVideoPlayer.loopPointReached -= OnCreditsFinished;
        creditsVideoPlayer.Stop();
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
        // Se stiamo lasciando i crediti, ferma il relativo video.
        if (targetPanel != creditsPanel)
            StopCreditsVideo();

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