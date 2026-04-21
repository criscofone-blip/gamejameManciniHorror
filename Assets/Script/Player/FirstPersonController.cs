using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckOffset = 0.05f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Head Bob")]
    [SerializeField] private float bobFrequency = 8f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobReturnSpeed = 8f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch;
    private bool isOnTheFloor;

    private Vector3 cameraStartLocalPosition;
    private float bobTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraStartLocalPosition = playerCamera.transform.localPosition;
    }

    private void Update()
    {
        CheckIfOnFloor();
        HandleLook();
        HandleMovement();
        HandleHeadBob();
    }

    private void CheckIfOnFloor()
    {
        Vector3 bottom = transform.position
                       + controller.center
                       - Vector3.up * (controller.height * 0.5f);

        Vector3 checkPosition = bottom + Vector3.down * groundCheckOffset;

        isOnTheFloor = Physics.CheckSphere(
            checkPosition,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        float lookX = lookInput.x * mouseSensitivity;
        float lookY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * lookX);

        cameraPitch -= lookY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (isOnTheFloor && velocity.y < 0f)
            velocity.y = -2f;

        if (jumpAction.action.WasPressedThisFrame() && isOnTheFloor)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        Vector3 targetLocalPosition = cameraStartLocalPosition;

        if (isMoving && isOnTheFloor)
        {
            bobTimer += Time.deltaTime * bobFrequency;

            float bobOffsetY = Mathf.Sin(bobTimer) * bobAmplitude;
            targetLocalPosition.y += bobOffsetY;
        }
        else
        {
            bobTimer = 0f;
        }

        Vector3 currentLocalPosition = playerCamera.transform.localPosition;

        currentLocalPosition.x = cameraStartLocalPosition.x;
        currentLocalPosition.z = cameraStartLocalPosition.z;
        currentLocalPosition.y = Mathf.Lerp(
            currentLocalPosition.y,
            targetLocalPosition.y,
            Time.deltaTime * bobReturnSpeed
        );

        playerCamera.transform.localPosition = currentLocalPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (!controller)
            controller = GetComponent<CharacterController>();

        Vector3 bottom = transform.position
                       + controller.center
                       - Vector3.up * (controller.height * 0.5f);

        Vector3 checkPosition = bottom + Vector3.down * groundCheckOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
    }
}