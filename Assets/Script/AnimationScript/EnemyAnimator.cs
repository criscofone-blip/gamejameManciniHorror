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

    // Impostati a true solo se il parametro esiste davvero nel Controller.
    private bool hasSpeedParam;
    private bool hasChasingParam;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<EnemyVisionChase>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        speedHash = Animator.StringToHash(speedParameter);
        chasingHash = Animator.StringToHash(chasingParameter);

        // Verifica una sola volta quali parametri esistono, così evitiamo lo spam di warning.
        hasSpeedParam = HasParameter(speedHash, AnimatorControllerParameterType.Float);
        hasChasingParam = HasParameter(chasingHash, AnimatorControllerParameterType.Bool);
    }

    private void Update()
    {
        if (animator == null)
            return;

        if (hasSpeedParam)
        {
            // Velocità orizzontale reale dell'agent (ignora la componente verticale).
            Vector3 velocity = agent.velocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;

            // Speed → controlla Idle (fermo) vs Walk (Wander/Investigate) nell'Animator.
            animator.SetFloat(speedHash, speed, speedDampTime, Time.deltaTime);
        }

        if (hasChasingParam)
        {
            // IsChasing → passaggio all'animazione di inseguimento.
            bool isChasing = enemy.CurrentState == EnemyVisionChase.EnemyState.Chase;
            animator.SetBool(chasingHash, isChasing);
        }
    }

    private bool HasParameter(int paramHash, AnimatorControllerParameterType type)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == paramHash && param.type == type)
                return true;
        }

        return false;
    }
}
