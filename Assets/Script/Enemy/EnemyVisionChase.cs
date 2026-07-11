using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyVisionChase : MonoBehaviour
{
    public enum EnemyState
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

    [Header("Anti-Stuck")]
    [Tooltip("Sotto questa velocità (m/s) l'agent è considerato potenzialmente bloccato.")]
    [SerializeField] private float stuckSpeedThreshold = 0.1f;
    [Tooltip("Secondi di quasi-immobilità (mentre dovrebbe muoversi) prima di forzare una via di fuga.")]
    [SerializeField] private float stuckTimeToUnstuck = 1f;
    [Tooltip("Quanti punti campionare per trovare una via di fuga raggiungibile.")]
    [SerializeField] private int stuckSampleAttempts = 8;

    [Header("Game Over")]
    [SerializeField] private float killDistance = 1.5f;
    [Tooltip("Indice del mostro (0/1/2): sceglie il pannello di game over.")]
    [SerializeField] private int monsterIndex = 0;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private NavMeshAgent agent;
    private EnemyState currentState;
    private float wanderTimer;
    private float investigateTimer;
    private Vector3 lastSeenPlayerPosition;
    private bool wasFrozen;
    private float stuckTimer;
    private NavMeshPath cachedPath;
    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        cachedPath = new NavMeshPath();

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

        // Dopo aver aggiornato lo stato, controlla se è rimasto bloccato.
        DetectAndResolveStuck();
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
        if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            StartInvestigate();
        }
        if (showDebugLogs)
            Debug.Log("[Enemy] Stato -> CHASE");
    }

    private void UpdateChase()
    {
        agent.speed = chaseSpeed;
        lastSeenPlayerPosition = player.position;

        if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            StartInvestigate();
        }
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
                gameOverManager.TriggerGameOver(monsterIndex);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 enemyEyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerTargetPosition = player.position + Vector3.up * 1.2f;

        Vector3 toPlayer = playerTargetPosition - transform.position; //modifica => "transform.position" invece di "enemyEyePosition".
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
                distanceToPlayer,
                visionMask,
                QueryTriggerInteraction.Ignore))
        {
            if(hit.collider != null) //agginta sezione if
            {
                string LayerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

                if(LayerName == "Obstacles")
                {

                    return false;
                }

                if (hit.transform == player || hit.transform.IsChildOf(player))
                {                    
                    return true;
                }
                   

            }
                
            
        }

        return false;
    }

    private void SetNewWanderPoint()
    {
        // Preferisci un punto con percorso COMPLETO: evita di puntare verso posti irraggiungibili.
        if (TryFindReachablePoint(out Vector3 reachablePoint))
        {
            agent.SetDestination(reachablePoint);
            return;
        }

        // Fallback: comportamento originale.
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ---------- Anti-Stuck ----------

    private void DetectAndResolveStuck()
    {
        // Niente da controllare se è fermo di proposito o senza percorso.
        if (agent.isStopped || agent.pathPending || !agent.hasPath)
        {
            stuckTimer = 0f;
            return;
        }

        // È praticamente arrivato: non è bloccato.
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            stuckTimer = 0f;
            return;
        }

        // In Investigate la pausa di rotazione è un fermo voluto.
        if (currentState == EnemyState.Investigate && agent.remainingDistance <= investigateStopDistance)
        {
            stuckTimer = 0f;
            return;
        }

        // Dovrebbe muoversi ma è quasi fermo → accumula tempo di blocco.
        if (agent.velocity.sqrMagnitude < stuckSpeedThreshold * stuckSpeedThreshold)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckTimeToUnstuck)
            {
                stuckTimer = 0f;
                ResolveStuck();
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private void ResolveStuck()
    {
        if (showDebugLogs)
            Debug.Log("[Enemy] Bloccato → cerco una via di fuga");

        switch (currentState)
        {
            case EnemyState.Chase:
                // Riprova un percorso completo verso il player; se non c'è, fuggi vagando.
                if (!TrySetDestinationTo(player.position))
                    GoWanderEscape();
                break;

            case EnemyState.Investigate:
                GoWanderEscape();
                break;

            case EnemyState.Wander:
                if (!SetEscapePoint())
                    SetNewWanderPoint();
                break;
        }
    }

    private void GoWanderEscape()
    {
        currentState = EnemyState.Wander;
        agent.speed = wanderSpeed;
        wanderTimer = 0f;

        if (!SetEscapePoint())
            SetNewWanderPoint();
    }

    private bool SetEscapePoint()
    {
        if (TryFindReachablePoint(out Vector3 point))
        {
            agent.SetDestination(point);
            return true;
        }

        return false;
    }

    // Campiona più punti e sceglie il raggiungibile (percorso completo) più lontano.
    private bool TryFindReachablePoint(out Vector3 result)
    {
        float bestDistance = -1f;
        Vector3 best = transform.position;
        bool found = false;

        for (int i = 0; i < stuckSampleAttempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir += transform.position;
            randomDir.y = transform.position.y;

            if (!NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                continue;

            if (!agent.CalculatePath(hit.position, cachedPath) || cachedPath.status != NavMeshPathStatus.PathComplete)
                continue;

            float d = (hit.position - transform.position).sqrMagnitude;

            if (d > bestDistance)
            {
                bestDistance = d;
                best = hit.position;
                found = true;
            }
        }

        result = best;
        return found;
    }

    // Imposta la destinazione solo se esiste un percorso completo verso il target.
    private bool TrySetDestinationTo(Vector3 target)
    {
        if (!NavMesh.SamplePosition(target, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            return false;

        if (!agent.CalculatePath(hit.position, cachedPath) || cachedPath.status != NavMeshPathStatus.PathComplete)
            return false;

        agent.SetDestination(hit.position);
        return true;
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