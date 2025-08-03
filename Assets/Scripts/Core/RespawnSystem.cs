using UnityEngine;
using System.Collections;

public class RespawnSystem : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;
    [Tooltip("Y position below which player respawns. Adjust based on terrain height.")]
    public float fallThreshold = -10f; // Y position below which player respawns
    public float respawnDelay = 1f;
    public bool autoRespawn = true;
    [Tooltip("Minimum safe height above fall threshold to update last safe position")]
    public float safeHeightBuffer = 1f;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private PlayerFPSController playerController;
    private Vector3 lastSafePosition;
    private bool isRespawning = false;
    
    // Public method to set player controller reference
    public void SetPlayerController(PlayerFPSController controller)
    {
        playerController = controller;
        if (playerController != null)
        {
            lastSafePosition = playerController.transform.position;
        }
    }

    private void Start()
    {
        Debug.Log("RespawnSystem: Initializing");
        
        // Find player controller
        playerController = FindObjectOfType<PlayerFPSController>();
        
        if (playerController != null)
        {
            lastSafePosition = playerController.transform.position;
        }
        
        if (respawnPoint == null)
        {
            Debug.LogWarning("RespawnSystem: No respawn point assigned, using player start position");
            if (playerController != null)
            {
                respawnPoint = playerController.transform;
            }
        }
    }

    private void Update()
    {
        if (playerController == null || isRespawning) return;
        
        // Check if player fell below threshold
        if (playerController.transform.position.y < fallThreshold)
        {
            Debug.Log("RespawnSystem: Player fell below threshold, respawning");
            StartRespawn();
        }
        
        // Update last safe position when player is on ground
        if (playerController.transform.position.y > fallThreshold + safeHeightBuffer)
        {
            lastSafePosition = playerController.transform.position;
        }
    }

    public void StartRespawn()
    {
        if (isRespawning) return;
        
        Debug.Log("RespawnSystem: Starting respawn");
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        
        // Disable player movement
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
        
        // Show respawn message
        if (feedbackMessages != null)
        {
            string message = feedbackMessages.GetMessage("respawning_message");
            Debug.Log($"RespawnSystem: {message}");
        }
        
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawn player
        if (playerController != null)
        {
            Vector3 respawnPosition = respawnPoint != null ? respawnPoint.position : lastSafePosition;
            playerController.transform.position = respawnPosition;
            
            Debug.Log($"RespawnSystem: Player respawned at {respawnPosition}");
        }
        
        // Re-enable player movement
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }
        
        isRespawning = false;
    }

    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
        Debug.Log($"RespawnSystem: Respawn point set to {newRespawnPoint.name}");
    }

    public void SetLastSafePosition(Vector3 position)
    {
        lastSafePosition = position;
        Debug.Log($"RespawnSystem: Last safe position updated to {position}");
    }

    public Vector3 GetRespawnPosition()
    {
        return respawnPoint != null ? respawnPoint.position : lastSafePosition;
    }

    public bool IsRespawning()
    {
        return isRespawning;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw fall threshold
        Gizmos.color = Color.red;
        Vector3 thresholdPosition = transform.position;
        thresholdPosition.y = fallThreshold;
        Gizmos.DrawLine(thresholdPosition + Vector3.left * 10f, thresholdPosition + Vector3.right * 10f);
        
        // Draw respawn point
        if (respawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(respawnPoint.position, 1f);
        }
        
        // Draw last safe position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(lastSafePosition, 0.5f);
    }
}

// ScriptRole: Handles player respawn when falling or getting stuck
// Dependencies: PlayerFPSController
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: PlayerFPSController
// SendsTo: PlayerFPSController (disables/enables movement during respawn) 