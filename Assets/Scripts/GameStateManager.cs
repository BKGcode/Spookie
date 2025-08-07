using UnityEngine;

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
        private DayNightManager dayNightManager;
        private PlayerController.PlayerMovement playerMovement;
        private PlayerController.PlayerInteraction playerInteraction;
        private PlayerController.MouseLook mouseLook;
        private MessageSystem messageSystem; // New: Reference to MessageSystem
        
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
                dayNightManager.OnStateChanged += OnDayNightStateChanged; // New: Listen to state changes
            }
        }
        
        private void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart -= OnDayNightTransition;
                dayNightManager.OnNightStart -= OnDayNightTransition;
                dayNightManager.OnStateChanged -= OnDayNightStateChanged; // New: Unsubscribe from state changes
            }
        }
        
        private void OnDayNightStateChanged(DayNightState newState)
        {
            // Handle state changes from DayNightManager
            if (newState == DayNightState.Night)
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
            else if (newState == DayNightState.Day)
            {
                SetGameState(GameState.Playing);
                EnablePlayerInput();
                
                // Show day start message
                StartCoroutine(ShowDayStartMessage());
                
                if (showDebugLogs)
                    Debug.Log("[GameStateManager] Day state detected - resuming normal gameplay");
            }
        }
        
        private System.Collections.IEnumerator ShowDayStartMessage()
        {
            // Wait a moment for the transition to complete
            yield return new WaitForSeconds(0.5f);
            
            if (messageSystem != null)
            {
                string dayMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("new_day_started") : 
                    "A new day has begun!";
                messageSystem.ShowMessage(dayMessage, 2f);
            }
        }
        
        private void FindReferences()
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
            playerMovement = FindObjectOfType<PlayerController.PlayerMovement>();
            playerInteraction = FindObjectOfType<PlayerController.PlayerInteraction>();
            mouseLook = FindObjectOfType<PlayerController.MouseLook>();
            messageSystem = FindObjectOfType<MessageSystem>(); // Initialize MessageSystem
            
            // Use serialized field if assigned, otherwise find in scene
            if (feedbackMessages == null)
            {
                feedbackMessages = FindObjectOfType<FeedbackMessagesSO>();
            }
        }
        
        private void ValidateReferences()
        {
            if (dayNightManager == null)
            {
                Debug.LogError("[GameStateManager] No DayNightManager found in scene!");
            }
            
            if (playerMovement == null)
            {
                Debug.LogWarning("[GameStateManager] No PlayerMovement found in scene!");
            }
            
            if (playerInteraction == null)
            {
                Debug.LogWarning("[GameStateManager] No PlayerInteraction found in scene!");
            }
            
            if (mouseLook == null)
            {
                Debug.LogWarning("[GameStateManager] No MouseLook found in scene!");
            }

            if (messageSystem == null)
            {
                Debug.LogWarning("[GameStateManager] No MessageSystem found in scene!");
            }

            if (feedbackMessages == null)
            {
                Debug.LogWarning("[GameStateManager] No FeedbackMessagesSO found in scene!");
            }
        }
        
        private void OnDayNightTransition()
        {
            // Check if it's night time and player should be blocked
            if (dayNightManager != null)
            {
                if (dayNightManager.CurrentState == DayNightState.Night)
                {
                    SetNightBlockedState();
                    
                    if (showDebugLogs)
                        Debug.Log("[GameStateManager] Night detected - blocking player completely");
                }
                else if (dayNightManager.CurrentState == DayNightState.Day)
                {
                    // Resume normal gameplay for day
                    SetGameState(GameState.Playing);
                    EnablePlayerInput();
                    
                    if (showDebugLogs)
                        Debug.Log("[GameStateManager] Day detected - resuming normal gameplay");
                }
            }
        }
        
        public void PauseGame()
        {
            if (currentState == GameState.Paused) return;
            
            SetGameState(GameState.Paused);
            
            // Pause day/night time
            if (dayNightManager != null)
            {
                dayNightManager.PauseTime(true);
            }
            
            // Disable player input
            DisablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Game paused");
        }
        
        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            
            SetGameState(GameState.Playing);
            
            // Resume day/night time
            if (dayNightManager != null)
            {
                dayNightManager.PauseTime(false);
            }
            
            // Enable player input
            EnablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Game resumed");
        }
        
        public void SetTransitioningState()
        {
            if (currentState == GameState.TransitioningDayNight) return;
            
            SetGameState(GameState.TransitioningDayNight);
            
            // Disable player input during transition
            DisablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set transitioning state - player input disabled");
        }
        
        public void EndTransition()
        {
            SetGameState(GameState.Playing);
            EnablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Transition ended - player input re-enabled");
        }
        
        public void SetNightBlockedState()
        {
            SetGameState(GameState.NightBlocked);
            DisablePlayerInput();
            
            // Pause time during night
            if (dayNightManager != null)
            {
                dayNightManager.PauseTime(true);
            }
            
            // Show night message with delay
            StartCoroutine(ShowNightMessageWithDelay());
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set night blocked state - player completely blocked");
        }
        
        private System.Collections.IEnumerator ShowNightMessageWithDelay()
        {
            // Wait a moment for the transition to complete
            yield return new WaitForSeconds(1f);
            
            if (messageSystem != null)
            {
                string nightMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("night_message") : 
                    "...a strange night passes...";
                messageSystem.ShowNightMessage(nightMessage, 3f);
            }
        }
        
        public void SetMenuState()
        {
            SetGameState(GameState.Menu);
            
            // Pause day/night time
            if (dayNightManager != null)
            {
                dayNightManager.PauseTime(true);
            }
            
            // Disable player input
            DisablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set menu state");
        }
        
        public void SetLoadingState()
        {
            SetGameState(GameState.Loading);
            
            // Disable player input
            DisablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Set loading state");
        }
        
        public void SetGameOverState()
        {
            SetGameState(GameState.GameOver);
            
            // Pause day/night time
            if (dayNightManager != null)
            {
                dayNightManager.PauseTime(true);
            }
            
            // Disable player input
            DisablePlayerInput();
            
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
        
        // Public method to check if player can perform actions
        public bool CanPlayerAct()
        {
            return currentState == GameState.Playing && !IsNightBlocked;
        }
        
        // Public method to get current state as string
        public string GetCurrentStateString()
        {
            return currentState.ToString();
        }
        
        // Public method to set FeedbackMessagesSO
        public void SetFeedbackMessages(FeedbackMessagesSO messages)
        {
            feedbackMessages = messages;
            if (showDebugLogs)
                Debug.Log($"[GameStateManager] FeedbackMessagesSO set to: {messages?.name ?? "null"}");
        }
        
        // Public method to force state (for debugging)
        [ContextMenu("Force Playing State")]
        public void ForcePlayingState()
        {
            SetGameState(GameState.Playing);
            EnablePlayerInput();
            
            if (dayNightManager != null)
            {
                dayNightManager.PauseTime(false);
            }
        }
        
        [ContextMenu("Force Paused State")]
        public void ForcePausedState()
        {
            SetGameState(GameState.Paused);
            DisablePlayerInput();
            
            if (dayNightManager != null)
            {
                dayNightManager.PauseTime(true);
            }
        }
        
        [ContextMenu("Force Night Blocked State")]
        public void ForceNightBlockedState()
        {
            SetNightBlockedState();
        }
        
        [ContextMenu("Test Night Message")]
        public void TestNightMessage()
        {
            if (messageSystem != null)
            {
                string nightMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("night_message") : 
                    "...a strange night passes...";
                messageSystem.ShowNightMessage(nightMessage, 3f);
            }
            else
            {
                Debug.LogWarning("[GameStateManager] Cannot test night message - MessageSystem not found");
            }
        }
        
        [ContextMenu("Test Day Message")]
        public void TestDayMessage()
        {
            if (messageSystem != null)
            {
                string dayMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("new_day_started") : 
                    "A new day has begun!";
                messageSystem.ShowMessage(dayMessage, 2f);
            }
            else
            {
                Debug.LogWarning("[GameStateManager] Cannot test day message - MessageSystem not found");
            }
        }
        
        [ContextMenu("Show Current State")]
        public void ShowCurrentState()
        {
            string state = $"Current Game State: {currentState}\n" +
                         $"Can Player Act: {CanPlayerAct()}\n" +
                         $"Is Playing: {IsPlaying}\n" +
                         $"Is Paused: {IsPaused}\n" +
                         $"Is Transitioning: {IsTransitioning}\n" +
                         $"Is Night Blocked: {IsNightBlocked}";
            
            Debug.Log($"[GameStateManager] {state}");
        }
    }
}

// ScriptRole: Manages game states and integrates with day/night system, including night blocking and messages
// RelatedScripts: DayNightManager, PlayerMovement, PlayerInteraction, MouseLook, MessageSystem
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: DayNightManager events
// SendsTo: Player components (enable/disable), MessageSystem (night/day messages)
