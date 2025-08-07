using UnityEngine;

namespace DayNightSystem
{
    public class DebugCommands : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private float timeAddAmount = 5f; // minutes
        
        // Private fields
        private DayNightManager dayNightManager;
        private PlayerPenalty playerPenalty;
        private DayNightVisuals dayNightVisuals;
        private GameStateManager gameStateManager;
        private MessageSystem messageSystem;
        
        private void Awake()
        {
            FindReferences();
        }
        
        private void FindReferences()
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
            playerPenalty = FindObjectOfType<PlayerPenalty>();
            dayNightVisuals = FindObjectOfType<DayNightVisuals>();
            gameStateManager = FindObjectOfType<GameStateManager>();
            messageSystem = FindObjectOfType<MessageSystem>();
            
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] No DayNightManager found in scene!");
            }
        }
        
        [ContextMenu("Skip To Night")]
        public void SkipToNight()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot skip to night - DayNightManager not found!");
                return;
            }
            
            dayNightManager.ForceNight();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Skipped to night");
        }
        
        [ContextMenu("Skip To Day")]
        public void SkipToDay()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot skip to day - DayNightManager not found!");
                return;
            }
            
            dayNightManager.ForceDay();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Skipped to day");
        }
        
        [ContextMenu("Toggle Penalty")]
        public void TogglePenalty()
        {
            if (playerPenalty == null)
            {
                Debug.LogError("[DebugCommands] Cannot toggle penalty - PlayerPenalty not found!");
                return;
            }
            
            if (playerPenalty.HasExhaustionPenalty || playerPenalty.HasFaintedPenalty)
            {
                playerPenalty.RemoveAllPenalties();
                if (showDebugLogs)
                    Debug.Log("[DebugCommands] Removed all penalties");
            }
            else
            {
                playerPenalty.ApplyExhaustionPenalty();
                if (showDebugLogs)
                    Debug.Log("[DebugCommands] Applied exhaustion penalty");
            }
        }
        
        [ContextMenu("Reset Day")]
        public void ResetDay()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot reset day - DayNightManager not found!");
                return;
            }
            
            dayNightManager.ForceDay();
            SaveUtility.ClearDayNightSaveData();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Reset day and cleared save data");
        }
        
        [ContextMenu("Add Time")]
        public void AddTime()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot add time - DayNightManager not found!");
                return;
            }
            
            dayNightManager.AddTime(timeAddAmount);
            
            if (showDebugLogs)
                Debug.Log($"[DebugCommands] Added {timeAddAmount} minutes to current time");
        }
        
        [ContextMenu("Add 1 Minute")]
        public void AddOneMinute()
        {
            AddTime(1f);
        }
        
        [ContextMenu("Add 5 Minutes")]
        public void AddFiveMinutes()
        {
            AddTime(5f);
        }
        
        [ContextMenu("Add 10 Minutes")]
        public void AddTenMinutes()
        {
            AddTime(10f);
        }
        
        public void AddTime(float minutes)
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot add time - DayNightManager not found!");
                return;
            }
            
            dayNightManager.AddTime(minutes);
            
            if (showDebugLogs)
                Debug.Log($"[DebugCommands] Added {minutes} minutes to current time");
        }
        
        [ContextMenu("Pause Time")]
        public void PauseTime()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot pause time - DayNightManager not found!");
                return;
            }
            
            dayNightManager.PauseTime(true);
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Paused time");
        }
        
        [ContextMenu("Resume Time")]
        public void ResumeTime()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot resume time - DayNightManager not found!");
                return;
            }
            
            dayNightManager.PauseTime(false);
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Resumed time");
        }
        
        [ContextMenu("Force Update Visuals")]
        public void ForceUpdateVisuals()
        {
            if (dayNightVisuals == null)
            {
                Debug.LogError("[DebugCommands] Cannot update visuals - DayNightVisuals not found!");
                return;
            }
            
            dayNightVisuals.ForceUpdateVisuals();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Forced visual update");
        }
        
        [ContextMenu("Show Current State")]
        public void ShowCurrentState()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot show state - DayNightManager not found!");
                return;
            }
            
            string state = $"Current State:\n" +
                         $"Time: {dayNightManager.CurrentTimeNormalized:P1}\n" +
                         $"Is Day: {dayNightManager.IsDay}\n" +
                         $"Is Transitioning: {dayNightManager.IsTransitioning}\n" +
                         $"Is Paused: {dayNightManager.IsPaused}";
            
            if (playerPenalty != null)
            {
                state += $"\nHas Exhaustion Penalty: {playerPenalty.HasExhaustionPenalty}";
                state += $"\nHas Fainted Penalty: {playerPenalty.HasFaintedPenalty}";
                state += $"\nCurrent Penalty Type: {playerPenalty.CurrentPenaltyType}";
            }
            
            Debug.Log($"[DebugCommands] {state}");
        }
        
        [ContextMenu("Clear Save Data")]
        public void ClearSaveData()
        {
            SaveUtility.ClearDayNightSaveData();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Cleared all save data");
        }
        
        [ContextMenu("Show Save Data")]
        public void ShowSaveData()
        {
            bool hasData = SaveUtility.HasSaveData();
            var saveData = SaveUtility.LoadDayNightState();
            
            string info = $"Save Data:\n" +
                         $"Has Data: {hasData}\n" +
                         $"Time: {saveData.currentTime:F1}s\n" +
                         $"Is Day: {saveData.isDay}\n" +
                         $"Has Penalty: {saveData.hasPenalty}";
            
            Debug.Log($"[DebugCommands] {info}");
        }
        
        [ContextMenu("Show Game State")]
        public void ShowGameState()
        {
            if (gameStateManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot show game state - GameStateManager not found!");
                return;
            }
            
            string state = $"Game State:\n" +
                         $"Current State: {gameStateManager.CurrentState}\n" +
                         $"Can Player Act: {gameStateManager.CanPlayerAct()}\n" +
                         $"Is Playing: {gameStateManager.IsPlaying}\n" +
                         $"Is Paused: {gameStateManager.IsPaused}\n" +
                         $"Is Transitioning: {gameStateManager.IsTransitioning}";
            
            Debug.Log($"[DebugCommands] {state}");
        }
        
        [ContextMenu("Pause Game")]
        public void PauseGame()
        {
            if (gameStateManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot pause game - GameStateManager not found!");
                return;
            }
            
            gameStateManager.PauseGame();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Game paused");
        }
        
        [ContextMenu("Resume Game")]
        public void ResumeGame()
        {
            if (gameStateManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot resume game - GameStateManager not found!");
                return;
            }
            
            gameStateManager.ResumeGame();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Game resumed");
        }
        
        [ContextMenu("Show Test Message")]
        public void ShowTestMessage()
        {
            if (messageSystem == null)
            {
                Debug.LogError("[DebugCommands] Cannot show test message - MessageSystem not found!");
                return;
            }
            
            messageSystem.ShowMessage("This is a test message from DebugCommands!");
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Test message shown");
        }
        
        [ContextMenu("Show Message Status")]
        public void ShowMessageStatus()
        {
            if (messageSystem == null)
            {
                Debug.LogError("[DebugCommands] Cannot show message status - MessageSystem not found!");
                return;
            }
            
            string status = $"Message System:\n" +
                         $"Is Active: {messageSystem.IsMessageActive}\n" +
                         $"Is Displaying: {messageSystem.IsDisplayingMessage()}\n" +
                         $"Current Message: {messageSystem.GetCurrentMessage()}";
            
            Debug.Log($"[DebugCommands] {status}");
        }
        
        [ContextMenu("Force Close Message")]
        public void ForceCloseMessage()
        {
            if (messageSystem == null)
            {
                Debug.LogError("[DebugCommands] Cannot force close message - MessageSystem not found!");
                return;
            }
            
            messageSystem.OnDayNightTransitionRequested();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Message force closed");
        }
    }
}

// ScriptRole: Provides debug commands for testing the day/night system
// RelatedScripts: DayNightManager, PlayerPenalty, DayNightVisuals
// UsesSO: None
// ReceivesFrom: None
// SendsTo: DayNightManager, PlayerPenalty, DayNightVisuals, SaveUtility
