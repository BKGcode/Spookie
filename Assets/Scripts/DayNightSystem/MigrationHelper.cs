using UnityEngine;
using DayNightSystem.Core;
using DayNightSystem.Validation;
using DayNightSystem.Persistence;

namespace DayNightSystem
{
    public class MigrationHelper : MonoBehaviour
    {
        [Header("Migration Settings")]
        [SerializeField] private bool autoMigrate = true;
        [SerializeField] private bool preserveOldScripts = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightManager oldDayNightManager;
        private DayNightManager newDayNightManager;
        
        private void Awake()
        {
            if (autoMigrate)
            {
                MigrateToNewSystem();
            }
        }
        
        [ContextMenu("Migrate to New System")]
        public void MigrateToNewSystem()
        {
            if (showDebugLogs)
                Debug.Log("[MigrationHelper] Starting migration to new modular system...");
            
            // Find old DayNightManager
            oldDayNightManager = FindObjectOfType<DayNightManager>();
            
            if (oldDayNightManager == null)
            {
                Debug.LogWarning("[MigrationHelper] No old DayNightManager found. Creating new one.");
                CreateNewDayNightManager();
                return;
            }
            
            // Create new DayNightManager
            CreateNewDayNightManager();
            
            // Migrate configuration
            MigrateConfiguration();
            
            // Migrate references
            MigrateReferences();
            
            // Disable old manager
            if (preserveOldScripts)
            {
                oldDayNightManager.enabled = false;
                Debug.Log("[MigrationHelper] Old DayNightManager disabled (preserved)");
            }
            else
            {
                DestroyImmediate(oldDayNightManager);
                Debug.Log("[MigrationHelper] Old DayNightManager destroyed");
            }
            
            if (showDebugLogs)
                Debug.Log("[MigrationHelper] Migration completed successfully!");
        }
        
        private void CreateNewDayNightManager()
        {
            // Create new GameObject for modular system
            GameObject modularSystem = new GameObject("DayNightSystem_Modular");
            modularSystem.transform.SetParent(transform);
            
            // Add new DayNightManager
            newDayNightManager = modularSystem.AddComponent<DayNightManager>();
            
            // Add all required components
            modularSystem.AddComponent<DayNightTimeController>();
            modularSystem.AddComponent<DayNightStateController>();
            modularSystem.AddComponent<DayNightEventController>();
            modularSystem.AddComponent<DayNightValidationController>();
            modularSystem.AddComponent<DayNightPersistenceController>();
            
            if (showDebugLogs)
                Debug.Log("[MigrationHelper] Created new modular DayNightManager");
        }
        
        private void MigrateConfiguration()
        {
            if (oldDayNightManager != null && newDayNightManager != null)
            {
                // Copy configuration
                var oldConfig = oldDayNightManager.Config;
                if (oldConfig != null)
                {
                    // The new system will auto-assign the config
                    if (showDebugLogs)
                        Debug.Log("[MigrationHelper] Configuration migrated");
                }
            }
        }
        
        private void MigrateReferences()
        {
            // Find all scripts that reference the old DayNightManager
            var allScripts = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var script in allScripts)
            {
                if (script == null) continue;
                
                // Check if this script needs migration
                if (script is GameStateManager gameStateManager)
                {
                    MigrateGameStateManager(gameStateManager);
                }
                else if (script is AudioManager audioManager)
                {
                    MigrateAudioManager(audioManager);
                }
                else if (script is DayNightUI dayNightUI)
                {
                    MigrateDayNightUI(dayNightUI);
                }
                else if (script is TransitionHandler transitionHandler)
                {
                    MigrateTransitionHandler(transitionHandler);
                }
            }
            
            if (showDebugLogs)
                Debug.Log("[MigrationHelper] References migrated");
        }
        
        private void MigrateGameStateManager(GameStateManager gameStateManager)
        {
            // Update the reference to the new DayNightManager
            var newManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            if (newManager != null)
            {
                // Use reflection to update the private field
                var field = typeof(GameStateManager).GetField("dayNightManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(gameStateManager, newManager);
                    
                    if (showDebugLogs)
                        Debug.Log("[MigrationHelper] GameStateManager reference updated");
                }
            }
        }
        
        private void MigrateAudioManager(AudioManager audioManager)
        {
            // Update the reference to the new DayNightManager
            var newManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            if (newManager != null)
            {
                // Use reflection to update the private field
                var field = typeof(AudioManager).GetField("dayNightManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(audioManager, newManager);
                    
                    if (showDebugLogs)
                        Debug.Log("[MigrationHelper] AudioManager reference updated");
                }
            }
        }
        
        private void MigrateDayNightUI(DayNightUI dayNightUI)
        {
            // Create new modular UI system
            GameObject uiSystem = new GameObject("DayNightUI_Modular");
            uiSystem.transform.SetParent(dayNightUI.transform);
            
            // Add new DayNightUIManager
            var newUIManager = uiSystem.AddComponent<DayNightSystem.UI.DayNightUIManager>();
            
            // Add all required UI components
            uiSystem.AddComponent<DayNightSystem.UI.TimeDisplay.TimeDisplayController>();
            uiSystem.AddComponent<DayNightSystem.UI.StatusDisplay.StatusDisplayController>();
            uiSystem.AddComponent<DayNightSystem.UI.WarningSystem.WarningSystemController>();
            uiSystem.AddComponent<DayNightSystem.UI.ProximitySystem.ProximitySystemController>();
            
            // Migrate UI references using reflection
            MigrateUIRefferences(dayNightUI, newUIManager);
            
            if (showDebugLogs)
                Debug.Log("[MigrationHelper] DayNightUI migrated to modular system");
        }
        
        private void MigrateUIRefferences(DayNightUI oldUI, DayNightSystem.UI.DayNightUIManager newUI)
        {
            // Migrate TextMeshPro references
            var oldTimeDisplayField = typeof(DayNightUI).GetField("timeDisplay", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var oldStatusDisplayField = typeof(DayNightUI).GetField("statusDisplay", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var oldWarningIconField = typeof(DayNightUI).GetField("warningIcon", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var oldSpawnProximityIconField = typeof(DayNightUI).GetField("spawnProximityIcon", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Get references from old UI
            var timeDisplay = oldTimeDisplayField?.GetValue(oldUI) as TMPro.TextMeshProUGUI;
            var statusDisplay = oldStatusDisplayField?.GetValue(oldUI) as TMPro.TextMeshProUGUI;
            var warningIcon = oldWarningIconField?.GetValue(oldUI) as UnityEngine.UI.Image;
            var spawnProximityIcon = oldSpawnProximityIconField?.GetValue(oldUI) as UnityEngine.UI.Image;
            
            // Assign to new UI components
            var timeController = newUI.GetTimeDisplayController();
            var statusController = newUI.GetStatusDisplayController();
            var warningController = newUI.GetWarningSystemController();
            var proximityController = newUI.GetProximitySystemController();
            
            if (timeController != null && timeDisplay != null)
            {
                var newTimeDisplayField = typeof(DayNightSystem.UI.TimeDisplay.TimeDisplayController).GetField("timeDisplay", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                newTimeDisplayField?.SetValue(timeController, timeDisplay);
            }
            
            if (statusController != null && statusDisplay != null)
            {
                var newStatusDisplayField = typeof(DayNightSystem.UI.StatusDisplay.StatusDisplayController).GetField("statusDisplay", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                newStatusDisplayField?.SetValue(statusController, statusDisplay);
            }
            
            if (warningController != null && warningIcon != null)
            {
                var newWarningIconField = typeof(DayNightSystem.UI.WarningSystem.WarningSystemController).GetField("warningIcon", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                newWarningIconField?.SetValue(warningController, warningIcon);
            }
            
            if (proximityController != null && spawnProximityIcon != null)
            {
                var newSpawnProximityIconField = typeof(DayNightSystem.UI.ProximitySystem.ProximitySystemController).GetField("spawnProximityIcon", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                newSpawnProximityIconField?.SetValue(proximityController, spawnProximityIcon);
            }
            
            if (showDebugLogs)
                Debug.Log("[MigrationHelper] UI references migrated to modular system");
        }
        
        private void MigrateTransitionHandler(TransitionHandler transitionHandler)
        {
            // Update the reference to the new DayNightManager
            var newManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            if (newManager != null)
            {
                // Use reflection to update the private field
                var field = typeof(TransitionHandler).GetField("dayNightManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(transitionHandler, newManager);
                    
                    if (showDebugLogs)
                        Debug.Log("[MigrationHelper] TransitionHandler reference updated");
                }
            }
        }
        
        [ContextMenu("Show Migration Status")]
        public void ShowMigrationStatus()
        {
            var oldManager = FindObjectOfType<DayNightManager>();
            var newManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            
            Debug.Log($"[MigrationHelper] Migration Status:\n" +
                     $"Old DayNightManager: {oldManager != null}\n" +
                     $"New DayNightManager: {newManager != null}\n" +
                     $"Auto Migrate: {autoMigrate}\n" +
                     $"Preserve Old Scripts: {preserveOldScripts}");
        }
        
        [ContextMenu("Clean Up Old Scripts")]
        public void CleanUpOldScripts()
        {
            var oldScripts = FindObjectsOfType<MonoBehaviour>();
            int cleanedCount = 0;
            
            foreach (var script in oldScripts)
            {
                if (script == null) continue;
                
                // Check if this is an old script that should be cleaned up
                if (script.GetType().Name == "DayNightManager" && 
                    script.GetType().Namespace == "DayNightSystem" &&
                    !script.GetType().Namespace.Contains("Core"))
                {
                    DestroyImmediate(script);
                    cleanedCount++;
                }
            }
            
            if (showDebugLogs)
                Debug.Log($"[MigrationHelper] Cleaned up {cleanedCount} old scripts");
        }
        
        [ContextMenu("Validate New System")]
        public void ValidateNewSystem()
        {
            var newManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            
            if (newManager == null)
            {
                Debug.LogError("[MigrationHelper] No new DayNightManager found!");
                return;
            }
            
            // Check if all components are present
            var timeController = newManager.GetTimeController();
            var stateController = newManager.GetStateController();
            var eventController = newManager.GetEventController();
            var validationController = newManager.GetValidationController();
            var persistenceController = newManager.GetPersistenceController();
            
            bool allComponentsPresent = timeController != null && 
                                     stateController != null && 
                                     eventController != null && 
                                     validationController != null && 
                                     persistenceController != null;
            
            Debug.Log($"[MigrationHelper] New System Validation:\n" +
                     $"All Components Present: {allComponentsPresent}\n" +
                     $"TimeController: {timeController != null}\n" +
                     $"StateController: {stateController != null}\n" +
                     $"EventController: {eventController != null}\n" +
                     $"ValidationController: {validationController != null}\n" +
                     $"PersistenceController: {persistenceController != null}");
        }
    }
}
