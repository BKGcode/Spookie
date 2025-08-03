using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    
    [Header("Detection")]
    public float detectionRange = 5f;
    public LayerMask playerLayerMask = 1;
    
    [Header("References")]
    public Transform groundCheck;
    public Transform wallCheck;
    public FeedbackMessagesSO feedbackMessages;
    
    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool movingRight = true;
    private bool playerDetected = false;
    private Transform playerTransform;
    
    // Events
    public System.Action OnEnemyDeath;

    private void Awake()
    {
        Debug.Log("EnemyController: Initializing");
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        targetPosition = startPosition + Vector2.right * patrolDistance;
        
        if (groundCheck == null)
        {
            Debug.LogWarning("EnemyController: GroundCheck not assigned, creating one");
            GameObject check = new GameObject("GroundCheck");
            check.transform.SetParent(transform);
            check.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = check.transform;
        }
        
        if (wallCheck == null)
        {
            Debug.LogWarning("EnemyController: WallCheck not assigned, creating one");
            GameObject check = new GameObject("WallCheck");
            check.transform.SetParent(transform);
            check.transform.localPosition = Vector3.right * 0.5f;
            wallCheck = check.transform;
        }
    }

    private void Start()
    {
        Debug.Log("EnemyController: Starting enemy patrol");
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
            return;

        CheckForPlayer();
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsGamePlaying())
            return;

        if (playerDetected)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void CheckForPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayerMask);
        
        if (playerCollider != null && playerCollider.CompareTag("Player"))
        {
            if (!playerDetected)
            {
                Debug.Log("EnemyController: Player detected!");
                playerDetected = true;
                playerTransform = playerCollider.transform;
            }
        }
        else
        {
            if (playerDetected)
            {
                Debug.Log("EnemyController: Player lost");
                playerDetected = false;
                playerTransform = null;
            }
        }
    }

    private void Patrol()
    {
        // Check if reached patrol boundary
        if (movingRight && transform.position.x >= targetPosition.x)
        {
            Flip();
            targetPosition = startPosition;
        }
        else if (!movingRight && transform.position.x <= startPosition.x)
        {
            Flip();
            targetPosition = startPosition + Vector2.right * patrolDistance;
        }
        
        // Check for obstacles
        if (IsGrounded() && !IsWallAhead())
        {
            Vector2 direction = movingRight ? Vector2.right : Vector2.left;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            Flip();
        }
    }

    private void ChasePlayer()
    {
        if (playerTransform == null) return;
        
        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
        
        if (IsGrounded() && !IsWallAhead())
        {
            rb.linearVelocity = new Vector2(direction * moveSpeed * 1.5f, rb.linearVelocity.y);
            
            // Update facing direction
            if (direction > 0 && !movingRight)
            {
                Flip();
            }
            else if (direction < 0 && movingRight)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        
        Debug.Log($"EnemyController: Flipped direction, now moving {(movingRight ? "right" : "left")}");
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, LayerMask.GetMask("Ground"));
    }

    private bool IsWallAhead()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, LayerMask.GetMask("Ground"));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("EnemyController: Hit player");
            // Player will handle the collision in PlayerController
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Debug.Log("EnemyController: Hit by player attack");
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("EnemyController: Enemy died");
        OnEnemyDeath?.Invoke();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw patrol area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(startPosition + Vector2.right * patrolDistance * 0.5f, 
                           new Vector3(patrolDistance, 1, 1));
        
        // Draw detection range
        Gizmos.color = playerDetected ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw ground and wall checks
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
        
        if (wallCheck != null)
        {
            Gizmos.color = IsWallAhead() ? Color.red : Color.green;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }
    }
}

// ScriptRole: Handles enemy AI, movement and player detection
// Dependencies: Rigidbody2D, Collider2D
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: Player collision
// SendsTo: None (destroys self on death) 