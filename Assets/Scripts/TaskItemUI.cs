using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class TaskItemUI : MonoBehaviour
{
    [Header("Core UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image taskIconImage;
    [SerializeField] private Image timerStateIcon;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Main Action Button")]
    [SerializeField] private Button mainActionButton;
    [SerializeField] private Image mainActionButtonBackground;
    [SerializeField] private Image mainActionButtonIcon;
    [SerializeField] private Animator mainActionButtonAnimator;

    [Header("Predefined Break Buttons")]
    [SerializeField] private Button break5mButton;
    [SerializeField] private Button break15mButton;

    [Header("Secondary Action Buttons")]
    [SerializeField] private Button completeButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Animator completeButtonAnimator;

    [Header("Visual Assets (Assign Sprites)")]
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite stopBreakIcon;
    [SerializeField] private Sprite completeIcon;
    [SerializeField] private Sprite timerRunningIcon;
    [SerializeField] private Sprite timerPausedIcon;
    [SerializeField] private Sprite timerBreakIcon;

    [Header("State Colors (Assign Colors)")]
    [SerializeField] private Color runningStateColor = Color.green;
    [SerializeField] private Color pausedStateColor = Color.yellow;
    [SerializeField] private Color breakStateColor = Color.cyan;
    [SerializeField] private Color stoppedStateColor = Color.grey;
    [SerializeField] private Color completedStateColor = Color.magenta;
    [SerializeField] private Color normalTimeColor = Color.white;
    [SerializeField] private Color pausedTimeColor = Color.grey;
    [SerializeField] private Color nearlyFinishedColor = Color.red; // Color for near target duration

    [Header("Feedback Effects (Optional Assign)")]
    [SerializeField] private ParticleSystem completeParticles;
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip mainButtonClickSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip breakFinishedSound;
    [SerializeField] private AudioClip nearlyFinishedSound;


    // Events fired when UI buttons are clicked
    public event Action OnMainActionButtonClicked;
    public event Action OnBreak5mClicked;
    public event Action OnBreak15mClicked;
    public event Action OnCompleteClicked;
    public event Action OnResetClicked;
    public event Action OnCloseClicked;

    // Internal state
    private Coroutine _tempStatusCoroutine;
    private bool _isMarkedCompleted = false; // Tracks if the 'Completed' visual style is currently applied

    void Awake()
    {
        // Ensure essential components are grabbed if not assigned
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();

        // Validate mandatory references; disable script if any are missing
        if (!ValidateReferences())
        {
             Debug.LogError("[TaskItemUI] Critical UI references missing. Disabling component.", this);
             enabled = false;
             return;
        }
        SetupButtonListeners();
        SetStatusText("", false); // Clear status initially
        _isMarkedCompleted = false;
        ResetCompletedStylesIfNeeded(); // Ensure normal styles on awake
        Debug.Log("[TaskItemUI] Initialized.");
    }

    private bool ValidateReferences()
    {
        // Check all critical UI elements that MUST be assigned in the Inspector
        bool valid = true;
        if(titleText == null) { Debug.LogError("[TaskItemUI] 'Title Text' (TextMeshProUGUI) is not assigned!", this); valid = false; }
        if(timeText == null) { Debug.LogError("[TaskItemUI] 'Time Text' (TextMeshProUGUI) is not assigned!", this); valid = false; }
        if(statusText == null) { Debug.LogError("[TaskItemUI] 'Status Text' (TextMeshProUGUI) is not assigned!", this); valid = false; }
        if(taskIconImage == null) { Debug.LogError("[TaskItemUI] 'Task Icon Image' (Image) is not assigned!", this); valid = false; }
        if(timerStateIcon == null) { Debug.LogError("[TaskItemUI] 'Timer State Icon' (Image) is not assigned!", this); valid = false; }
        if(canvasGroup == null) { Debug.LogError("[TaskItemUI] 'Canvas Group' component is missing (should be added by RequireComponent)!", this); valid = false; }
        if(mainActionButton == null) { Debug.LogError("[TaskItemUI] 'Main Action Button' (Button) is not assigned!", this); valid = false; }
        if(mainActionButtonBackground == null) { Debug.LogError("[TaskItemUI] 'Main Action Button Background' (Image) is not assigned!", this); valid = false; }
        if(mainActionButtonIcon == null) { Debug.LogError("[TaskItemUI] 'Main Action Button Icon' (Image) is not assigned!", this); valid = false; }
        if(break5mButton == null) { Debug.LogError("[TaskItemUI] 'Break 5m Button' (Button) is not assigned!", this); valid = false; }
        if(break15mButton == null) { Debug.LogError("[TaskItemUI] 'Break 15m Button' (Button) is not assigned!", this); valid = false; }
        if(completeButton == null) { Debug.LogError("[TaskItemUI] 'Complete Button' (Button) is not assigned!", this); valid = false; }
        if(resetButton == null) { Debug.LogError("[TaskItemUI] 'Reset Button' (Button) is not assigned!", this); valid = false; }
        if(closeButton == null) { Debug.LogError("[TaskItemUI] 'Close Button' (Button) is not assigned!", this); valid = false; }

        // Audio source is required, check it too
        if(uiAudioSource == null) { Debug.LogError("[TaskItemUI] 'UI Audio Source' component is missing (should be added by RequireComponent)!", this); valid = false; }

        // Optional references can be checked with warnings if desired
        // if(mainActionButtonAnimator == null) { Debug.LogWarning("[TaskItemUI] 'Main Action Button Animator' is not assigned.", this); }
        // if(completeButtonAnimator == null) { Debug.LogWarning("[TaskItemUI] 'Complete Button Animator' is not assigned.", this); }
        // if(completeParticles == null) { Debug.LogWarning("[TaskItemUI] 'Complete Particles' (ParticleSystem) is not assigned.", this); }
        return valid;
    }

    private void SetupButtonListeners() {
        // Clear existing listeners and add the private handler methods
        mainActionButton.onClick.RemoveAllListeners(); mainActionButton.onClick.AddListener(HandleMainActionButtonClick);
        break5mButton.onClick.RemoveAllListeners(); break5mButton.onClick.AddListener(HandleBreak5mClick);
        break15mButton.onClick.RemoveAllListeners(); break15mButton.onClick.AddListener(HandleBreak15mClick);
        completeButton.onClick.RemoveAllListeners(); completeButton.onClick.AddListener(HandleCompleteClick);
        resetButton.onClick.RemoveAllListeners(); resetButton.onClick.AddListener(HandleResetClick);
        closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(HandleCloseClick);
    }

    // --- Button Handlers (Play sound, trigger animation, invoke event) ---
    private void HandleMainActionButtonClick() { PlaySound(mainButtonClickSound); TriggerAnimation(mainActionButtonAnimator, "OnClick"); OnMainActionButtonClicked?.Invoke(); }
    private void HandleBreak5mClick() { PlaySound(mainButtonClickSound); OnBreak5mClicked?.Invoke(); }
    private void HandleBreak15mClick() { PlaySound(mainButtonClickSound); OnBreak15mClicked?.Invoke(); }
    private void HandleCompleteClick() { PlaySound(mainButtonClickSound); TriggerAnimation(completeButtonAnimator, "OnClick"); OnCompleteClicked?.Invoke(); }
    private void HandleResetClick() { PlaySound(mainButtonClickSound); OnResetClicked?.Invoke(); }
    private void HandleCloseClick() { PlaySound(mainButtonClickSound); OnCloseClicked?.Invoke(); }


    // --- Public Methods to Update UI ---

    /// <summary>Sets the main title text.</summary>
    public void SetTitle(string title) { titleText.text = title ?? "Untitled Task"; }

    /// <summary>Sets the task icon sprite.</summary>
    public void SetTaskIcon(Sprite iconSprite) { if(taskIconImage != null) { taskIconImage.sprite = iconSprite; taskIconImage.enabled = iconSprite != null; } }

    /// <summary>Formats and displays the time, updating color and timer icon based on state.</summary>
    public void SetTimeDisplay(TimeSpan timeSpan, TaskState currentState)
    {
        string formattedTime;
        // Show H:MM:SS if over an hour, otherwise MM:SS
        if (timeSpan.TotalHours >= 1) formattedTime = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        else formattedTime = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        timeText.text = formattedTime;

        // Update time color and timer state icon visibility/sprite based on TaskState
        switch (currentState)
        {
            case TaskState.Running: timeText.color = normalTimeColor; timerStateIcon.sprite = timerRunningIcon; timerStateIcon.enabled = true; break;
            case TaskState.Paused: timeText.color = pausedTimeColor; timerStateIcon.sprite = timerPausedIcon; timerStateIcon.enabled = true; break;
            case TaskState.OnBreak: timeText.color = breakStateColor; timerStateIcon.sprite = timerBreakIcon; timerStateIcon.enabled = true; break; // Break state uses specific color
            case TaskState.Completed: timeText.color = stoppedStateColor; timerStateIcon.enabled = false; break; // Use stopped color for completed time
            case TaskState.Stopped: default: timeText.color = normalTimeColor; timerStateIcon.enabled = false; break; // Normal color, no timer icon when stopped
        }
    }

    /// <summary>Sets the status text message, optionally making it temporary.</summary>
    public void SetStatusText(string message, bool isTemporary = false, float duration = 2.0f)
    {
        if (statusText == null) return;
        statusText.text = message;
        statusText.gameObject.SetActive(!string.IsNullOrEmpty(message)); // Show/hide based on if message exists

        // Stop any previous temporary status timer
        if (_tempStatusCoroutine != null) StopCoroutine(_tempStatusCoroutine);

        // Start a new timer if this is a temporary message
        if (isTemporary && !string.IsNullOrEmpty(message))
        {
            _tempStatusCoroutine = StartCoroutine(ClearStatusAfterDelay(duration));
        }
    }

    /// <summary>Coroutine to clear the status text after a delay.</summary>
    private IEnumerator ClearStatusAfterDelay(float delay)
    {
         yield return new WaitForSeconds(delay);
         if (statusText != null) {
             statusText.text = "";
             statusText.gameObject.SetActive(false);
         }
         _tempStatusCoroutine = null; // Reset coroutine tracker
    }

    /// <summary>Updates button interactability, main button appearance, and timer text color based on the current state.</summary>
    /// <param name="state">The current TaskState.</param>
    /// <param name="currentElapsedTime">The current elapsed time (used for reset button logic).</param>
    /// <param name="isNearlyFinished">Optional flag to change time color if near target duration.</param>
    public void UpdateVisualState(TaskState state, float currentElapsedTime, bool isNearlyFinished = false)
    {
        // Don't update if critical refs are missing or if the completed style is forcefully applied
        if (!enabled || _isMarkedCompleted) return;

        string baseStatusText = ""; // Default status message per state

        // Update Main Action Button appearance (color/icon) and get base status text
        switch (state)
        {
            case TaskState.Running:
                mainActionButtonBackground.color = runningStateColor;
                mainActionButtonIcon.sprite = pauseIcon; // Show Pause icon when running
                baseStatusText = "En ejecución";
                break;
            case TaskState.Paused:
                mainActionButtonBackground.color = pausedStateColor;
                mainActionButtonIcon.sprite = playIcon; // Show Play icon when paused
                baseStatusText = "Pausada";
                break;
            case TaskState.OnBreak:
                mainActionButtonBackground.color = breakStateColor;
                mainActionButtonIcon.sprite = stopBreakIcon; // Show "Stop Break" icon
                baseStatusText = "En descanso";
                break;
             case TaskState.Completed:
                // Should be handled by ShowCompletedState, but prevent state bleeding if called incorrectly
                Debug.LogWarning("[TaskItemUI] UpdateVisualState called for Completed task. Use ShowCompletedState instead.", this);
                return; // Exit early, ShowCompletedState handles visuals
            case TaskState.Stopped:
            default:
                mainActionButtonBackground.color = stoppedStateColor;
                mainActionButtonIcon.sprite = playIcon; // Show Play icon when stopped
                baseStatusText = "Detenida";
                break;
        }
        mainActionButtonIcon.enabled = mainActionButtonIcon.sprite != null; // Ensure icon is visible

        // Update status text only if a temporary message isn't currently displayed
        if (_tempStatusCoroutine == null) SetStatusText(baseStatusText, false);


        // --- Update Button Interactability ---
        // Determine which actions are valid based on the current state
        bool canStart = state == TaskState.Stopped || state == TaskState.Paused;
        bool canPause = state == TaskState.Running;
        bool canStopBreak = state == TaskState.OnBreak;
        bool canBreak = (state == TaskState.Running || state == TaskState.Paused); // Can start break if running or paused
        bool canCompleteViaButton = state != TaskState.Completed; // Can always mark complete unless already done
        bool canReset = state != TaskState.Stopped || currentElapsedTime > 0.1f; // Can reset if not stopped, OR if stopped but has time logged

        // Enable/disable buttons based on valid actions
        mainActionButton.interactable = (canStart || canPause || canStopBreak);
        break5mButton.interactable = canBreak;
        break15mButton.interactable = canBreak;
        completeButton.interactable = canCompleteViaButton;
        resetButton.interactable = canReset;
        closeButton.interactable = true; // Close button is always interactable


        // --- Update Time Text Color for Emphasis ---
        // Use 'nearly finished' color if applicable and running, otherwise use state-appropriate color (set in SetTimeDisplay)
        if (isNearlyFinished && state == TaskState.Running) {
            timeText.color = nearlyFinishedColor;
        }
        // Ensure time color is reset if condition no longer met (relies on SetTimeDisplay setting the base color)
        else if (timeText.color == nearlyFinishedColor) {
             // Re-apply the appropriate color based on current state (redundant if SetTimeDisplay is called after, but safer)
             SetTimeDisplay(TimeSpan.FromSeconds(currentElapsedTime), state);
        }
    }


    /// <summary>Applies a distinct visual style for completed tasks (strikethrough, dimming, specific colors/icons, disabled buttons).</summary>
    public void ShowCompletedState()
    {
        if (!enabled || _isMarkedCompleted) return; // Don't apply if already completed or disabled
        Debug.Log($"[TaskItemUI] Applying completed state visuals for task: '{titleText.text}'");
        _isMarkedCompleted = true;

        // Stop any temporary status message coroutine
        if (_tempStatusCoroutine != null) { StopCoroutine(_tempStatusCoroutine); _tempStatusCoroutine = null; }

        // Apply visual changes for completion
        titleText.fontStyle = FontStyles.Strikethrough;
        titleText.color = stoppedStateColor; // Use greyed-out color
        canvasGroup.alpha = 0.7f; // Dim the entire item slightly
        mainActionButtonBackground.color = completedStateColor; // Specific color for main button bg
        mainActionButtonIcon.sprite = completeIcon; // Specific icon for main button
        mainActionButtonIcon.enabled = true;
        timerStateIcon.enabled = false; // Hide timer state icon
        SetStatusText("¡Completada! 🎉", false); // Set permanent completed status message
        timeText.color = stoppedStateColor; // Grey out the time display

        // Disable all action buttons except 'Close'
        mainActionButton.interactable = false;
        break5mButton.interactable = false;
        break15mButton.interactable = false;
        completeButton.interactable = false;
        resetButton.interactable = false; // Typically cannot reset a completed task easily
        closeButton.interactable = true;

        // Trigger feedback effects
        PlaySound(completeSound);
        PlayParticles(completeParticles);
        TriggerAnimation(completeButtonAnimator, "OnCompleted"); // Use completion animation if available
    }


    /// <summary>Resets visual styles if the task was previously marked completed (e.g., if completion is undone).</summary>
    public void ResetCompletedStylesIfNeeded()
    {
        // Only reset if the completed style was actively applied
        if (!_isMarkedCompleted) return;
        Debug.Log($"[TaskItemUI] Resetting completed styles for task: '{titleText.text}'");
        _isMarkedCompleted = false;

        // Revert visual changes
        titleText.fontStyle = FontStyles.Normal;
        titleText.color = normalTimeColor; // Restore normal title color (might need adjustment based on design)
        canvasGroup.alpha = 1.0f; // Restore full opacity

        // Note: Button states and other visuals should be updated via a subsequent call to UpdateVisualState
    }

    // --- Notifications ---

    /// <summary>Displays a temporary status message and plays a sound for break finishing.</summary>
    public void NotifyBreakFinished()
    {
        SetStatusText("¡Descanso terminado!", true, 3.0f); // Show temporary message
        PlaySound(breakFinishedSound);
        Debug.Log("[TaskItemUI] Notified break finished.");
    }

    /// <summary>Displays a temporary status message and plays a sound when nearing target time.</summary>
    public void NotifyNearlyFinished(bool isNear)
    {
         if (isNear)
         {
             SetStatusText("¡Casi terminado!", true, 1.5f); // Show temporary message
             PlaySound(nearlyFinishedSound);
             Debug.Log("[TaskItemUI] Notified nearly finished.");
         }
         // Note: UpdateVisualState is responsible for applying the nearlyFinishedColor to the timeText
    }


    // --- Helper Methods for Feedback ---
    private void PlaySound(AudioClip clip) { if (uiAudioSource != null && clip != null) { uiAudioSource.PlayOneShot(clip); } }
    private void PlayParticles(ParticleSystem particles) { if (particles != null && particles.gameObject.activeInHierarchy) { particles.Play(); } } // Ensure PS is active
    private void TriggerAnimation(Animator animator, string triggerName) { if (animator != null && animator.isActiveAndEnabled && !string.IsNullOrEmpty(triggerName)) { animator.SetTrigger(triggerName); } } // Ensure Animator is active


    void OnDestroy()
    {
         // Clean up coroutines and event listeners to prevent memory leaks
         if (_tempStatusCoroutine != null) { StopCoroutine(_tempStatusCoroutine); }

         // Explicitly nullify events to break references
         OnMainActionButtonClicked = null;
         OnBreak5mClicked = null;
         OnBreak15mClicked = null;
         OnCompleteClicked = null;
         OnResetClicked = null;
         OnCloseClicked = null;

         Debug.Log($"[TaskItemUI] Destroyed for task: '{titleText?.text ?? "Unknown"}'.");
    }
}

// --- Summary Block ---
// ScriptRole: Manages visual elements (text, buttons, icons, colors, sounds, particles) for the *active* task view. Fires C# events when its buttons are clicked. Receives display instructions via public methods.
// RelatedScripts: TaskItemActive (Controls this UI), TaskState (Enum definition)
// SendsTo: TaskItemActive (via C# events like OnMainActionButtonClicked, OnCompleteClicked, etc.)
// ReceivesFrom: TaskItemActive (calls public methods like SetTitle, SetTimeDisplay, UpdateVisualState, ShowCompletedState)
