using UnityEngine;
using DayNightSystem.Core;

namespace DayNightSystem
{
    public class DebugCommands : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private float timeAddAmount = 5f; // minutes
        
        // Private fields
        private DayNightSystem.Core.DayNightManager dayNightManager;
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
            dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
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
            
            if (playerPenalty.CurrentPenaltyType != PenaltyType.None)
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
                Debug.Log($"[DebugCommands] Added {timeAddAmount} minutes");
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
                Debug.Log($"[DebugCommands] Added {minutes} minutes to day/night cycle");
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
                Debug.Log("[DebugCommands] Time paused");
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
                Debug.Log("[DebugCommands] Time resumed");
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
                Debug.Log("[DebugCommands] Visuals force updated");
        }
        
        [ContextMenu("Show Current State")]
        public void ShowCurrentState()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[DebugCommands] Cannot show state - DayNightManager not found!");
                return;
            }
            
            string state = $"Day/Night State:\n" +
                          $"Current State: {dayNightManager.CurrentState}\n" +
                          $"Is Day: {dayNightManager.IsDay}\n" +
                          $"Is Transitioning: {dayNightManager.IsTransitioning}\n" +
                          $"Is Paused: {dayNightManager.IsPaused}\n" +
                          $"Time Normalized: {dayNightManager.CurrentTimeNormalized:F2}\n" +
                          $"Player Slept Correctly: {dayNightManager.PlayerSleptCorrectly}";
            
            Debug.Log($"[DebugCommands] {state}");
        }
        
        [ContextMenu("Clear Save Data")]
        public void ClearSaveData()
        {
            SaveUtility.ClearDayNightSaveData();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Save data cleared");
        }
        
        [ContextMenu("Show Save Data")]
        public void ShowSaveData()
        {
            if (SaveUtility.HasSaveData())
            {
                var saveData = SaveUtility.LoadDayNightState();
                Debug.Log($"[DebugCommands] Save data found:\n" +
                         $"Time: {saveData.currentTime:F1}s\n" +
                         $"Is Day: {saveData.isDay}\n" +
                         $"Has Penalty: {saveData.hasPenalty}\n" +
                         $"Slept Correctly: {saveData.playerSleptCorrectly}");
            }
            else
            {
                Debug.Log("[DebugCommands] No save data found");
            }
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
            
            messageSystem.ShowMessage("This is a test message from DebugCommands", 3f);
            
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
            
            string status = $"Message System Status:\n" +
                          $"Is Message Active: {messageSystem.IsMessageActive}\n" +
                          $"Is Displaying Message: {messageSystem.IsDisplayingMessage()}\n" +
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
            
            messageSystem.StopCurrentMessage();
            
            if (showDebugLogs)
                Debug.Log("[DebugCommands] Message force closed");
        }
    }
}

// ScriptRole: Provides debug commands for testing day/night system functionality
// RelatedScripts: DayNightManager, PlayerPenalty, DayNightVisuals, GameStateManager, MessageSystem
// UsesSO: None
// ReceivesFrom: None
// SendsTo: DayNightManager, PlayerPenalty, DayNightVisuals, GameStateManager, MessageSystem (debug commands)
