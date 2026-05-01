using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerImpactSound : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource impactSource;

    [Header("Clips")]
    [SerializeField] private AudioClip[] impactClips;

    [Header("Filter")]
    [SerializeField] private LayerMask impactLayers;

    [Header("Settings")]
    [SerializeField] private float minImpactSpeed = 0.15f;
    [SerializeField] private float volume = 0.7f;
    [SerializeField] private float pitchRandomness = 0.08f;
    [SerializeField] private float impactCooldown = 0.25f;

    private Vector3 lastPosition;
    private float currentSpeed;
    private float lastImpactTime;

    private void Awake()
    {
        if (impactSource == null)
            impactSource = GetComponent<AudioSource>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 movement = transform.position - lastPosition;
        movement.y = 0f;

        currentSpeed = movement.magnitude / Time.deltaTime;

        lastPosition = transform.position;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.gameObject.layer) & impactLayers) == 0)
            return;

        if (Time.time < lastImpactTime + impactCooldown)
            return;

        if (currentSpeed < minImpactSpeed)
            return;

        lastImpactTime = Time.time;
        PlayImpact(currentSpeed);
    }

    private void PlayImpact(float speed)
    {
        if (impactClips == null || impactClips.Length == 0)
            return;

        if (impactSource.isPlaying)
            return;

        AudioClip clip = impactClips[Random.Range(0, impactClips.Length)];

        float volumeMultiplier = Mathf.Clamp(speed, 0.2f, 1f);

        impactSource.clip = clip;
        impactSource.volume = volume * volumeMultiplier;
        impactSource.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
        impactSource.Play();
    }
}
