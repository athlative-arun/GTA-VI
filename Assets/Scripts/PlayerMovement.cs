using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Animator animator;

    private Vector3 moveDirection;
    private Vector3 velocity;
    private float currentSpeed;
    public bool isAiming = false, mobileAiming;
    private bool isSprinting = false; // Tracks sprint toggle state

    [Header("Aiming Settings")]
    public Transform upperBodyBone;
    public bool onpc;

    public DynamicJoystick joystick;
    

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleRotation();
        HandleAnimations();
        HandleMovementInput();
        HandleAiming();

        float targetWeight = isAiming ? 1f : 0f;
        animator.SetLayerWeight(
            animator.GetLayerIndex("AimingLayer"),
            Mathf.Lerp(
                animator.GetLayerWeight(animator.GetLayerIndex("AimingLayer")),
                targetWeight,
                Time.deltaTime * 5f // Adjust speed as needed
            )
        );
        animator.SetBool("IsAiming", isAiming);
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift))
        {
            ToggleSprint();
        }
    }

    void LateUpdate()
    {
        if (isAiming && upperBodyBone != null)
        {
            float pitch = cameraTransform.eulerAngles.x;
            if (pitch > 180) pitch -= 360; // Normalize pitch

            Quaternion targetRotation = Quaternion.Euler(pitch, 0f, 0f);
            upperBodyBone.localRotation = targetRotation; // Ensure manual rotation is applied last
        }
    }

    void HandleMovement()
    {
        // Get inputs from both joystick and keyboard
        float horizontal = Input.GetAxis("Horizontal") + joystick.Horizontal;
        float vertical = Input.GetAxis("Vertical") + joystick.Vertical;

        // Determine movement direction relative to the camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        moveDirection = forward * vertical + right * horizontal;

        // Determine movement speed based on input and aiming
        if (isAiming)
        {
            currentSpeed = walkSpeed; // Disable sprinting while aiming
        }
        else if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Move the character
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f; // Reset vertical velocity when on the ground
        }

        velocity.y += gravity * Time.deltaTime; // Apply gravity
        controller.Move(velocity * Time.deltaTime); // Apply vertical movement
    }

    void HandleRotation()
    {
        if (isAiming)
        {
            // If aiming, make the player face the camera's forward direction
            Quaternion targetRotation = Quaternion.LookRotation(cameraTransform.forward);
            targetRotation.x = 0f; // Ignore x rotation (prevent tilting)
            targetRotation.z = 0f; // Ignore z rotation (prevent tilting)
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else if (moveDirection != Vector3.zero)
        {
            // Rotate to face movement direction if not aiming
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleAnimations()
    {
        if (animator != null)
        {
            float speedPercent = moveDirection.magnitude * (currentSpeed == sprintSpeed ? 1f : 0.5f);
            animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
        }
    }

    void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal") + joystick.Horizontal;
        float vertical = Input.GetAxis("Vertical") + joystick.Vertical;

        // Smooth transition by passing gradual values for the animator
        if (animator != null)
        {
            animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
            animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);
        }
    }

    void HandleAiming()
    {
        if (onpc)
        {
            if (Input.GetMouseButton(1)) // Right mouse button for aiming
            {
                isAiming = true;
            }
            else if (!mobileAiming)
            {
                isAiming = false;
            }
        }
    }
    public void ToggleSprint()
    {
        isSprinting = !isSprinting;
    }

    public void Aim()
    {
        mobileAiming = !mobileAiming;
        isAiming = !isAiming;
        animator.SetBool("IsAiming", isAiming);
    }
}
