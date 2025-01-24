using UnityEngine;
using System.Collections.Generic;

public class CarAI : MonoBehaviour
{
    public Transform path;
    public float maxSteerAngle = 30f;
    public float acceleration = 1000f;
    public float maxSpeed = 50f;
    public float brakeForce = 3000f;
    public float waypointDistance = 1f;

    public float detectionDistance = 10f;

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    public bool flipFrontLeftWheel;
    public bool flipFrontRightWheel;
    public bool flipRearLeftWheel;
    public bool flipRearRightWheel;

    public Transform player;
    public float invisibilityDistance = 100f;
    public float distanceToPlayer;

    private List<Transform> nodes;
    public int currentNode = 0;
    private bool isBraking = false;
    private bool obstacleDetected = false;
    private Rigidbody rb;
    private float currentSpeed;
    private List<MeshRenderer> meshRenderers;
    public GameObject[] ped;
    Transform drivingPos;
    public GameObject driver;

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;
        path = FindFirstObjectByType<Path>().transform;
        drivingPos = transform.Find("DrivingPosition");
        driver = Instantiate(ped[RandomPed()], drivingPos.position, drivingPos.rotation);
        rb = GetComponent<Rigidbody>();

        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        foreach (Transform t in pathTransforms)
        {
            if (t != path.transform)
            {
                nodes.Add(t);
            }
        }

        currentNode = FindClosestWaypoint();

        if (nodes.Count > 0)
        {
            transform.LookAt(nodes[currentNode]);
        }

        meshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
    }

    int RandomPed()
    {
        int rand = Random.Range(0, ped.Length);
        return rand;
    }

    private int FindClosestWaypoint()
    {
        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, nodes[i].position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex + 1;
    }

    void FixedUpdate()
    {
        DetectObstacle();

        if (!obstacleDetected)
        {
            isBraking = false;
            ApplySteer();
            Drive();
            CheckWaypointDistance();
        }
        else
        {
            StopCar();
        }

        ApplyBrakes();
    }

    void Update()
    {
        UpdateWheelPosition(frontLeftWheel, frontLeftWheelTransform, flipFrontLeftWheel);
        UpdateWheelPosition(frontRightWheel, frontRightWheelTransform, flipFrontRightWheel);
        UpdateWheelPosition(rearLeftWheel, rearLeftWheelTransform, flipRearLeftWheel);
        UpdateWheelPosition(rearRightWheel, rearRightWheelTransform, flipRearRightWheel);

        UpdateDistanceToPlayer();

        driver.transform.position = drivingPos.position;
        driver.transform.rotation = drivingPos.rotation;
    }

    private void UpdateDistanceToPlayer()
    {
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        }
    }

    private void ApplySteer()
    {
        Vector3 carPosition = transform.position;
        Vector3 currentWaypoint = nodes[currentNode].position;
        Vector3 previousWaypoint = nodes[(currentNode == 0 ? nodes.Count - 1 : currentNode - 1)].position;

        Vector3 pathDirection = (currentWaypoint - previousWaypoint).normalized;
        Vector3 closestPointOnPath = Vector3.Project(carPosition - previousWaypoint, pathDirection) + previousWaypoint;

        Vector3 targetPoint = Vector3.Lerp(closestPointOnPath, currentWaypoint, 0.5f);
        Vector3 relativeVector = transform.InverseTransformPoint(targetPoint);

        float steer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;

        frontLeftWheel.steerAngle = steer;
        frontRightWheel.steerAngle = steer;
    }

    private void Drive()
    {
        currentSpeed = rb.linearVelocity.magnitude * 3.6f;

        if (currentSpeed < maxSpeed && !isBraking)
        {
            rearLeftWheel.motorTorque = acceleration;
            rearRightWheel.motorTorque = acceleration;
        }
        else
        {
            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;
        }
    }

    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode].position) < waypointDistance)
        {
            currentNode++;
            if (currentNode >= nodes.Count)
            {
                currentNode = 0;
            }
        }
    }

    private void DetectObstacle()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        float brakingDistance = Mathf.Pow(rb.linearVelocity.magnitude, 2) / (2 * (brakeForce / rb.mass));
        float adjustedDistance = Mathf.Max(brakingDistance, detectionDistance);

        RaycastHit hit;
        if (Physics.SphereCast(rayOrigin, 1f, transform.forward, out hit, adjustedDistance))
        {
            obstacleDetected = true;
        }
        else
        {
            obstacleDetected = false;
        }
    }

    private void StopCar()
    {
        if (obstacleDetected)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            isBraking = true;
            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;

            ApplyBrakes();
        }
    }

    private void ApplyBrakes()
    {
        if (isBraking)
        {
            frontLeftWheel.brakeTorque = brakeForce;
            frontRightWheel.brakeTorque = brakeForce;
            rearLeftWheel.brakeTorque = brakeForce;
            rearRightWheel.brakeTorque = brakeForce;
        }
        else
        {
            frontLeftWheel.brakeTorque = 0;
            frontRightWheel.brakeTorque = 0;
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }
    }

    private void UpdateWheelPosition(WheelCollider wheelCollider, Transform wheelTransform, bool flip)
    {
        Vector3 wheelPos;
        Quaternion wheelRot;
        wheelCollider.GetWorldPose(out wheelPos, out wheelRot);

        if (flip)
        {
            wheelRot *= Quaternion.Euler(0f, 180f, 0f);
        }

        wheelTransform.position = wheelPos;
        wheelTransform.rotation = wheelRot;
    }
}
