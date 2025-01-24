using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    public Transform target;
    public Transform aimTarget;
    public Vector3 defaultOffset = new Vector3(0, 2, -5);
    public Vector3 aimingOffset = new Vector3(0, 2, -1);
    public float sensitivity = 5f;
    public float smoothTime = 0.3f;

    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private Vector3 currentVelocity = Vector3.zero;

    public float minPitch = -30f;
    public float maxPitch = 40f;

    public float aimingMinPitch = -10f;
    public float aimingMaxPitch = 20f;

    private bool isAiming = false;
    private PlayerMovement player;
    public bool onpc;

    void Start()
    {
        if (onpc)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        player = target.GetComponent<PlayerMovement>();
    }

    void LateUpdate()
    {
        if (!target) return;

        isAiming = player.isAiming;

        FollowTarget();

        if (!IsPointerOverUI())
        {
            RotateCamera();
        }
    }

    private void FollowTarget()
    {
        Transform currentTarget = isAiming ? aimTarget : target;

        Vector3 currentOffset = isAiming ? aimingOffset : defaultOffset;

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = currentTarget.position + rotation * currentOffset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        transform.LookAt(currentTarget.position + Vector3.up * 1.5f);
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;

        if (isAiming)
        {
            currentPitch = Mathf.Clamp(currentPitch, aimingMinPitch, aimingMaxPitch);
        }
        else
        {
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current != null)
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return false;
    }
}
