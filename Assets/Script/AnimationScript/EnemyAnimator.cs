using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVisionChase))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string chasingParameter = "IsChasing";

    [Header("Settings")]
    [Tooltip("Smoothing applicato al parametro Speed, per blend Idle/Walk più morbidi.")]
    [SerializeField] private float speedDampTime = 0.1f;

    private NavMeshAgent agent;
    private EnemyVisionChase enemy;

    private int speedHash;
    private int chasingHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<EnemyVisionChase>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        speedHash = Animator.StringToHash(speedParameter);
        chasingHash = Animator.StringToHash(chasingParameter);
    }

    private void Update()
    {
        if (animator == null)
            return;

        // Velocità orizzontale reale dell'agent (ignora la componente verticale).
        Vector3 velocity = agent.velocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;

        // Speed → controlla Idle (fermo) vs Walk (Wander/Investigate) nell'Animator.
        animator.SetFloat(speedHash, speed, speedDampTime, Time.deltaTime);

        // IsChasing → passaggio all'animazione di inseguimento.
        bool isChasing = enemy.CurrentState == EnemyVisionChase.EnemyState.Chase;
        animator.SetBool(chasingHash, isChasing);
    }
}
