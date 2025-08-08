using UnityEngine;
using DayNightSystem.Core;

namespace DayNightSystem
{
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 2f;
        [SerializeField] private float safeRestRadius = 1.5f;
        [SerializeField] private float warningRadius = 3f; // New: Warning zone
        
        [Header("Distance Indicators")]
        [SerializeField] private bool showDistanceIndicator = true;
        [SerializeField] private float distanceUpdateInterval = 0.5f; // Update distance display every 0.5s
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Public properties
        public bool PlayerInRange { get; private set; }
        public bool PlayerInSafeRestArea { get; private set; }
        public bool PlayerInWarningZone { get; private set; } // New: Warning zone
        public float DistanceToPlayer { get; private set; }
        public float DistancePercentage { get; private set; } // New: Distance as percentage
        public static SpawnPoint Current { get; private set; }
        
        // Events
        public System.Action OnPlayerEnteredSpawn;
        public System.Action OnPlayerLeftSpawn;
        public System.Action OnPlayerEnteredSafeArea;
        public System.Action OnPlayerLeftSafeArea;
        public System.Action<float> OnDistanceChanged; // New: Distance change events
        public System.Action OnPlayerEnteredWarningZone; // New: Warning zone events
        public System.Action OnPlayerLeftWarningZone;
        
        // Private fields
        private Transform playerTransform;
        private bool wasInSafeArea = false;
        private bool wasInWarningZone = false; // New: Track warning zone state
        private float lastDistanceUpdate = 0f; // New: Track distance update timing
        private float lastReportedDistance = 0f; // New: Track last reported distance
        
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
            DistancePercentage = Mathf.Clamp01(DistanceToPlayer / warningRadius); // Calculate percentage
            
            bool wasInRange = PlayerInRange;
            bool wasInSafeArea = PlayerInSafeRestArea;
            
            PlayerInRange = DistanceToPlayer <= detectionRadius;
            PlayerInSafeRestArea = DistanceToPlayer <= safeRestRadius;
            PlayerInWarningZone = DistanceToPlayer <= warningRadius;
            
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
            
            // Check warning zone changes
            if (wasInWarningZone != PlayerInWarningZone)
            {
                if (PlayerInWarningZone)
                {
                    OnPlayerEnteredWarningZone?.Invoke();
                    if (showDebugLogs)
                        Debug.Log($"[SpawnPoint] Player entered warning zone. Distance: {DistanceToPlayer:F2}");
                }
                else
                {
                    OnPlayerLeftWarningZone?.Invoke();
                    if (showDebugLogs)
                        Debug.Log($"[SpawnPoint] Player left warning zone. Distance: {DistanceToPlayer:F2}");
                }
            }
            
            // Update distance indicator at intervals
            if (showDistanceIndicator && Time.time - lastDistanceUpdate >= distanceUpdateInterval)
            {
                float distanceChange = Mathf.Abs(DistanceToPlayer - lastReportedDistance);
                if (distanceChange >= 0.1f) // Only report if distance changed significantly
                {
                    OnDistanceChanged?.Invoke(DistanceToPlayer);
                    lastReportedDistance = DistanceToPlayer;
                    
                    if (showDebugLogs)
                        Debug.Log($"[SpawnPoint] Distance updated: {DistanceToPlayer:F2}m ({DistancePercentage:P0})");
                }
                lastDistanceUpdate = Time.time;
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
            if (playerPenalty != null && playerPenalty.CurrentPenaltyType != PenaltyType.None)
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
        
        // New: Get distance percentage
        public static float GetDistancePercentageToCurrentSpawn()
        {
            return Current != null ? Current.DistancePercentage : 1f;
        }
        
        // New: Check if player is in warning zone
        public static bool IsPlayerInWarningZone()
        {
            return Current != null && Current.PlayerInWarningZone;
        }
        
        // New: Get distance description
        public string GetDistanceDescription()
        {
            if (DistanceToPlayer <= safeRestRadius)
                return "Safe Rest Area";
            else if (DistanceToPlayer <= detectionRadius)
                return "Spawn Range";
            else if (DistanceToPlayer <= warningRadius)
                return "Warning Zone";
            else
                return $"Far ({DistanceToPlayer:F1}m)";
        }
        
        // New: Get distance description with messages
        public string GetDistanceDescriptionWithMessages()
        {
            if (DistanceToPlayer <= safeRestRadius)
                return "safe_rest_area";
            else if (DistanceToPlayer <= detectionRadius)
                return "spawn_range";
            else if (DistanceToPlayer <= warningRadius)
                return "warning_zone";
            else
                return "far_from_spawn";
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw warning radius (outermost)
            Gizmos.color = PlayerInWarningZone ? new Color(1f, 0.5f, 0f) : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, warningRadius);
            
            // Draw detection radius
            Gizmos.color = PlayerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Draw safe rest area (innermost)
            Gizmos.color = PlayerInSafeRestArea ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, safeRestRadius);
            
            // Draw spawn point indicator
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            
            // Draw distance line to player if in scene view
            if (playerTransform != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }
        }
        
        private void OnDrawGizmos()
        {
            // Always show spawn point location
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
        
        // Context menu methods for testing
        [ContextMenu("Test Distance Description")]
        public void TestDistanceDescription()
        {
            Debug.Log($"[SpawnPoint] Distance Description: {GetDistanceDescription()}");
            Debug.Log($"[SpawnPoint] Distance: {DistanceToPlayer:F2}m, Percentage: {DistancePercentage:P0}");
            Debug.Log($"[SpawnPoint] In Warning Zone: {PlayerInWarningZone}, In Range: {PlayerInRange}, In Safe Area: {PlayerInSafeRestArea}");
        }
        
        [ContextMenu("Test Distance Events")]
        public void TestDistanceEvents()
        {
            OnDistanceChanged?.Invoke(DistanceToPlayer);
            Debug.Log($"[SpawnPoint] Triggered distance change event: {DistanceToPlayer:F2}m");
        }
        
        [ContextMenu("Test Warning Zone")]
        public void TestWarningZone()
        {
            if (PlayerInWarningZone)
            {
                OnPlayerEnteredWarningZone?.Invoke();
                Debug.Log("[SpawnPoint] Triggered warning zone entered event");
            }
            else
            {
                OnPlayerLeftWarningZone?.Invoke();
                Debug.Log("[SpawnPoint] Triggered warning zone left event");
            }
        }
    }
}

// ScriptRole: Manages player spawn point with enhanced distance indicators and warning zones
// RelatedScripts: DayNightManager, PlayerPenalty, DayNightUI
// UsesSO: None
// ReceivesFrom: None
// SendsTo: Player Transform (teleportation), Events for proximity detection and distance updates
