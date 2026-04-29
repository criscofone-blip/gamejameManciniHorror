using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyVisionChase : MonoBehaviour
{
    private enum EnemyState
    {
        Wander,
        Chase,
        Investigate
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private LayerMask visionMask = ~0;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float wanderInterval = 3f;

    [Header("Investigate")]
    [SerializeField] private float investigateDuration = 4f;
    [SerializeField] private float investigateStopDistance = 1f;

    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float investigateSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Game Over")]
    [SerializeField] private float killDistance = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private NavMeshAgent agent;
    private EnemyState currentState;
    private float wanderTimer;
    private float investigateTimer;
    private Vector3 lastSeenPlayerPosition;
    private bool wasFrozen;

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

        if (showDebugLogs)
        {
            Debug.Log($"[Enemy] Player trovato: {player != null}");
            Debug.Log($"[Enemy] Agent on NavMesh: {agent.isOnNavMesh}");
        }
    }

    private void Update()
    {
        if (player == null || !agent.isOnNavMesh)
            return;

        if (PlayerEyesCover.EyesCovered)
        {
            FreezeEnemy();
            return;
        }

        if (wasFrozen)
            UnfreezeEnemy();

        CheckGameOver();

        bool canSeePlayer = CanSeePlayer();

        switch (currentState)
        {
            case EnemyState.Wander:
                if (canSeePlayer)
                    StartChase();
                else
                    UpdateWander();
                break;

            case EnemyState.Chase:
                if (canSeePlayer)
                    UpdateChase();
                else
                    StartInvestigate();
                break;

            case EnemyState.Investigate:
                if (canSeePlayer)
                    StartChase();
                else
                    UpdateInvestigate();
                break;
        }
    }

    private void FreezeEnemy()
    {
        if (wasFrozen)
            return;

        wasFrozen = true;
        agent.isStopped = true;

        if (showDebugLogs)
            Debug.Log("[Enemy] Frozen");
    }

    private void UnfreezeEnemy()
    {
        wasFrozen = false;
        agent.isStopped = false;

        switch (currentState)
        {
            case EnemyState.Wander:
                SetNewWanderPoint();
                break;

            case EnemyState.Chase:
                agent.SetDestination(player.position);
                break;

            case EnemyState.Investigate:
                agent.SetDestination(lastSeenPlayerPosition);
                break;
        }

        if (showDebugLogs)
            Debug.Log("[Enemy] Unfrozen");
    }

    private void StartChase()
    {
        currentState = EnemyState.Chase;
        agent.speed = chaseSpeed;
        lastSeenPlayerPosition = player.position;
        agent.SetDestination(player.position);

        if (showDebugLogs)
            Debug.Log("[Enemy] Stato -> CHASE");
    }

    private void UpdateChase()
    {
        agent.speed = chaseSpeed;
        lastSeenPlayerPosition = player.position;
        agent.SetDestination(player.position);
    }

    private void StartInvestigate()
    {
        currentState = EnemyState.Investigate;
        agent.speed = investigateSpeed;
        investigateTimer = investigateDuration;
        agent.SetDestination(lastSeenPlayerPosition);

        if (showDebugLogs)
            Debug.Log("[Enemy] Stato -> INVESTIGATE");
    }

    private void UpdateInvestigate()
    {
        agent.speed = investigateSpeed;

        if (!agent.pathPending && agent.remainingDistance <= investigateStopDistance)
        {
            investigateTimer -= Time.deltaTime;
            transform.Rotate(Vector3.up * 60f * Time.deltaTime);
        }

        if (investigateTimer <= 0f)
            StartWander();
    }

    private void StartWander()
    {
        currentState = EnemyState.Wander;
        agent.speed = wanderSpeed;
        wanderTimer = 0f;
        SetNewWanderPoint();

        if (showDebugLogs)
            Debug.Log("[Enemy] Stato -> WANDER");
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
        Vector3 playerTargetPosition = player.position + Vector3.up * 1.2f;

        Vector3 toPlayer = playerTargetPosition - enemyEyePosition;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer.normalized);

        if (angleToPlayer > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast( 
        enemyEyePosition,
        toPlayer.normalized,
        out RaycastHit hit,
        100,
        visionMask,
        QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Enemy vede raycast hit: " + hit.transform.name);

            if (hit.transform == player || hit.transform.IsChildOf(player) || player.IsChildOf(hit.transform))
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
            agent.SetDestination(hit.position);
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

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(lastSeenPlayerPosition, 0.2f);
    }
}