using UnityEngine;
using DayNightSystem;

namespace DayNightSystem.Core
{
    public class DayNightEventController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FeedbackMessagesSO feedbackMessages;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // References
        private DayNightTimeController timeController;
        private DayNightStateController stateController;
        private MessageSystem messageSystem;
        private GameStateManager gameStateManager;
        private AudioManager audioManager;
        
        // Events
        public System.Action OnDayStart;
        public System.Action OnNightStart;
        public System.Action<float> OnExhaustionWarning;
        public System.Action<float> OnTimeChanged;
        public System.Action OnPlayerSlept;
        public System.Action OnPlayerFainted;
        public System.Action OnExhaustionStarted;
        public System.Action<DayNightState> OnStateChanged;
        public System.Action OnNightBlocked;
        public System.Action<float> OnSunsetWarning; // seconds remaining to night
        
        private void Awake()
        {
            FindReferences();
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Initialized");
        }
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged += OnTimeChangedHandler;
                timeController.OnDayTimeReached += OnDayTimeReachedHandler;
                timeController.OnNightTimeReached += OnNightTimeReachedHandler;
                timeController.OnSunsetWarning += OnSunsetWarningHandler;
            }
            
            if (stateController != null)
            {
                stateController.OnStateChanged += OnStateChangedHandler;
                stateController.OnDayStart += OnDayStartHandler;
                stateController.OnNightStart += OnNightStartHandler;
                stateController.OnExhaustionWarning += OnExhaustionWarningHandler;
                stateController.OnPlayerSlept += OnPlayerSleptHandler;
                stateController.OnPlayerFainted += OnPlayerFaintedHandler;
                stateController.OnExhaustionStarted += OnExhaustionStartedHandler;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged -= OnTimeChangedHandler;
                timeController.OnDayTimeReached -= OnDayTimeReachedHandler;
                timeController.OnNightTimeReached -= OnNightTimeReachedHandler;
                timeController.OnSunsetWarning -= OnSunsetWarningHandler;
            }
            
            if (stateController != null)
            {
                stateController.OnStateChanged -= OnStateChangedHandler;
                stateController.OnDayStart -= OnDayStartHandler;
                stateController.OnNightStart -= OnNightStartHandler;
                stateController.OnExhaustionWarning -= OnExhaustionWarningHandler;
                stateController.OnPlayerSlept -= OnPlayerSleptHandler;
                stateController.OnPlayerFainted -= OnPlayerFaintedHandler;
                stateController.OnExhaustionStarted -= OnExhaustionStartedHandler;
            }
        }
        
        private void OnTimeChangedHandler(float time)
        {
            OnTimeChanged?.Invoke(time);
            
            if (showDebugLogs)
                Debug.Log($"[DayNightEventController] Time changed to: {time:F1}s");
        }

        private void OnSunsetWarningHandler(float secondsToNight)
        {
            OnSunsetWarning?.Invoke(secondsToNight);
            
            // Show subtle sunset warning message
            if (messageSystem != null)
            {
                string warning = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("night_falling") : 
                    "Night is falling...";
                messageSystem.ShowMessage(warning, 2f);
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightEventController] Sunset warning dispatched: {secondsToNight:F1}s to night");
        }
        
        private void OnDayTimeReachedHandler()
        {
            OnDayStart?.Invoke();
            
            // Show day start message
            if (messageSystem != null)
            {
                string dayMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("new_day_started") : 
                    "A new day has begun!";
                messageSystem.ShowMessage(dayMessage, 3f);
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Day time reached event triggered");
        }
        
        private void OnNightTimeReachedHandler()
        {
            OnNightStart?.Invoke();
            
            // Show night start message
            if (messageSystem != null)
            {
                string nightMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("night_falling") : 
                    "Night is falling...";
                messageSystem.ShowMessage(nightMessage, 3f);
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Night time reached event triggered");
        }
        
        private void OnStateChangedHandler(DayNightState newState)
        {
            OnStateChanged?.Invoke(newState);
            
            // Handle specific state changes
            switch (newState)
            {
                case DayNightState.Night:
                    HandleNightState();
                    break;
                case DayNightState.Day:
                    HandleDayState();
                    break;
                case DayNightState.Exhausted:
                    HandleExhaustedState();
                    break;
                case DayNightState.Sleeping:
                    HandleSleepingState();
                    break;
                case DayNightState.Fainted:
                    HandleFaintedState();
                    break;
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightEventController] State changed to: {newState}");
        }
        
        private void OnDayStartHandler()
        {
            OnDayStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Day start event triggered");
        }
        
        private void OnNightStartHandler()
        {
            OnNightStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Night start event triggered");
        }
        
        private void OnExhaustionWarningHandler(float timer)
        {
            OnExhaustionWarning?.Invoke(timer);
            
            // Show exhaustion warning message
            if (messageSystem != null)
            {
                string warningMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("exhaustion_warning") : 
                    "Warning: Exhaustion approaching!";
                messageSystem.ShowMessage(warningMessage, 2f);
            }
            
            // Play warning sound
            if (audioManager != null)
            {
                audioManager.PlayWarningSound();
            }
            
            if (showDebugLogs)
                Debug.Log($"[DayNightEventController] Exhaustion warning at {timer:F1}s");
        }
        
        private void OnPlayerSleptHandler()
        {
            OnPlayerSlept?.Invoke();
            
            // Show sleep message
            if (messageSystem != null)
            {
                string sleepMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("status_sleeping") : 
                    "Sleeping...";
                messageSystem.ShowMessage(sleepMessage, 2f);
            }
            
            // Play sleep sound
            if (audioManager != null)
            {
                audioManager.PlaySleepSound();
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Player slept event triggered");
        }
        
        private void OnPlayerFaintedHandler()
        {
            OnPlayerFainted?.Invoke();
            
            // Show faint message
            if (messageSystem != null)
            {
                string faintMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("status_fainted") : 
                    "Fainted - You will be penalized";
                messageSystem.ShowMessage(faintMessage, 3f);
            }
            
            // Play faint sound
            if (audioManager != null)
            {
                audioManager.PlayFaintSound();
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Player fainted event triggered");
        }
        
        private void OnExhaustionStartedHandler()
        {
            OnExhaustionStarted?.Invoke();
            
            // Show exhaustion message
            if (messageSystem != null)
            {
                string exhaustionMessage = feedbackMessages != null ? 
                    feedbackMessages.GetMessage("exhaustion_active") : 
                    "You are exhausted! Return to spawn!";
                messageSystem.ShowMessage(exhaustionMessage, 3f);
            }
            
            // Play penalty sound
            if (audioManager != null)
            {
                audioManager.PlayPenaltySound();
            }
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Exhaustion started event triggered");
        }
        
        private void HandleNightState()
        {
            // Notify that night is blocked
            OnNightBlocked?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Night state detected - blocking player completely");
        }
        
        private void HandleDayState()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Day state detected - resuming normal gameplay");
        }
        
        private void HandleExhaustedState()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Player is now exhausted");
        }
        
        private void HandleSleepingState()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Player is sleeping");
        }
        
        private void HandleFaintedState()
        {
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Player has fainted");
        }
        
        private void FindReferences()
        {
            timeController = GetComponent<DayNightTimeController>();
            stateController = GetComponent<DayNightStateController>();
            messageSystem = FindObjectOfType<MessageSystem>();
            gameStateManager = FindObjectOfType<GameStateManager>();
            audioManager = FindObjectOfType<AudioManager>();
        }
        
        private void ValidateReferences()
        {
            if (feedbackMessages == null)
            {
                Debug.LogWarning("[DayNightEventController] FeedbackMessagesSO reference is missing!");
            }
            
            if (timeController == null)
            {
                Debug.LogError("[DayNightEventController] DayNightTimeController reference is missing!");
            }
            
            if (stateController == null)
            {
                Debug.LogError("[DayNightEventController] DayNightStateController reference is missing!");
            }
        }
        
        [ContextMenu("Test Day Event")]
        public void TestDayEvent()
        {
            OnDayStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Test day event triggered");
        }
        
        [ContextMenu("Test Night Event")]
        public void TestNightEvent()
        {
            OnNightStart?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Test night event triggered");
        }
        
        [ContextMenu("Test Exhaustion Warning")]
        public void TestExhaustionWarning()
        {
            OnExhaustionWarning?.Invoke(25f);
            
            if (showDebugLogs)
                Debug.Log("[DayNightEventController] Test exhaustion warning triggered");
        }
        
        [ContextMenu("Show Event Status")]
        public void ShowEventStatus()
        {
            Debug.Log($"[DayNightEventController] Event Status - TimeController: {timeController != null}, StateController: {stateController != null}, MessageSystem: {messageSystem != null}");
        }
    }
}
