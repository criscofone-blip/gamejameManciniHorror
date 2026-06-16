using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class CutsceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private AudioSource audioSource;

    [Header("Fallback")]
    [SerializeField] private string defaultNextSceneName = "MainScene";

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayRequestedCutscene();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            FinishCutscene();
        }
    }

    private void PlayRequestedCutscene()
    {
        VideoClip clip = CutsceneRequest.VideoClip;

        if (clip == null)
        {
            FinishCutscene();
            return;
        }

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        videoImage.texture = source.texture;
        source.Play();

        if (audioSource != null)
            audioSource.Play();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        FinishCutscene();
    }

    private void FinishCutscene()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;

        if (CutsceneRequest.IncreaseDifficultyAfterCutscene && GameManager.Instance != null)
            GameManager.Instance.IncreaseDifficultyAndSave();

        string nextScene = string.IsNullOrEmpty(CutsceneRequest.NextSceneName)
            ? defaultNextSceneName
            : CutsceneRequest.NextSceneName;

        CutsceneRequest.Clear();
        SceneManager.LoadScene(nextScene);
    }
}