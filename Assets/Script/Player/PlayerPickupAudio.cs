using UnityEngine;

public class PlayerPickupAudio : MonoBehaviour
{
    public static PlayerPickupAudio Instance { get; private set; }

    [Header("Audio raccolta")]
    public AudioSource audioSource;
    public AudioClip pickupClip;

    private void Awake()
    {
        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // Suona la clip di default.
    public void PlayPickupSound()
    {
        PlayPickup(null);
    }

    // Suona la clip passata; se null usa quella di default.
    public void PlayPickup(AudioClip clip = null)
    {
        if (audioSource == null)
            return;

        AudioClip toPlay = clip != null ? clip : pickupClip;

        if (toPlay != null)
            audioSource.PlayOneShot(toPlay);
    }
}