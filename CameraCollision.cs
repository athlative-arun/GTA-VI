using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float smoothTime = 0.3f;
    public float collisionRadius = 0.5f;
    public float minDistance = 2f;
    public float maxDistance = 5f;

    private Vector3 currentVelocity = Vector3.zero;
    private float currentYaw = 0f;
    private float currentPitch = 0f;

    public float minPitch = -30f;
    public float maxPitch = 60f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentYaw += mouseX;
        currentPitch -= mouseY;

        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        Vector3 desiredPosition = target.position + rotation * offset;

        Vector3 direction = desiredPosition - target.position;
        RaycastHit hit;

        if (Physics.Raycast(target.position, direction.normalized, out hit, direction.magnitude))
        {
            float hitDistance = Mathf.Clamp(hit.distance - collisionRadius, minDistance, maxDistance);
            desiredPosition = target.position + direction.normalized * hitDistance;
        }
        else
        {
            float distance = Vector3.Distance(target.position, desiredPosition);
            float clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);

            desiredPosition = target.position + direction.normalized * clampedDistance;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
