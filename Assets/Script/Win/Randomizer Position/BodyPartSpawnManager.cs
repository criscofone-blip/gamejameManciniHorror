using System.Collections.Generic;
using UnityEngine;

public class BodyPartSpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private List<BodyPartSpawnPoint> spawnPoints;

    [Header("Body Parts Prefabs")]
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private GameObject torsoPrefab;
    [SerializeField] private GameObject armsPrefab;
    [SerializeField] private GameObject legsPrefab;

    private void Start()
    {
        SpawnBodyParts();
    }

    private void SpawnBodyParts()
    {
        List<GameObject> bodyParts = new List<GameObject>
        {
            headPrefab,
            torsoPrefab,
            armsPrefab,
            legsPrefab
        };

        // Spegni le luci di tutti gli spawn point: le accenderemo solo dove spawna un pezzo.
        foreach (var point in spawnPoints)
        {
            if (point != null)
                point.SetLightActive(false);
        }

        // Mischia spawn points
        List<BodyPartSpawnPoint> shuffled = new List<BodyPartSpawnPoint>(spawnPoints);
        ShuffleList(shuffled);

        HashSet<RoomType> usedRooms = new HashSet<RoomType>();

        int spawned = 0;

        foreach (var point in shuffled)
        {
            if (spawned >= bodyParts.Count)
                break;

            // Se abbiamo già usato questa stanza → skip
            if (usedRooms.Contains(point.roomType))
                continue;

            GameObject prefab = bodyParts[spawned];

            Instantiate(
                prefab,
                point.transform.position,
                point.transform.rotation
            );

            // Qui è spawnato un pezzo → accendi la luce di questo spawn point.
            point.SetLightActive(true);

            usedRooms.Add(point.roomType);
            spawned++;
        }

        if (spawned < bodyParts.Count)
        {
            Debug.LogWarning("Non ci sono abbastanza stanze diverse per spawnare tutti i pezzi!");
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}