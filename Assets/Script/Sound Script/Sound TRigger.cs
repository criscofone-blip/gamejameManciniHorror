using UnityEngine;

public class SoundTRigger : MonoBehaviour
{

    public AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.time = 0f;
                audioSource.Play();
            }
        }
    }

}
