using UnityEngine;
using DayNightSystem.Core;

namespace DayNightSystem
{
    public enum GameState
    {
        Playing,
        Paused,
        TransitioningDayNight,
        NightBlocked, // New: Night state where player is completely blocked
        Menu,
        Loading,
        GameOver
    }
    
    public class GameStateManager : MonoBehaviour
    {
        [Header("State Management")]
        [SerializeField] private GameState currentState = GameState.Playing;
        
        [Header("References")]
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private DayNightSystem.Core.DayNightManager dayNightManager;
        private PlayerController.PlayerMovement playerMovement;
        private PlayerController.PlayerInteraction playerInteraction;
        private PlayerController.MouseLook mouseLook;
        private MessageSystem messageSystem; // New: Reference to MessageSystem
        [SerializeField] private GameStateController.PlayerInputGate playerInputGate; // Delegates input toggling
        [SerializeField] private GameStateController.GameStateMessages gameStateMessages; // Delegates UI messages
        
        // Public properties
        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsTransitioning => currentState == GameState.TransitioningDayNight;
        public bool IsNightBlocked => currentState == GameState.NightBlocked;
        
        // Events
        public System.Action<GameState> OnGameStateChanged;
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
        }
        
        private void OnEnable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart += OnDayNightTransition;
                dayNightManager.OnNightStart += OnDayNightTransition;
                dayNightManager.OnStateChanged += OnDayNightStateChanged;
            }
        }
        
        private void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart -= OnDayNightTransition;
                dayNightManager.OnNightStart -= OnDayNightTransition;
                dayNightManager.OnStateChanged -= OnDayNightStateChanged;
            }
        }
        
        private void OnDayNightStateChanged(DayNightSystem.Core.DayNightState newState)
        {
            // Handle state changes from DayNightManager
            if (newState == DayNightSystem.Core.DayNightState.Night)
            {
                SetNightBlockedState();
                
                // Notify that night is blocked
                if (dayNightManager != null)
                {
                    dayNightManager.OnNightBlocked?.Invoke();
                }
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Night state detected - blocking player completely");
            }
            else if (newState == DayNightSystem.Core.DayNightState.Day)
            {
                SetGameState(GameState.Playing);
                // Teleport player to spawn at day start to guarantee wake-up location
                TeleportPlayerToSpawn();
                EnablePlayerInput();
                
            // Show day start message
            if (gameStateMessages != null)
            {
                gameStateMessages.ShowDayStartMessage(0.5f);
            }
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Day state detected - resuming normal gameplay");
            }
        }

        private void TeleportPlayerToSpawn()
        {
            if (DayNightSystem.SpawnPoint.Current != null)
            {
                DayNightSystem.SpawnPoint.Current.TeleportPlayerHere();
            }
            else if (showDebugLogs)
            {
                Debug.LogWarning("[GameStateManager] Cannot teleport to spawn - no SpawnPoint.Current found");
            }
        }
        
        // Deprecated local method kept for backward compatibility (no references should call it)
        private System.Collections.IEnumerator ShowDayStartMessage() { yield break; }
        
        private void FindReferences()
        {
            dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            playerMovement = FindObjectOfType<PlayerController.PlayerMovement>();
            playerInteraction = FindObjectOfType<PlayerController.PlayerInteraction>();
            mouseLook = FindObjectOfType<PlayerController.MouseLook>();
            messageSystem = FindObjectOfType<MessageSystem>(); // Initialize MessageSystem
            
            // Use serialized field if assigned, otherwise find in scene
            if (feedbackMessages == null)
            {
                feedbackMessages = FindObjectOfType<FeedbackMessagesSO>();
            }

            // Optional: auto-wire helpers if present in the same GameObject
            if (playerInputGate == null) { playerInputGate = GetComponent<GameStateController.PlayerInputGate>(); }
            if (gameStateMessages == null) { gameStateMessages = GetComponent<GameStateController.GameStateMessages>(); }
        }
        
        private void ValidateReferences()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[GameStateManager] No DayNightManager found in scene!");
            }
            
            if (playerMovement == null) { Debug.LogWarning("[GameStateManager] No PlayerMovement found in scene!"); }
            if (playerInteraction == null) { Debug.LogWarning("[GameStateManager] No PlayerInteraction found in scene!"); }
            if (mouseLook == null) { Debug.LogWarning("[GameStateManager] No MouseLook found in scene!"); }
            
            if (messageSystem == null)
            {
                Debug.LogWarning("[GameStateManager] No MessageSystem found in scene!");
            }
            
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[GameStateManager] No FeedbackMessagesSO found in scene!");
            }

            if (playerInputGate == null) { Debug.LogWarning("[GameStateManager] PlayerInputGate not assigned (optional)"); }
            if (gameStateMessages == null) { Debug.LogWarning("[GameStateManager] GameStateMessages not assigned (optional)"); }
        }
        
        private void OnDayNightTransition()
        {
            if (currentState == GameState.Playing)
            {
                SetNightBlockedState();
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Night detected - blocking player completely");
            }
            else if (currentState == GameState.NightBlocked)
            {
                SetGameState(GameState.Playing);
                EnablePlayerInput();
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Day detected - resuming normal gameplay");
            }
        }
        
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                if (playerInputGate != null) { playerInputGate.DisableInput(); } else { DisablePlayerInput(); }
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Game paused");
            }
        }
        
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                if (playerInputGate != null) { playerInputGate.EnableInput(); } else { EnablePlayerInput(); }
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Game resumed");
            }
        }
        
        public void SetTransitioningState()
        {
            SetGameState(GameState.TransitioningDayNight);
            if (playerInputGate != null) { playerInputGate.DisableInput(); } else { DisablePlayerInput(); }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set transitioning state - player input disabled");
        }
        
        public void EndTransition()
        {
            if (currentState == GameState.TransitioningDayNight)
            {
                SetGameState(GameState.Playing);
                if (playerInputGate != null) { playerInputGate.EnableInput(); } else { EnablePlayerInput(); }
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Transition ended - player input re-enabled");
            }
        }
        
        public void SetNightBlockedState()
        {
            SetGameState(GameState.NightBlocked);
            if (playerInputGate != null) { playerInputGate.DisableInput(); } else { DisablePlayerInput(); }
            
            // Show night message
            StartCoroutine(ShowNightMessageWithDelay());
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set night blocked state - player completely blocked");
        }
        
        private System.Collections.IEnumerator ShowNightMessageWithDelay()
        {
            // Wait a moment for the transition to complete
            yield return new WaitForSeconds(0.5f);
            
            if (messageSystem != null)
            {
                string nightMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("night_falling") : 
                    "Night is falling...";
                messageSystem.ShowMessage(nightMessage, 3f);
            }
        }
        
        public void SetMenuState()
        {
            SetGameState(GameState.Menu);
            if (playerInputGate != null) { playerInputGate.DisableInput(); } else { DisablePlayerInput(); }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set menu state");
        }
        
        public void SetLoadingState()
        {
            SetGameState(GameState.Loading);
            if (playerInputGate != null) { playerInputGate.DisableInput(); } else { DisablePlayerInput(); }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set loading state");
        }
        
        public void SetGameOverState()
        {
            SetGameState(GameState.GameOver);
            if (playerInputGate != null) { playerInputGate.DisableInput(); } else { DisablePlayerInput(); }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set game over state");
        }
        
        private void SetGameState(GameState newState)
        {
            if (currentState == newState) return;
            
            GameState previousState = currentState;
            currentState = newState;
            
            OnGameStateChanged?.Invoke(newState);
            
            if (showDebugLogs)
                Debug.Log($"[GameStateManager] State changed from {previousState} to {newState}");
        }
        
        private void DisablePlayerInput()
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }
            
            if (playerInteraction != null)
            {
                playerInteraction.enabled = false;
            }
            
            if (mouseLook != null)
            {
                mouseLook.enabled = false;
            }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Player input disabled");
        }
        
        private void EnablePlayerInput()
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
            
            if (playerInteraction != null)
            {
                playerInteraction.enabled = true;
            }
            
            if (mouseLook != null)
            {
                mouseLook.enabled = true;
            }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Player input enabled");
        }
        
        public bool CanPlayerAct()
        {
            return currentState == GameState.Playing;
        }
        
        public string GetCurrentStateString()
        {
            return currentState.ToString();
        }
        
        public void SetFeedbackMessages(FeedbackMessagesSO messages)
        {
            feedbackMessages = messages;
            
            if (showDebugLogs)
                Debug.Log($"[GameStateManager] FeedbackMessagesSO set to: {messages?.name ?? "null"}");
        }
        
        [ContextMenu("Force Playing State")]
        public void ForcePlayingState()
        {
            SetGameState(GameState.Playing);
            EnablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Forced to playing state");
        }
        
        [ContextMenu("Force Paused State")]
        public void ForcePausedState()
        {
            SetGameState(GameState.Paused);
            if (playerInputGate != null) { playerInputGate.DisableInput(); } else { DisablePlayerInput(); }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Forced to paused state");
        }
        
        [ContextMenu("Force Night Blocked State")]
        public void ForceNightBlockedState()
        {
            SetNightBlockedState();
        }
        
        [ContextMenu("Test Night Message")]
        public void TestNightMessage()
        {
            if (gameStateMessages != null) { gameStateMessages.ShowNightMessage(0f); return; }
            if (messageSystem == null) { Debug.LogWarning("[GameStateManager] Cannot test night message - MessageSystem not found"); return; }
            string nightMessage = feedbackMessages != null ? feedbackMessages.GetMessage("night_falling") : string.Empty;
            if (!string.IsNullOrEmpty(nightMessage))
            {
                messageSystem.ShowMessage(nightMessage, 3f);
            }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Test night message shown");
        }
        
        [ContextMenu("Test Day Message")]
        public void TestDayMessage()
        {
            if (gameStateMessages != null) { gameStateMessages.ShowDayStartMessage(0f); return; }
            if (messageSystem == null) { Debug.LogWarning("[GameStateManager] Cannot test day message - MessageSystem not found"); return; }
            string dayMessage = feedbackMessages != null ? feedbackMessages.GetMessage("new_day_started") : string.Empty;
            if (!string.IsNullOrEmpty(dayMessage))
            {
                messageSystem.ShowMessage(dayMessage, 2f);
            }
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Test day message shown");
        }
        
        [ContextMenu("Show Current State")]
        public void ShowCurrentState()
        {
            string state = $"Current State: {currentState}\n" +
                          $"Can Player Act: {CanPlayerAct()}\n" +
                          $"Is Playing: {IsPlaying}\n" +
                          $"Is Paused: {IsPaused}\n" +
                          $"Is Transitioning: {IsTransitioning}";
            
            Debug.Log($"[GameStateManager] {state}");
        }
    }
}

// ScriptRole: Manages global game states and player input control
// RelatedScripts: DayNightManager, PlayerMovement, PlayerInteraction, MouseLook, MessageSystem
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: DayNightManager events
// SendsTo: PlayerMovement, PlayerInteraction, MouseLook (input control), MessageSystem (state messages)
