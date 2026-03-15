using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gamepadLookSpeed = 120f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        jumpAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        jumpAction?.action.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        bool isGamepad = Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.0001f;

        float lookX;
        float lookY;

        if (isGamepad)
        {
            lookX = lookInput.x * gamepadLookSpeed * Time.deltaTime;
            lookY = lookInput.y * gamepadLookSpeed * Time.deltaTime;
        }
        else
        {
            lookX = lookInput.x * mouseSensitivity;
            lookY = lookInput.y * mouseSensitivity;
        }

        // Rotazione orizzontale del corpo
        transform.Rotate(Vector3.up * lookX);

        // Rotazione verticale della camera
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

        // Se siamo a terra e stiamo cadendo, manteniamo una piccola spinta verso il basso
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Salto
        if (jumpAction.action.WasPressedThisFrame() && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravità
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}