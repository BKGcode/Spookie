using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask = 1;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private bool jumpInput;
    
    // Events
    public System.Action OnPlayerJump;
    public System.Action OnPlayerLand;
    public System.Action OnPlayerDeath;

    private void Awake()
    {
        Debug.Log("PlayerController: Initializing");
        rb = GetComponent<Rigidbody2D>();
        
        if (groundCheck == null)
        {
            Debug.LogWarning("PlayerController: GroundCheck not assigned, creating one");
            GameObject check = new GameObject("GroundCheck");
            check.transform.SetParent(transform);
            check.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = check.transform;
        }
    }

    private void Start()
    {
        Debug.Log("PlayerController: Starting player controller");
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
            return;

        HandleInput();
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsGamePlaying())
            return;

        HandleMovement();
        HandleJump();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetButtonDown("Jump");
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        if (!wasGrounded && isGrounded)
        {
            Debug.Log("PlayerController: Player landed");
            OnPlayerLand?.Invoke();
        }
    }

    private void HandleMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;
        rb.linearVelocity = velocity;
        
        // Flip sprite based on direction
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }
    }

    private void HandleJump()
    {
        if (jumpInput && isGrounded)
        {
            Debug.Log("PlayerController: Player jumping");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            OnPlayerJump?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if player hit enemy or hazard
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hazard"))
        {
            Debug.Log("PlayerController: Player hit enemy/hazard");
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for collectibles or win conditions
        if (other.CompareTag("Collectible"))
        {
            Debug.Log("PlayerController: Collected item");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("WinZone"))
        {
            Debug.Log("PlayerController: Reached win zone");
            GameManager.Instance.GameWin();
        }
    }

    private void Die()
    {
        Debug.Log("PlayerController: Player died");
        OnPlayerDeath?.Invoke();
        GameManager.Instance.GameOver();
        
        // Disable player movement
        enabled = false;
        rb.simulated = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

// ScriptRole: Handles player movement, jumping and collision detection
// Dependencies: Rigidbody2D, Collider2D
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: Input system, GameManager
// SendsTo: GameManager via events 