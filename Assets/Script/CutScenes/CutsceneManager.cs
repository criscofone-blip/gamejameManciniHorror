using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private AudioSource audioSource;

    [Header("UI")]
    [Tooltip("Bottone di skip: mostrato solo negli step skippabili.")]
    [SerializeField] private GameObject skipButton;

    [Header("Fallback")]
    [SerializeField] private string defaultNextSceneName = "MainScene";

    private int currentStepIndex;

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentStepIndex = 0;
        PlayCurrentStep();
    }

    private void PlayCurrentStep()
    {
        var steps = CutsceneRequest.Steps;

        if (steps == null || currentStepIndex >= steps.Count)
        {
            EndCutscenes();
            return;
        }

        CutsceneRequest.Step step = steps[currentStepIndex];

        // Il tasto skip appare solo se questo step è skippabile (i credits no).
        if (skipButton != null)
            skipButton.SetActive(step.skippable);

        if (step.clip == null)
        {
            OnStepEnded();
            return;
        }

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = step.clip;
        videoPlayer.isLooping = false;

        if (audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (videoImage != null)
            videoImage.texture = source.texture;

        source.Play();

        if (audioSource != null)
            audioSource.Play();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        OnStepEnded();
    }

    // Collegato al bottone Skip: salta solo se lo step corrente è skippabile.
    public void Skip()
    {
        var steps = CutsceneRequest.Steps;

        if (steps == null || currentStepIndex >= steps.Count)
            return;

        if (!steps[currentStepIndex].skippable)
            return;

        OnStepEnded();
    }

    private void OnStepEnded()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.Stop();

        currentStepIndex++;
        PlayCurrentStep();
    }

    private void EndCutscenes()
    {
        if (CutsceneRequest.IncreaseDifficultyAfterCutscene && GameManager.Instance != null)
            GameManager.Instance.IncreaseDifficultyAndSave();

        string nextScene = string.IsNullOrEmpty(CutsceneRequest.NextSceneName)
            ? defaultNextSceneName
            : CutsceneRequest.NextSceneName;

        CutsceneRequest.Clear();
        SceneManager.LoadScene(nextScene);
    }
}
