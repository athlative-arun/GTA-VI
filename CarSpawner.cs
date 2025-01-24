using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs; 
    public GameObject waypointsParent; 
    public Transform player;     public float spawnRadius = 5f; 

    private List<Transform> waypoints = new List<Transform>();
    private Dictionary<Transform, GameObject> activeCars = new Dictionary<Transform, GameObject>(); 
    public int carCount;
    public int maxCars;

    private float minSpawnDistance = 150f; 
    private float maxSpawnDistance = 200f; 
    private float minSpawnDistanceSqr;
    private float maxSpawnDistanceSqr;

    void Start()
    {
        
        minSpawnDistanceSqr = minSpawnDistance * minSpawnDistance;
        maxSpawnDistanceSqr = maxSpawnDistance * maxSpawnDistance;

        
        foreach (Transform child in waypointsParent.transform)
        {
            waypoints.Add(child);
        }
    }

    void Update()
    {
        if (carCount < maxCars)
        {
            SpawnCarsInRange();
            print("spawning");
        }
        DespawnCarsOutOfRange();
        carCount = activeCars.Count;
    }

    void SpawnCarsInRange()
    {
        foreach (Transform waypoint in waypoints)
        {
            
            float sqrDistanceToPlayer = (player.position - waypoint.position).sqrMagnitude;

            if (sqrDistanceToPlayer >= minSpawnDistanceSqr && sqrDistanceToPlayer <= maxSpawnDistanceSqr && !activeCars.ContainsKey(waypoint))
            {
                if (!IsObjectNearby(waypoint.position))
                {
                    SpawnCar(waypoint);
                }
            }
        }
    }

    void DespawnCarsOutOfRange()
    {
      
        List<Transform> waypointsToDespawn = new List<Transform>();

        foreach (var entry in activeCars)
        {
            Transform waypoint = entry.Key;
            GameObject car = entry.Value;

            float sqrDistanceToPlayer = (player.position - car.transform.position).sqrMagnitude;

            
            if (sqrDistanceToPlayer > maxSpawnDistanceSqr)
            {
                Destroy(car.GetComponent<CarAI>().driver);
                Destroy(car);
                waypointsToDespawn.Add(waypoint);
            }
        }

       
        foreach (Transform waypoint in waypointsToDespawn)
        {
            activeCars.Remove(waypoint);
        }
    }

    void SpawnCar(Transform waypoint)
    {
        
        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

        
        GameObject car = Instantiate(carPrefab, waypoint.position, waypoint.rotation);

        Transform nextWaypoint = GetNextWaypoint(waypoint);
        if (nextWaypoint != null)
        {
            Vector3 directionToNext = (nextWaypoint.position - waypoint.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToNext);
            car.transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0); 
        }

        
        activeCars[waypoint] = car;
    }

    Transform GetNextWaypoint(Transform currentWaypoint)
    {
        int currentIndex = waypoints.IndexOf(currentWaypoint);

        
        if (currentIndex >= 0 && currentIndex < waypoints.Count - 1)
        {
            return waypoints[currentIndex + 1];
        }

        return null; 
    }

    bool IsObjectNearby(Vector3 position)
    {
        
        Collider[] colliders = Physics.OverlapSphere(position, spawnRadius);

        foreach (Collider collider in colliders)
        {
          
            if (collider.CompareTag("Car"))
            {
                return true; 
            }
        }

        return false; 
    }
}
