using UnityEngine;
using UnityEngine.AI;
using static EnemyVisionChase;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVisionChase))]
public class EnemyAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioSource screamSource;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] wanderFootsteps;
    [SerializeField] private AudioClip[] investigateFootsteps;
    [SerializeField] private AudioClip[] chaseFootsteps;

    [Header("Breathing")]
    [SerializeField] private AudioClip wanderBreath;
    [SerializeField] private AudioClip investigateBreath;
    [SerializeField] private AudioClip chaseBreath;

    [Header("Scream")]
    [SerializeField] private AudioClip chaseScream;

    [Header("Footstep Settings")]
    [SerializeField] private float wanderStepInterval = 0.8f;
    [SerializeField] private float investigateStepInterval = 0.6f;
    [SerializeField] private float chaseStepInterval = 0.35f;
    [SerializeField] private float minMoveSpeed = 0.05f;
    [SerializeField] private float footstepVolume = 0.8f;
    [SerializeField] private float pitchRandomness = 0.08f;

    private NavMeshAgent agent;
    private EnemyVisionChase enemy;
    private EnemyState lastState;
    private float stepTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<EnemyVisionChase>();

        lastState = enemy.CurrentState;
    }

    private void Start()
    {
        Setup3DAudio(footstepsSource);
        Setup3DAudio(breathingSource);
        Setup3DAudio(screamSource);

        UpdateBreathing(enemy.CurrentState);
    }

    private void Update()
    {
        HandleStateAudio();
        HandleFootsteps();
    }

    private void HandleStateAudio()
    {
        EnemyState currentState = enemy.CurrentState;

        if (currentState == lastState)
            return;

        UpdateBreathing(currentState);

        if (currentState == EnemyState.Chase && chaseScream != null)
            screamSource.PlayOneShot(chaseScream);

        lastState = currentState;
    }

    private void HandleFootsteps()
    {
        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

        if (speed < minMoveSpeed)
        {
            stepTimer = 0f;
            return;
        }

        float interval = GetStepInterval(enemy.CurrentState);

        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            PlayFootstep(enemy.CurrentState);
            stepTimer = 0f;
        }
    }

    private void PlayFootstep(EnemyState state)
    {
        if (footstepsSource.isPlaying)
            return;

        AudioClip[] clips = GetFootstepClips(state);

        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        footstepsSource.clip = clip;
        footstepsSource.volume = footstepVolume;
        footstepsSource.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
        footstepsSource.Play();
    }

    private void UpdateBreathing(EnemyState state)
    {
        AudioClip breathClip = GetBreathClip(state);

        if (breathClip == null)
            return;

        if (breathingSource.clip == breathClip && breathingSource.isPlaying)
            return;

        breathingSource.Stop();
        breathingSource.clip = breathClip;
        breathingSource.loop = true;
        breathingSource.Play();
    }

    private AudioClip[] GetFootstepClips(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Wander:
                return wanderFootsteps;

            case EnemyState.Investigate:
                return investigateFootsteps;

            case EnemyState.Chase:
                return chaseFootsteps;

            default:
                return wanderFootsteps;
        }
    }

    private AudioClip GetBreathClip(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Wander:
                return wanderBreath;

            case EnemyState.Investigate:
                return investigateBreath;

            case EnemyState.Chase:
                return chaseBreath;

            default:
                return wanderBreath;
        }
    }

    private float GetStepInterval(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Wander:
                return wanderStepInterval;

            case EnemyState.Investigate:
                return investigateStepInterval;

            case EnemyState.Chase:
                return chaseStepInterval;

            default:
                return wanderStepInterval;
        }
    }

    private void Setup3DAudio(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
    }
}