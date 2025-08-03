using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpHeight = 1f;
    public float gravity = -9.81f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    
    [Header("References")]
    public Transform cameraTransform;
    public FeedbackMessagesSO feedbackMessages;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private float speedMultiplier = 1f;
    private bool canMove = true;
    private bool canLook = true;
    
    // Events
    public System.Action OnPlayerJump;
    public System.Action OnPlayerLand;
    public System.Action OnPlayerDeath;
    public System.Action OnPlayerInteract;

    private void Awake()
    {
        Debug.Log("PlayerFPSController: Initializing");
        controller = GetComponent<CharacterController>();
        
        if (cameraTransform == null)
        {
            Debug.LogWarning("PlayerFPSController: Camera not assigned, using main camera");
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        Debug.Log("PlayerFPSController: Starting FPS controller");
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !canMove)
            return;

        HandleMovement();
        HandleMouseLook();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        // Run input
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Calculate movement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * speedMultiplier * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Debug.Log("PlayerFPSController: Player jumping");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            OnPlayerJump?.Invoke();
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (!canLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        if (cameraTransform != null)
        {
            float currentRotationX = cameraTransform.localEulerAngles.x;
            if (currentRotationX > 180f)
                currentRotationX -= 360f;
            
            float newRotationX = Mathf.Clamp(currentRotationX - mouseY, -maxLookAngle, maxLookAngle);
            cameraTransform.localEulerAngles = new Vector3(newRotationX, 0f, 0f);
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("PlayerFPSController: Interaction key pressed");
            OnPlayerInteract?.Invoke();
            
            // Raycast for interactable objects
            if (cameraTransform != null)
            {
                RaycastHit hit;
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 3f))
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        canLook = enabled;
        
        if (!enabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetLookEnabled(bool enabled)
    {
        canLook = enabled;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Clamp01(multiplier);
        Debug.Log($"PlayerFPSController: Speed multiplier set to {speedMultiplier}");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check for hazards or enemies
        if (hit.gameObject.CompareTag("Hazard") || hit.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("PlayerFPSController: Hit hazard/enemy");
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WinZone"))
        {
            Debug.Log("PlayerFPSController: Reached win zone");
            GameManager.Instance.GameWin();
        }
        else if (other.CompareTag("DeathZone"))
        {
            Debug.Log("PlayerFPSController: Entered death zone");
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("PlayerFPSController: Player died");
        OnPlayerDeath?.Invoke();
        GameManager.Instance.GameOver();
        
        // Disable movement
        SetMovementEnabled(false);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && canMove)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

// Interface for interactable objects
public interface IInteractable
{
    void Interact();
}

// ScriptRole: Handles first-person movement, camera control and interaction
// Dependencies: CharacterController
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: Input system, GameManager
// SendsTo: GameManager via events, interactable objects 