using UnityEngine;

namespace DayNightSystem
{
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 2f;
        [SerializeField] private float safeRestRadius = 1.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Public properties
        public bool PlayerInRange { get; private set; }
        public bool PlayerInSafeRestArea { get; private set; }
        public float DistanceToPlayer { get; private set; }
        public static SpawnPoint Current { get; private set; }
        
        // Events
        public System.Action OnPlayerEnteredSpawn;
        public System.Action OnPlayerLeftSpawn;
        public System.Action OnPlayerEnteredSafeArea;
        public System.Action OnPlayerLeftSafeArea;
        
        // Private fields
        private Transform playerTransform;
        private bool wasInSafeArea = false;
        
        private void Awake()
        {
            SetAsCurrentSpawnPoint();
            FindPlayer();
            
            if (showDebugLogs)
                Debug.Log($"[SpawnPoint] Initialized at position: {transform.position}");
        }
        
        private void Update()
        {
            CheckPlayerDistance();
        }
        
        private void SetAsCurrentSpawnPoint()
        {
            Current = this;
            
            if (showDebugLogs)
                Debug.Log($"[SpawnPoint] Set as current spawn point: {gameObject.name}");
        }
        
        private void FindPlayer()
        {
            // Try to find player by tag first
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // Fallback: find by PlayerMovement component
                var playerMovement = FindObjectOfType<PlayerController.PlayerMovement>();
                if (playerMovement != null)
                {
                    playerTransform = playerMovement.transform;
                }
                else
                {
                    Debug.LogWarning("[SpawnPoint] No player found in scene!");
                }
            }
        }
        
        private void CheckPlayerDistance()
        {
            if (playerTransform == null) return;
            
            DistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = PlayerInRange;
            bool wasInSafeArea = PlayerInSafeRestArea;
            
            PlayerInRange = DistanceToPlayer <= detectionRadius;
            PlayerInSafeRestArea = DistanceToPlayer <= safeRestRadius;
            
            // Check spawn range changes
            if (wasInRange != PlayerInRange)
            {
                if (PlayerInRange)
                {
                    OnPlayerEnteredSpawn?.Invoke();
                    if (showDebugLogs)
                        Debug.Log($"[SpawnPoint] Player entered spawn range. Distance: {DistanceToPlayer:F2}");
                }
                else
                {
                    OnPlayerLeftSpawn?.Invoke();
                    if (showDebugLogs)
                        Debug.Log($"[SpawnPoint] Player left spawn range. Distance: {DistanceToPlayer:F2}");
                }
            }
            
            // Check safe area changes
            if (wasInSafeArea != PlayerInSafeRestArea)
            {
                if (PlayerInSafeRestArea)
                {
                    OnPlayerEnteredSafeArea?.Invoke();
                    if (showDebugLogs)
                        Debug.Log($"[SpawnPoint] Player entered safe rest area. Distance: {DistanceToPlayer:F2}");
                }
                else
                {
                    OnPlayerLeftSafeArea?.Invoke();
                    if (showDebugLogs)
                        Debug.Log($"[SpawnPoint] Player left safe rest area. Distance: {DistanceToPlayer:F2}");
                }
            }
            
            // Check for rest when in safe area
            if (PlayerInSafeRestArea && !wasInSafeArea)
            {
                CheckForRest();
            }
        }
        
        private void CheckForRest()
        {
            // Find PlayerPenalty component
            var playerPenalty = playerTransform.GetComponent<DayNightSystem.PlayerPenalty>();
            if (playerPenalty != null && (playerPenalty.HasExhaustionPenalty || playerPenalty.HasFaintedPenalty))
            {
                // Player is exhausted/fainted and in safe rest area - allow rest
                playerPenalty.RemoveAllPenalties();
                
                if (showDebugLogs)
                    Debug.Log("[SpawnPoint] Player rested at spawn point - all penalties removed");
            }
        }
        
        public bool IsPlayerInSafeRestArea()
        {
            return PlayerInSafeRestArea;
        }
        
        public float GetDistanceToPlayer()
        {
            return DistanceToPlayer;
        }
        
        public bool CanPlayerRestHere()
        {
            return PlayerInSafeRestArea;
        }
        
        public void TeleportPlayerHere()
        {
            if (playerTransform == null)
            {
                Debug.LogError("[SpawnPoint] Cannot teleport player - no player reference found!");
                return;
            }
            
            playerTransform.position = transform.position;
            
            if (showDebugLogs)
                Debug.Log($"[SpawnPoint] Teleported player to spawn point: {transform.position}");
        }
        
        public Vector3 GetSpawnPosition()
        {
            return transform.position;
        }
        
        public static Vector3 GetCurrentSpawnPosition()
        {
            if (Current != null)
            {
                return Current.GetSpawnPosition();
            }
            
            Debug.LogWarning("[SpawnPoint] No current spawn point found - using Vector3.zero");
            return Vector3.zero;
        }
        
        public static bool IsPlayerAtSpawn()
        {
            return Current != null && Current.PlayerInSafeRestArea;
        }
        
        public static float GetDistanceToCurrentSpawn()
        {
            return Current != null ? Current.GetDistanceToPlayer() : float.MaxValue;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection radius
            Gizmos.color = PlayerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Draw safe rest area
            Gizmos.color = PlayerInSafeRestArea ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, safeRestRadius);
            
            // Draw spawn point indicator
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
        
        private void OnDrawGizmos()
        {
            // Always show spawn point location
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
}

// ScriptRole: Manages player spawn point and detection with safe rest area
// RelatedScripts: DayNightManager, PlayerPenalty
// UsesSO: None
// ReceivesFrom: None
// SendsTo: Player Transform (teleportation), Events for proximity detection
