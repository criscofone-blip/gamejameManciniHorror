using UnityEngine;

public class PickupObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPickupAudio audioPlayer = other.GetComponent<PlayerPickupAudio>();

            if (audioPlayer != null)
            {
                audioPlayer.PlayPickupSound();
            }

            Destroy(gameObject);
        }
    }
}