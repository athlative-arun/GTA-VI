using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class PedestrianSpawner : MonoBehaviour
{
    public GameObject[] pedestrianPrefab;
    public Transform player;
    public float spawnRadius = 100f;
    public float minSpawnDistance = 20f;
    public float despawnDistance = 200f;
    public float spawnInterval = 2f;
    public int initialSpawnCount = 10;
    public int maxPedestrianCount = 50;

    private List<GameObject> spawnedPedestrians = new List<GameObject>();
    public int currentPedestrianCount;

    void Start()
    {
        if (pedestrianPrefab == null || player == null)
        {
            return;
        }

        minSpawnDistance = 5f;
        SpawnInitialPedestrians();
        StartCoroutine(SpawnPedestrians());
    }

    void Update()
    {
        DespawnDistantPedestrians();
        currentPedestrianCount = spawnedPedestrians.Count;
    }

    void SpawnInitialPedestrians()
    {
        for (int i = 0; i < initialSpawnCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPositionCloseToPlayer();
            if (spawnPosition != Vector3.zero)
            {
                GameObject pedestrian = Instantiate(pedestrianPrefab[RandomPed()], spawnPosition, Quaternion.identity);
                spawnedPedestrians.Add(pedestrian);
            }
        }
        minSpawnDistance = 20;
    }

    int RandomPed()
    {
        return Random.Range(0, pedestrianPrefab.Length);
    }

    IEnumerator SpawnPedestrians()
    {
        while (true)
        {
            if (spawnedPedestrians.Count < maxPedestrianCount)
            {
                Vector3 spawnPosition = GetSpawnPositionCloseToPlayer();
                if (spawnPosition != Vector3.zero)
                {
                    GameObject pedestrian = Instantiate(pedestrianPrefab[RandomPed()], spawnPosition, Quaternion.identity);
                    spawnedPedestrians.Add(pedestrian);
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Vector3 GetSpawnPositionCloseToPlayer()
    {
        float currentRadius = minSpawnDistance;
        float radiusStep = (spawnRadius - minSpawnDistance) / 5f;

        for (int step = 0; step < 5; step++)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * currentRadius;
                randomDirection += player.position;
                randomDirection.y = player.position.y;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
                {
                    int area = hit.mask;
                    int footpathArea = NavMesh.GetAreaFromName("Walkable");

                    if ((area & (1 << footpathArea)) != 0 && Vector3.Distance(hit.position, player.position) >= minSpawnDistance)
                    {
                        return hit.position;
                    }
                }
            }
            currentRadius += radiusStep;
        }

        return Vector3.zero;
    }

    void DespawnDistantPedestrians()
    {
        for (int i = spawnedPedestrians.Count - 1; i >= 0; i--)
        {
            GameObject pedestrian = spawnedPedestrians[i];
            if (Vector3.Distance(pedestrian.transform.position, player.position) > despawnDistance)
            {
                Destroy(pedestrian);
                spawnedPedestrians.RemoveAt(i);
            }
        }
    }
}
