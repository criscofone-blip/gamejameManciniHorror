using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float navMeshSampleDistance = 3f;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab non assegnato.");
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

        for (int i = 0; i < count; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (spawn == null)
                continue;

            // Aggancia la posizione al NavMesh, così l'agent nasce sempre navigabile.
            Vector3 spawnPosition = spawn.position;

            if (NavMesh.SamplePosition(spawn.position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                spawnPosition = hit.position;

            Instantiate(enemyPrefab, spawnPosition, spawn.rotation);
        }
    }
}