using UnityEngine;

namespace DayNightSystem
{
    public enum GameState
    {
        Playing,
        Paused,
        TransitioningDayNight,
        Menu,
        Loading,
        GameOver
    }
    
    public class GameStateManager : MonoBehaviour
    {
        [Header("State Management")]
        [SerializeField] private GameState currentState = GameState.Playing;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private DayNightManager dayNightManager;
        private PlayerController.PlayerMovement playerMovement;
        private PlayerController.PlayerInteraction playerInteraction;
        private PlayerController.MouseLook mouseLook;
        
        // Public properties
        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsTransitioning => currentState == GameState.TransitioningDayNight;
        
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
            }
        }
        
        private void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart -= OnDayNightTransition;
                dayNightManager.OnNightStart -= OnDayNightTransition;
            }
        }
        
        private void FindReferences()
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
            playerMovement = FindObjectOfType<PlayerController.PlayerMovement>();
            playerInteraction = FindObjectOfType<PlayerController.PlayerInteraction>();
            mouseLook = FindObjectOfType<PlayerController.MouseLook>();
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
        }
        
        private void OnDayNightTransition()
        {
            SetTransitioningState();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Day/Night transition detected - setting transitioning state");
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
            if (currentState != GameState.TransitioningDayNight) return;
            
            SetGameState(GameState.Playing);
            
            // Re-enable player input
            EnablePlayerInput();
            
            if (showDebugLogs)
                Debug.Log("[GameStateManager] Transition ended - player input re-enabled");
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
            return currentState == GameState.Playing;
        }
        
        // Public method to get current state as string
        public string GetCurrentStateString()
        {
            return currentState.ToString();
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
        
        [ContextMenu("Show Current State")]
        public void ShowCurrentState()
        {
            string state = $"Current Game State: {currentState}\n" +
                         $"Can Player Act: {CanPlayerAct()}\n" +
                         $"Is Playing: {IsPlaying}\n" +
                         $"Is Paused: {IsPaused}\n" +
                         $"Is Transitioning: {IsTransitioning}";
            
            Debug.Log($"[GameStateManager] {state}");
        }
    }
}

// ScriptRole: Manages game states and integrates with day/night system
// RelatedScripts: DayNightManager, PlayerMovement, PlayerInteraction, MouseLook
// UsesSO: None
// ReceivesFrom: DayNightManager events
// SendsTo: DayNightManager, Player components
