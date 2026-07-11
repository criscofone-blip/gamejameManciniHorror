using UnityEngine;

public class PlayerPickupAudio : MonoBehaviour
{
    [Header("Audio raccolta")]
    public AudioSource audioSource;
    public AudioClip pickupClip;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayPickupSound()
    {
        if (audioSource != null && pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip);
        }
    }
}