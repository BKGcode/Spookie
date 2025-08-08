using UnityEngine;
using DayNightSystem.Core;

namespace DayNightSystem.Validation
{
    public class DayNightValidationController : MonoBehaviour
    {
        [Header("Spawn Point")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnCheckRadius = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightStateController stateController;
        private PlayerController.PlayerMovement playerMovement;
        
        // Validation state
        private bool hasCheckedSpawnAtNightfall = false;
        private bool isPlayerAtSpawn = false;
        
        // Events
        public System.Action<bool> OnPlayerAtSpawnChanged;
        public System.Action OnPlayerAtSpawnWhenNightFalls;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[DayNightValidationController] Initialized");
        }
        
        private void OnEnable()
        {
            if (stateController != null)
            {
                stateController.OnStateChanged += OnStateChanged;
                stateController.OnNightStart += OnNightStart;
            }
        }
        
        private void OnDisable()
        {
            if (stateController != null)
            {
                stateController.OnStateChanged -= OnStateChanged;
                stateController.OnNightStart -= OnNightStart;
            }
        }
        
        private void Update()
        {
            CheckPlayerAtSpawn();
        }
        
        private void CheckPlayerAtSpawn()
        {
            if (spawnPoint == null || playerMovement == null) return;
            
            bool wasAtSpawn = isPlayerAtSpawn;
            float distanceToSpawn = Vector3.Distance(playerMovement.transform.position, spawnPoint.position);
            isPlayerAtSpawn = distanceToSpawn <= spawnCheckRadius;
            
            if (wasAtSpawn != isPlayerAtSpawn)
            {
                OnPlayerAtSpawnChanged?.Invoke(isPlayerAtSpawn);
                
                if (showDebugLogs)
                    Debug.Log($"[DayNightValidationController] Player at spawn: {isPlayerAtSpawn} (Distance: {distanceToSpawn:F1}m)");
            }
        }
        
        private void OnStateChanged(DayNightSystem.Core.DayNightState newState)
        {
            // Check if player is at spawn when night falls
            if (newState == DayNightSystem.Core.DayNightState.Night && !hasCheckedSpawnAtNightfall)
            {
                hasCheckedSpawnAtNightfall = true;
                
                if (isPlayerAtSpawn)
                {
                    OnPlayerAtSpawnWhenNightFalls?.Invoke();
                    
                    if (showDebugLogs)
                        Debug.Log("[DayNightValidationController] Player is at spawn when night falls");
                }
                else
                {
                    if (showDebugLogs)
                        Debug.Log("[DayNightValidationController] Player is NOT at spawn when night falls");
                }
            }
            
            // Reset spawn check when day starts
            if (newState == DayNightSystem.Core.DayNightState.Day)
            {
                hasCheckedSpawnAtNightfall = false;
                
                if (showDebugLogs)
                    Debug.Log("[DayNightValidationController] Reset spawn check for new day");
            }
        }
        
        private void OnNightStart()
        {
            // Additional night start validation
            if (showDebugLogs)
                Debug.Log("[DayNightValidationController] Night start validation completed");
        }
        
        public bool IsPlayerAtSpawn()
        {
            return isPlayerAtSpawn;
        }
        
        public bool HasCheckedSpawnAtNightfall()
        {
            return hasCheckedSpawnAtNightfall;
        }
        
        public float GetDistanceToSpawn()
        {
            if (spawnPoint == null || playerMovement == null) return float.MaxValue;
            return Vector3.Distance(playerMovement.transform.position, spawnPoint.position);
        }
        
        public Vector3 GetSpawnPosition()
        {
            return spawnPoint != null ? spawnPoint.position : Vector3.zero;
        }
        
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            spawnPoint = newSpawnPoint;
            
            if (showDebugLogs)
                Debug.Log($"[DayNightValidationController] Spawn point set to: {newSpawnPoint?.name ?? "null"}");
        }
        
        public void SetSpawnCheckRadius(float radius)
        {
            spawnCheckRadius = Mathf.Max(0.1f, radius);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightValidationController] Spawn check radius set to: {spawnCheckRadius}m");
        }
        
        private void FindReferences()
        {
            stateController = GetComponent<DayNightStateController>();
            playerMovement = FindObjectOfType<PlayerController.PlayerMovement>();
            
            // Auto-find spawn point if not assigned
            if (spawnPoint == null)
            {
                SpawnPoint spawnPointComponent = FindObjectOfType<SpawnPoint>();
                if (spawnPointComponent != null)
                {
                    spawnPoint = spawnPointComponent.transform;
                }
            }
        }
        
        private void ValidateReferences()
        {
            if (spawnPoint == null)
            {
                Debug.LogError("[DayNightValidationController] SpawnPoint reference is missing!");
            }
            
            if (stateController == null)
            {
                Debug.LogError("[DayNightValidationController] DayNightStateController reference is missing!");
            }
            
            if (playerMovement == null)
            {
                Debug.LogWarning("[DayNightValidationController] PlayerMovement reference is missing!");
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (spawnPoint != null)
            {
                Gizmos.color = isPlayerAtSpawn ? Color.green : Color.red;
                Gizmos.DrawWireSphere(spawnPoint.position, spawnCheckRadius);
            }
        }
        
        [ContextMenu("Test Spawn Check")]
        public void TestSpawnCheck()
        {
            CheckPlayerAtSpawn();
            Debug.Log($"[DayNightValidationController] Test - Player at spawn: {isPlayerAtSpawn}, Distance: {GetDistanceToSpawn():F1}m");
        }
        
        [ContextMenu("Show Spawn Info")]
        public void ShowSpawnInfo()
        {
            Debug.Log($"[DayNightValidationController] Spawn Info - Position: {GetSpawnPosition()}, Radius: {spawnCheckRadius}m, Player at spawn: {isPlayerAtSpawn}");
        }
        
        [ContextMenu("Force Spawn Check")]
        public void ForceSpawnCheck()
        {
            hasCheckedSpawnAtNightfall = false;
            CheckPlayerAtSpawn();
            
            if (showDebugLogs)
                Debug.Log("[DayNightValidationController] Forced spawn check");
        }
    }
}
