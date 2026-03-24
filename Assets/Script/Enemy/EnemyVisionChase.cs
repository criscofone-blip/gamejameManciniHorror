using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyVisionChase : MonoBehaviour
{
    private enum EnemyState
    {
        Wander,
        Chase
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float wanderInterval = 3f;

    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Game Over")]
    [SerializeField] private float killDistance = 1.5f;

    private NavMeshAgent agent;
    private EnemyState currentState;
    private float wanderTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    private void Start()
    {
        currentState = EnemyState.Wander;
        agent.speed = wanderSpeed;
        SetNewWanderPoint();
    }

    private void Update()
    {
        if (player == null)
            return;

        CheckGameOver();

        bool canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            currentState = EnemyState.Chase;
        }
        else if (currentState == EnemyState.Chase)
        {
            currentState = EnemyState.Wander;
            SetNewWanderPoint();
        }

        switch (currentState)
        {
            case EnemyState.Wander:
                UpdateWander();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;
        }
    }

    private void UpdateWander()
    {
        agent.speed = wanderSpeed;
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval || ReachedDestination())
        {
            SetNewWanderPoint();
            wanderTimer = 0f;
        }
    }

    private void UpdateChase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void CheckGameOver()
    {
        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = player.position;

        enemyPosition.y = 0f;
        playerPosition.y = 0f;

        float distanceToPlayer = Vector3.Distance(enemyPosition, playerPosition);

        if (distanceToPlayer <= killDistance)
        {
            if (gameOverManager != null)
                gameOverManager.TriggerGameOver();
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 enemyEyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerTargetPosition = player.position + Vector3.up * 1.6f;

        Vector3 toPlayer = playerTargetPosition - enemyEyePosition;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer.normalized);

        if (angleToPlayer > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(enemyEyePosition, toPlayer.normalized, out RaycastHit hit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
                return true;
        }

        return false;
    }

    private void SetNewWanderPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (agent.remainingDistance > agent.stoppingDistance)
            return false;

        return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(eyePosition, eyePosition + leftBoundary * viewDistance);
        Gizmos.DrawLine(eyePosition, eyePosition + rightBoundary * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killDistance);
    }
}