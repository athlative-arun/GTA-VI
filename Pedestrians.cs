using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Pedestrians : MonoBehaviour
{
    private NavMeshAgent agent;
    public Animator animator;

    public Transform[] waypoints;

    private float idleDuration = 3f;

    public float defaultSpeed;
    public float boostedSpeed = 6f;
    public float boostedDuration = 10f;

    public int health = 3;

    public GameObject ragdoll;
    public bool dead;
    Rigidbody rb;
    public PlayerMovement player;
    Collider carHit;

    public GameObject bloodSplash;

    public float hearingRange = 20f;

    void Start()
    {
        SetLayerRecursively(gameObject, "npc");
        GetComponent<NavMeshAgent>().enabled = true;
        agent = GetComponent<NavMeshAgent>();
        defaultSpeed = agent.speed;

        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("waypoint");
        waypoints = new Transform[waypointObjects.Length];

        for (int i = 0; i < waypointObjects.Length; i++)
        {
            waypoints[i] = waypointObjects[i].transform;
        }
        player = FindFirstObjectByType<PlayerMovement>();
        StartCoroutine(RandomMovement());
        rb = GetComponent<Rigidbody>();
        carHit = GetComponent<Collider>();
    }

    void Update()
    {
        animator.SetFloat("speed", agent.velocity.magnitude);

        if (player.enabled)
        {
            carHit.enabled = false;
        }
        else
        {
            carHit.enabled = true;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            int roadArea = NavMesh.GetAreaFromName("Road");
            int walkableArea = NavMesh.GetAreaFromName("Walkable");

            if (hit.mask == (1 << roadArea))
            {
                MoveToNearestWalkableArea(walkableArea);
            }
        }
    }

    void MoveToNearestWalkableArea(int walkableArea)
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit walkableHit, 10.0f, 1 << walkableArea))
        {
            agent.SetDestination(walkableHit.position);
        }
    }

    IEnumerator RandomMovement()
    {
        while (!dead)
        {
            Transform targetWaypoint = waypoints[Random.Range(0, waypoints.Length)];
            agent.SetDestination(targetWaypoint.position);

            while (!dead && (agent.remainingDistance > agent.stoppingDistance || agent.pathPending))
            {
                yield return null;
            }

            yield return new WaitForSeconds(idleDuration);
        }
    }

    public void TakeDamage(int damage)
    {
        StopCoroutine("BoostSpeed");
        StartCoroutine(BoostSpeed());
        health -= damage;

        if (health <= 0)
        {
            dead = true;
            StopCoroutine(RandomMovement());
            TriggerRagdoll();
        }
    }

    public IEnumerator BoostSpeed()
    {
        idleDuration = 0f;
        agent.speed = boostedSpeed;

        yield return new WaitForSeconds(boostedDuration);

        agent.speed = defaultSpeed;
        idleDuration = 3f;
    }

    private void TriggerRagdoll()
    {
        SetLayerRecursively(gameObject, "dead");

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null)
            mainCollider.enabled = false;

        rb.isKinematic = false;

        animator.enabled = false;
        agent.enabled = false;

        foreach (Rigidbody rg in ragdoll.GetComponentsInChildren<Rigidbody>())
        {
            rg.isKinematic = false;
        }
    }

    private void SetLayerRecursively(GameObject obj, string layer)
    {
        obj.layer = LayerMask.NameToLayer(layer);
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Car")
        {
            Rigidbody carRb = collision.gameObject.GetComponent<Rigidbody>();

            if (carRb != null && carRb.linearVelocity.magnitude > 4f)
            {
                GameObject blood = Instantiate(bloodSplash, transform.position, transform.rotation);
                Destroy(blood, 0.5f);
                health = 0;
                dead = true;
                StopCoroutine(RandomMovement());
                TriggerRagdoll();
            }
        }
    }

    public void OnHeardShooting(Vector3 soundOrigin)
    {
        float distance = Vector3.Distance(transform.position, soundOrigin);
        if (distance <= hearingRange)
        {
            StopCoroutine("BoostSpeed");
            StartCoroutine(BoostSpeed());
        }
    }
}
