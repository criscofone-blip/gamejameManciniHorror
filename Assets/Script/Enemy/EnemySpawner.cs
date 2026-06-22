using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemies (in ordine: Enemy_1, Enemy_2, Enemy_3)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float navMeshSampleDistance = 3f;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("[EnemySpawner] Nessun enemy prefab assegnato.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] Nessuno spawn point assegnato.");
            return;
        }

        int count = 1;

        if (GameManager.Instance != null)
            count = GameManager.Instance.EnemyCount;

        // Non spawnare più nemici di quanti prefab abbiamo in lista.
        count = Mathf.Clamp(count, 0, enemyPrefabs.Length);

        // Spawna i primi "count" nemici in ordine: Enemy_1, poi Enemy_2, poi Enemy_3.
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = enemyPrefabs[i];

            if (prefab == null)
            {
                Debug.LogWarning($"[EnemySpawner] enemyPrefabs[{i}] non assegnato, salto.");
                continue;
            }

            // Spawn point totalmente casuale (due nemici possono capitare nello stesso punto).
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (spawn == null)
                continue;

            // Aggancia la posizione al NavMesh, così l'agent nasce sempre navigabile.
            Vector3 spawnPosition = spawn.position;

            if (NavMesh.SamplePosition(spawn.position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                spawnPosition = hit.position;

            Instantiate(prefab, spawnPosition, spawn.rotation);
        }
    }
}
