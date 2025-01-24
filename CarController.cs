using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float acceleration = 1000f;
    public float maxSpeed = 50f;
    public float steeringAngle = 30f;
    public float brakeForce = 3000f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Transforms")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    [Header("Wheel Flip Settings")]
    public bool flipFrontLeft;
    public bool flipFrontRight;
    public bool flipRearLeft;
    public bool flipRearRight;

    [Header("Live Data")]
    public float speed;

    private float inputVertical;
    private float inputHorizontal;
    private bool isBraking;
    public float mobileControls, mobileControls2;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
        isBraking = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.F);

        UpdateWheelPositions();
        UpdateSpeed();
    }

    void FixedUpdate()
    {
        Drive();
        Steer();
        Brake();
    }

    public void Drive()
    {
        float effectiveInput = mobileControls != 0 ? mobileControls : inputVertical;
        float motorTorque = effectiveInput * acceleration;

        rearLeftWheel.motorTorque = motorTorque;
        rearRightWheel.motorTorque = motorTorque;

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;
        }
    }

    private void Steer()
    {
        float steer = (inputHorizontal + mobileControls2) * steeringAngle;

        frontLeftWheel.steerAngle = steer;
        frontRightWheel.steerAngle = steer;
    }

    private void Brake()
    {
        if (isBraking)
        {
            ApplyBrakes();
        }
        else
        {
            frontLeftWheel.brakeTorque = 0;
            frontRightWheel.brakeTorque = 0;
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }
    }

    public void ApplyBrakes()
    {
        frontLeftWheel.brakeTorque = brakeForce;
        frontRightWheel.brakeTorque = brakeForce;
        rearLeftWheel.brakeTorque = brakeForce;
        rearRightWheel.brakeTorque = brakeForce;
    }

    private void UpdateWheelPositions()
    {
        UpdateWheelPosition(frontLeftWheel, frontLeftTransform, flipFrontLeft);
        UpdateWheelPosition(frontRightWheel, frontRightTransform, flipFrontRight);
        UpdateWheelPosition(rearLeftWheel, rearLeftTransform, flipRearLeft);
        UpdateWheelPosition(rearRightWheel, rearRightTransform, flipRearRight);
    }

    private void UpdateWheelPosition(WheelCollider collider, Transform wheelTransform, bool shouldFlip)
    {
        Vector3 pos;
        Quaternion rot;

        collider.GetWorldPose(out pos, out rot);

        if (shouldFlip)
        {
            rot *= Quaternion.Euler(0, 180, 0);
        }

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void UpdateSpeed()
    {
        speed = rb.linearVelocity.magnitude * 3.6f;

        if (Mathf.Abs(inputVertical) < 0.01f && speed < 10f)
        {
            Vector3 currentVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * 2f);
        }
    }

    public void MobileControls(float dir)
    {
        mobileControls = dir;
    }

    public void Steer(float dir)
    {
        mobileControls2 = dir;
    }
}
