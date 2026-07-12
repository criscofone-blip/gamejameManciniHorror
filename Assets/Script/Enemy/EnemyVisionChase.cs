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

    [Header("Chase Repath (ottimizzazione)")]
    [Tooltip("Intervallo minimo (s) tra un ricalcolo del percorso e l'altro in inseguimento.")]
    [SerializeField] private float chaseRepathInterval = 0.15f;
    [Tooltip("Se il player si sposta più di questa distanza (m) si ricalcola subito.")]
    [SerializeField] private float chaseRepathMoveThreshold = 0.75f;

    [Header("Anti-Stuck")]
    [Tooltip("Ogni quanti secondi controllare i progressi di movimento.")]
    [SerializeField] private float stuckCheckInterval = 0.4f;
    [Tooltip("Se in un intervallo si sposta meno di questa distanza (m), l'intervallo è considerato bloccato.")]
    [SerializeField] private float minProgressDistance = 0.12f;
    [Tooltip("Secondi totali di mancato progresso prima di forzare una via di fuga.")]
    [SerializeField] private float stuckTimeToUnstuck = 1f;
    [Tooltip("Quanti punti campionare per trovare una via di fuga raggiungibile.")]
    [SerializeField] private int stuckSampleAttempts = 8;

    [Header("Game Over")]
    [SerializeField] private float killDistance = 1.5f;
    [Tooltip("Indice del mostro (0/1/2): sceglie il pannello di game over.")]
    [SerializeField] private int monsterIndex = 0;

    private NavMeshAgent agent;
    private EnemyState currentState;
    private float wanderTimer;
    private float investigateTimer;
    private Vector3 lastSeenPlayerPosition;
    private bool wasFrozen;
    private NavMeshPath cachedPath;
    private int obstaclesLayer;

    // Chase: throttle del ricalcolo percorso.
    private float chaseRepathTimer;
    private Vector3 lastChaseTarget;

    // Anti-stuck (basato sullo spostamento reale nel tempo).
    private Vector3 lastCheckPosition;
    private float lastCheckTime;
    private float stuckAccumulated;

    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        cachedPath = new NavMeshPath();
        obstaclesLayer = LayerMask.NameToLayer("Obstacles");

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

        lastCheckPosition = transform.position;
        lastCheckTime = Time.time;

        SetNewWanderPoint();
    }

    private void Update()
    {
        if (player == null)
            return;

        // Se è finito fuori dalla NavMesh, riportalo sul punto valido più vicino.
        if (!agent.isOnNavMesh)
        {
            TryRecoverToNavMesh();
            return;
        }

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
    }

    private void StartChase()
    {
        currentState = EnemyState.Chase;
        agent.speed = chaseSpeed;
        lastSeenPlayerPosition = player.position;

        chaseRepathTimer = 0f;
        RepathToPlayer();   // primo ricalcolo immediato
    }

    private void UpdateChase()
    {
        agent.speed = chaseSpeed;
        lastSeenPlayerPosition = player.position;

        // Ricalcola il percorso solo a intervalli o se il player si è spostato molto:
        // evita un pathfind ogni frame per ogni nemico.
        chaseRepathTimer -= Time.deltaTime;

        bool playerMovedFar =
            (player.position - lastChaseTarget).sqrMagnitude >
            chaseRepathMoveThreshold * chaseRepathMoveThreshold;

        if (chaseRepathTimer <= 0f || playerMovedFar)
        {
            chaseRepathTimer = chaseRepathInterval;
            RepathToPlayer();
        }
    }

    private void RepathToPlayer()
    {
        lastChaseTarget = player.position;

        if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            StartInvestigate();
    }

    private void StartInvestigate()
    {
        currentState = EnemyState.Investigate;
        agent.speed = investigateSpeed;
        investigateTimer = investigateDuration;
        agent.SetDestination(lastSeenPlayerPosition);
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
                // Confronto per indice di layer (niente stringhe/allocazioni ogni frame).
                if (hit.collider.gameObject.layer == obstaclesLayer)
                    return false;

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
        // Wander normale: un punto CASUALE raggiungibile (percorso completo).
        if (TryFindReachablePoint(out Vector3 reachablePoint, false))
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

    // ---------- Anti-Stuck (basato sullo spostamento reale) ----------

    private void DetectAndResolveStuck()
    {
        // Fermo di proposito.
        if (agent.isStopped)
        {
            ResetStuckTracking();
            return;
        }

        // In Investigate la pausa di rotazione è un fermo voluto.
        if (currentState == EnemyState.Investigate &&
            !agent.pathPending &&
            agent.remainingDistance <= investigateStopDistance)
        {
            ResetStuckTracking();
            return;
        }

        // Controlla i progressi solo a intervalli.
        if (Time.time - lastCheckTime < stuckCheckInterval)
            return;

        float moved = Vector3.Distance(transform.position, lastCheckPosition);
        float elapsed = Time.time - lastCheckTime;

        lastCheckPosition = transform.position;
        lastCheckTime = Time.time;

        // Se non ha una meta, dagliene una invece di considerarlo bloccato.
        if (!agent.hasPath && !agent.pathPending)
        {
            if (currentState == EnemyState.Wander)
                SetNewWanderPoint();

            stuckAccumulated = 0f;
            return;
        }

        if (moved >= minProgressDistance)
        {
            // Progredisce: tutto ok.
            stuckAccumulated = 0f;
        }
        else
        {
            // Nessun progresso reale in questo intervallo.
            stuckAccumulated += elapsed;

            if (stuckAccumulated >= stuckTimeToUnstuck)
            {
                stuckAccumulated = 0f;
                ResolveStuck();
            }
        }
    }

    private void ResetStuckTracking()
    {
        stuckAccumulated = 0f;
        lastCheckPosition = transform.position;
        lastCheckTime = Time.time;
    }

    private void ResolveStuck()
    {
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

    private void TryRecoverToNavMesh()
    {
        // Cerca il punto navmesh più vicino e riporta l'agent lì.
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            agent.Warp(hit.position);
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
        // Via di fuga: il punto raggiungibile più LONTANO, per uscire davvero dal punto stretto.
        if (TryFindReachablePoint(out Vector3 point, true))
        {
            agent.SetDestination(point);
            return true;
        }

        return false;
    }

    // Campiona più punti raggiungibili (percorso completo).
    // preferFarthest = true → sceglie il più lontano (fuga); false → il primo trovato (wander naturale).
    private bool TryFindReachablePoint(out Vector3 result, bool preferFarthest)
    {
        float bestDistance = -1f;
        Vector3 best = transform.position;
        bool found = false;

        // "Abbastanza lontano": appena troviamo un punto oltre questa distanza smettiamo
        // di campionare (meno CalculatePath = meno costo nel frame di sblocco).
        float goodEnough = wanderRadius * 0.6f;
        float goodEnoughSq = goodEnough * goodEnough;

        for (int i = 0; i < stuckSampleAttempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir += transform.position;
            randomDir.y = transform.position.y;

            if (!NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                continue;

            if (!agent.CalculatePath(hit.position, cachedPath) || cachedPath.status != NavMeshPathStatus.PathComplete)
                continue;

            if (!preferFarthest)
            {
                // Primo punto raggiungibile va bene (wander vario e naturale).
                result = hit.position;
                return true;
            }

            float d = (hit.position - transform.position).sqrMagnitude;

            if (d > bestDistance)
            {
                bestDistance = d;
                best = hit.position;
                found = true;
            }

            // Già trovato un punto abbastanza lontano → basta così.
            if (found && bestDistance >= goodEnoughSq)
                break;
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