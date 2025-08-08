using UnityEngine;
using UnityEngine.UI;
using DayNightSystem.Validation;

namespace DayNightSystem.UI.ProximitySystem
{
    public class ProximitySystemController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image spawnProximityIcon;
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        
        [Header("Settings")]
        [SerializeField] private bool showSpawnProximity = true;
        [SerializeField] private float proximityUpdateRate = 0.5f;
        [SerializeField] private float warningDistance = 5f;
        [SerializeField] private float dangerDistance = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightValidationController validationController;
        private Coroutine proximityUpdateCoroutine;
        
        // State
        private bool isProximityActive = false;
        private bool isInWarningZone = false;
        private bool isInDangerZone = false;
        private float currentDistance = float.MaxValue;
        
        // Events
        public System.Action<bool> OnProximityStateChanged;
        public System.Action<bool> OnWarningZoneChanged;
        public System.Action<bool> OnDangerZoneChanged;
        public System.Action<float> OnDistanceChanged;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[ProximitySystemController] Initialized");
        }
        
        private void OnEnable()
        {
            if (validationController != null)
            {
                validationController.OnPlayerAtSpawnChanged += OnPlayerAtSpawnChanged;
                validationController.OnPlayerAtSpawnWhenNightFalls += OnPlayerAtSpawnWhenNightFalls;
            }
            
            StartProximityUpdate();
        }
        
        private void OnDisable()
        {
            if (validationController != null)
            {
                validationController.OnPlayerAtSpawnChanged -= OnPlayerAtSpawnChanged;
                validationController.OnPlayerAtSpawnWhenNightFalls -= OnPlayerAtSpawnWhenNightFalls;
            }
            
            StopProximityUpdate();
        }
        
        private void OnPlayerAtSpawnChanged(bool isAtSpawn)
        {
            UpdateProximityState(isAtSpawn);
        }
        
        private void OnPlayerAtSpawnWhenNightFalls()
        {
            ShowProximityMessage(GetMessage("proximity_safe_night"));
        }
        
        private void StartProximityUpdate()
        {
            if (proximityUpdateCoroutine != null)
            {
                StopCoroutine(proximityUpdateCoroutine);
            }
            
            proximityUpdateCoroutine = StartCoroutine(ProximityUpdateCoroutine());
        }
        
        private void StopProximityUpdate()
        {
            if (proximityUpdateCoroutine != null)
            {
                StopCoroutine(proximityUpdateCoroutine);
                proximityUpdateCoroutine = null;
            }
        }
        
        private System.Collections.IEnumerator ProximityUpdateCoroutine()
        {
            while (true)
            {
                UpdateDistance();
                yield return new WaitForSeconds(proximityUpdateRate);
            }
        }
        
        private void UpdateDistance()
        {
            if (validationController == null) return;
            
            float newDistance = validationController.GetDistanceToSpawn();
            if (Mathf.Abs(newDistance - currentDistance) > 0.1f)
            {
                currentDistance = newDistance;
                OnDistanceChanged?.Invoke(currentDistance);
                
                UpdateProximityZones();
                
                if (showDebugLogs)
                    Debug.Log($"[ProximitySystemController] Distance updated: {currentDistance:F1}m");
            }
        }
        
        private void UpdateProximityZones()
        {
            bool wasInWarningZone = isInWarningZone;
            bool wasInDangerZone = isInDangerZone;
            
            isInWarningZone = currentDistance <= warningDistance && currentDistance > dangerDistance;
            isInDangerZone = currentDistance <= dangerDistance;
            
            if (wasInWarningZone != isInWarningZone)
            {
                OnWarningZoneChanged?.Invoke(isInWarningZone);
                if (showDebugLogs)
                    Debug.Log($"[ProximitySystemController] Warning zone: {isInWarningZone}");
            }
            
            if (wasInDangerZone != isInDangerZone)
            {
                OnDangerZoneChanged?.Invoke(isInDangerZone);
                if (showDebugLogs)
                    Debug.Log($"[ProximitySystemController] Danger zone: {isInDangerZone}");
            }
        }
        
        private void UpdateProximityState(bool isAtSpawn)
        {
            bool wasActive = isProximityActive;
            isProximityActive = showSpawnProximity && !isAtSpawn;
            
            if (wasActive != isProximityActive)
            {
                OnProximityStateChanged?.Invoke(isProximityActive);
                
                if (isProximityActive)
                {
                    ShowProximityIndicator();
                }
                else
                {
                    HideProximityIndicator();
                }
                
                if (showDebugLogs)
                    Debug.Log($"[ProximitySystemController] Proximity active: {isProximityActive}");
            }
        }
        
        private void ShowProximityIndicator()
        {
            if (spawnProximityIcon != null)
            {
                spawnProximityIcon.gameObject.SetActive(true);
                
                // Set color based on zone
                if (isInDangerZone)
                {
                    spawnProximityIcon.color = Color.red;
                }
                else if (isInWarningZone)
                {
                    spawnProximityIcon.color = Color.yellow;
                }
                else
                {
                    spawnProximityIcon.color = Color.green;
                }
            }
        }
        
        private void HideProximityIndicator()
        {
            if (spawnProximityIcon != null)
            {
                spawnProximityIcon.gameObject.SetActive(false);
            }
        }
        
        private void ShowProximityMessage(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[ProximitySystemController] Proximity Message: {message}");
        }
        
        private string GetMessage(string key, string defaultValue = "")
        {
            if (feedbackMessages != null)
            {
                return feedbackMessages.GetMessage(key);
            }
            
            return defaultValue;
        }
        
        public void ForceProximityCheck()
        {
            if (validationController != null)
            {
                bool isAtSpawn = validationController.IsPlayerAtSpawn();
                UpdateProximityState(isAtSpawn);
                UpdateDistance();
            }
        }
        
        public void SetShowSpawnProximity(bool show)
        {
            showSpawnProximity = show;
            if (!show)
            {
                HideProximityIndicator();
            }
            else
            {
                ForceProximityCheck();
            }
        }
        
        public void SetProximityUpdateRate(float rate)
        {
            proximityUpdateRate = Mathf.Max(0.1f, rate);
            StartProximityUpdate();
        }
        
        public void SetWarningDistance(float distance)
        {
            warningDistance = Mathf.Max(0f, distance);
        }
        
        public void SetDangerDistance(float distance)
        {
            dangerDistance = Mathf.Max(0f, distance);
        }
        
        public bool IsProximityActive()
        {
            return isProximityActive;
        }
        
        public bool IsInWarningZone()
        {
            return isInWarningZone;
        }
        
        public bool IsInDangerZone()
        {
            return isInDangerZone;
        }
        
        public float GetCurrentDistance()
        {
            return currentDistance;
        }
        
        public Vector3 GetSpawnPosition()
        {
            return validationController != null ? validationController.GetSpawnPosition() : Vector3.zero;
        }
        
        private void FindReferences()
        {
            validationController = GetComponent<DayNightValidationController>();
            if (validationController == null)
            {
                validationController = FindObjectOfType<DayNightValidationController>();
            }
        }
        
        private void ValidateReferences()
        {
            if (spawnProximityIcon == null)
            {
                Debug.LogWarning("[ProximitySystemController] SpawnProximityIcon reference is missing!");
            }
            
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[ProximitySystemController] FeedbackMessagesSO reference is missing!");
            }
            
            if (validationController == null)
            {
                Debug.LogError("[ProximitySystemController] DayNightValidationController reference is missing!");
            }
        }
        
        [ContextMenu("Force Proximity Check")]
        public void ForceProximityCheckContext()
        {
            ForceProximityCheck();
        }
        
        [ContextMenu("Show Proximity Info")]
        public void ShowProximityInfo()
        {
            Debug.Log($"[ProximitySystemController] Proximity Info - Active: {IsProximityActive()}, Distance: {GetCurrentDistance():F1}m, Warning Zone: {IsInWarningZone()}, Danger Zone: {IsInDangerZone()}");
        }
        
        [ContextMenu("Toggle Proximity Display")]
        public void ToggleProximityDisplay()
        {
            SetShowSpawnProximity(!showSpawnProximity);
        }
        
        [ContextMenu("Show Spawn Position")]
        public void ShowSpawnPosition()
        {
            Debug.Log($"[ProximitySystemController] Spawn Position: {GetSpawnPosition()}");
        }
    }
}
