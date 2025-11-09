using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Simple first/third-person player controller using CharacterController.
/// Handles WASD movement, mouse look, and basic gravity.
/// Supports both Unity legacy Input and the new Input System.
/// Now includes Animator support for Idle/Walk animations.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    public float moveSpeed = 5f;

    [Tooltip("Rotation speed for mouse look")]
    public float rotationSpeed = 2f;

    [Tooltip("Gravity force applied to player")]
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    [Tooltip("Optional camera transform to follow player (can be child)")]
    public Transform playerCamera;

    [Tooltip("Smooth camera follow speed")]
    public float cameraSmoothSpeed = 10f;

    [Tooltip("Camera height offset from player")]
    public float cameraHeightOffset = 1.5f;

    [Tooltip("Camera distance behind player")]
    public float cameraDistanceOffset = 3f;

    [Header("Look Settings")]
    [Tooltip("Minimum vertical look angle")]
    public float minLookAngle = -60f;

    [Tooltip("Maximum vertical look angle")]
    public float maxLookAngle = 60f;

    [Header("Animation")]
    [Tooltip("Player animator component (optional, for Idle/Walk animations)")]
    public Animator playerAnimator;

    private CharacterController characterController;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
         playerCamera = Camera.main?.transform;
       if (playerCamera == null)
       {
   Debug.LogWarning("PlayerController: No camera assigned and no Main Camera found!");
  }
        }

      // Auto-find animator if not assigned
    if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }

        // Disable root motion if animator exists
  if (playerAnimator != null)
  {
 playerAnimator.applyRootMotion = false;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleCameraFollow();
    }

    /// <summary>
    /// Handles WASD movement and gravity.
    /// Supports both legacy Input and new Input System.
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
      // New Input System
        if (Keyboard.current != null)
        {
       if (Keyboard.current.aKey.isPressed) horizontal = -1f;
            if (Keyboard.current.dKey.isPressed) horizontal = 1f;
      if (Keyboard.current.wKey.isPressed) vertical = 1f;
   if (Keyboard.current.sKey.isPressed) vertical = -1f;
        }
#else
        // Legacy Input Manager (default)
     horizontal = Input.GetAxis("Horizontal");
     vertical = Input.GetAxis("Vertical");
#endif

        // Calculate movement direction relative to player rotation
   Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();

        // Apply movement
        Vector3 move = moveDirection * moveSpeed * Time.deltaTime;

        // Apply gravity
        if (characterController.isGrounded && velocity.y < 0)
        {
 velocity.y = -2f; // Small value to keep grounded
        }
        velocity.y += gravity * Time.deltaTime;
        move.y = velocity.y * Time.deltaTime;

   // Move the character
 characterController.Move(move);

        // Update animator
        bool isMoving = moveDirection.magnitude > 0.1f;
 if (playerAnimator != null)
     {
            playerAnimator.SetBool("isWalking", isMoving);
        }
    }

    /// <summary>
    /// Handles mouse look for player rotation.
    /// Supports both legacy Input and new Input System.
    /// </summary>
    private void HandleRotation()
    {
        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System
        if (Mouse.current != null)
        {
        Vector2 delta = Mouse.current.delta.ReadValue();
   mouseX = delta.x * 0.01f * rotationSpeed;
            mouseY = delta.y * 0.01f * rotationSpeed;
  }
#else
      // Legacy Input Manager (default)
        mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
#endif

 // Rotate player horizontally
transform.Rotate(Vector3.up * mouseX);

        // Update vertical camera rotation
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minLookAngle, maxLookAngle);
    }

    /// <summary>
    /// Smoothly positions the camera behind and above the player.
    /// </summary>
    private void HandleCameraFollow()
    {
        if (playerCamera == null) return;

        // Calculate desired camera position
        Vector3 desiredPosition = transform.position
        - transform.forward * cameraDistanceOffset
        + Vector3.up * cameraHeightOffset;

        // Smooth camera movement
 playerCamera.position = Vector3.Lerp(
            playerCamera.position,
            desiredPosition,
          cameraSmoothSpeed * Time.deltaTime
        );

        // Calculate look direction with pitch
        Quaternion pitchRotation = Quaternion.Euler(cameraPitch, 0, 0);
        Quaternion desiredRotation = transform.rotation * pitchRotation;

        // Smooth camera rotation
        playerCamera.rotation = Quaternion.Lerp(
            playerCamera.rotation,
        desiredRotation,
        cameraSmoothSpeed * Time.deltaTime
        );
    }

    /// <summary>
 /// Public method to enable/disable player controls.
    /// </summary>
    public void SetControlsEnabled(bool enabled)
{
        this.enabled = enabled;

        if (enabled)
        {
    Cursor.lockState = CursorLockMode.Locked;
         Cursor.visible = false;
        }
    }
}
