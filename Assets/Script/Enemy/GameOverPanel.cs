using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameOverPanel : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private AudioSource audioSource;

    [Header("Buttons")]
    [SerializeField] private GameObject buttonsContainer;
    [Tooltip("Secondi (tempo reale) prima che compaiano i tasti.")]
    [SerializeField] private float buttonsDelay = 10f;

    private Coroutine buttonsRoutine;

    public void Show()
    {
        gameObject.SetActive(true);

        if (buttonsContainer != null)
            buttonsContainer.SetActive(false);

        PlayVideo();

        if (buttonsRoutine != null)
            StopCoroutine(buttonsRoutine);

        buttonsRoutine = StartCoroutine(RevealButtonsAfterDelay());
    }

    public void Hide()
    {
        if (buttonsRoutine != null)
        {
            StopCoroutine(buttonsRoutine);
            buttonsRoutine = null;
        }

        gameObject.SetActive(false);
    }

    private void PlayVideo()
    {
        if (videoPlayer == null)
            return;

        if (audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (videoImage != null)
            videoImage.texture = vp.texture;

        vp.Play();

        if (audioSource != null)
            audioSource.Play();
    }

    private IEnumerator RevealButtonsAfterDelay()
    {
        // Tempo reale: durante il game over Time.timeScale è a 0.
        yield return new WaitForSecondsRealtime(buttonsDelay);

        if (buttonsContainer != null)
            buttonsContainer.SetActive(true);
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
    }
}
