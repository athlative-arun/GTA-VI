using UnityEngine;
using UnityEngine.EventSystems;

public class CarCameraController : MonoBehaviour
{
    public Transform car;
    public float distance = 5f;
    public float height = 2f;
    public float rotationSpeed = 5f;
    public float followSpeed = 10f;

    public float autoCenterDelay = 2f;
    public float autoCenterSpeed = 3f;

    public Vector3 centerOffset = new Vector3(0, 2f, -5f);
    private Vector3 defaultCenterOffset;

    public bool isReversing = false;
    private float reverseTimer = 0f;

    public bool isManuallyControlled = false;

    private Vector3 offset;
    private float yaw;
    private float pitch;

    private float idleTime = 0f;

    public float minPitch = -20f;
    public float maxPitch = 60f;

    private void Start()
    {
        offset = centerOffset;
        defaultCenterOffset = centerOffset;
    }

    public void SetCar(GameObject nearcar)
    {
        car = nearcar.GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        if (car == null) return;

        HandleReversing();
        HandleMouseRotation();
        FollowCar();
        AutoCenterCamera();
    }

    private void FollowCar()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = car.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(car.position + Vector3.up * 1f);
    }

    private void HandleMouseRotation()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.1f || Mathf.Abs(mouseY) > 0.1f)
        {
            isManuallyControlled = true;
            idleTime = 0f;
            yaw += mouseX * rotationSpeed;
            pitch -= mouseY * rotationSpeed;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            idleTime += Time.deltaTime;
        }
    }

    private void AutoCenterCamera()
    {
        if (!isReversing && idleTime > autoCenterDelay)
        {
            isManuallyControlled = false;

            yaw = Mathf.LerpAngle(yaw, car.eulerAngles.y, autoCenterSpeed * Time.deltaTime);
            pitch = Mathf.Lerp(pitch, 0, autoCenterSpeed * Time.deltaTime);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPosition = car.position + rotation * centerOffset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.LookAt(car.position + Vector3.up * 1f);
        }
    }

    private void HandleReversing()
    {
        Rigidbody carRigidbody = car.GetComponent<Rigidbody>();
        if (carRigidbody == null) return;

        Vector3 carVelocity = carRigidbody.linearVelocity;
        float forwardDot = Vector3.Dot(car.forward, carVelocity.normalized);

        float targetZ = defaultCenterOffset.z;

        if (carVelocity.magnitude > 0.1f && forwardDot < -0.5f)
        {
            reverseTimer += Time.deltaTime;
            if (reverseTimer > 2f)
            {
                isReversing = true;
                targetZ = Mathf.Abs(defaultCenterOffset.z);
            }
        }
        else
        {
            reverseTimer = 0f;
            isReversing = false;
        }

        offset = new Vector3(
            defaultCenterOffset.x,
            defaultCenterOffset.y,
            Mathf.Lerp(offset.z, targetZ, 1.5f * Time.deltaTime)
        );
    }
}
