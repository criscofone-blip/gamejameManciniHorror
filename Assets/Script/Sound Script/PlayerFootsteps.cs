using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private FirstPersonController playerController;

    [Header("Clips")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Settings")]
    [SerializeField] private float stepInterval = 0.45f;
    [SerializeField] private float minVelocity = 0.01f;
    [SerializeField] private float volume = 0.7f;
    [SerializeField] private float pitchRandomness = 0.08f;

    private Vector3 lastPosition;
    private float stepTimer;

    private void Awake()
    {
        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();

        if (playerController == null)
            playerController = GetComponent<FirstPersonController>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        HandleFootsteps();
        lastPosition = transform.position;
    }

    private void HandleFootsteps()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        movement.y = 0f;

        float speed = movement.magnitude / Time.deltaTime;

        bool hasInput = playerController != null &&
                        playerController.MoveInput.sqrMagnitude > 0.001f;

        bool isActuallyMoving = speed > minVelocity;

        bool isOnFloor = playerController != null &&
                         playerController.IsOnTheFloor;

        if (!hasInput || !isOnFloor)
        {
            stepTimer = 0f;
            return;
        }

        // Se il player sta premendo movimento ma è praticamente fermo contro un ostacolo,
        // non riproduciamo passi.
        if (!isActuallyMoving && speed < 0.0001f)
        {
            stepTimer = 0f;
            return;
        }

        float effectiveSpeed = Mathf.Max(speed, 0.2f);
        float dynamicInterval = stepInterval / Mathf.Clamp(effectiveSpeed, 0.5f, 2f);

        stepTimer += Time.deltaTime;

        if (stepTimer >= dynamicInterval)
        {
            PlayFootstep(effectiveSpeed);
            stepTimer = 0f;
        }
    }

    private void PlayFootstep(float speed)
    {
        if (footstepSource.isPlaying)
            return;

        if (footstepClips == null || footstepClips.Length == 0)
            return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

        float volumeMultiplier = Mathf.Clamp(speed, 0.25f, 1f);

        footstepSource.clip = clip;
        footstepSource.volume = volume * volumeMultiplier;
        footstepSource.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
        footstepSource.Play();
    }
}