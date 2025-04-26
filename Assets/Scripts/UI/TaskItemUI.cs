using UnityEngine;
using UnityEngine.UI; // Required for Button
using TMPro; // Required for TextMeshPro elements
using System; // Required for TimeSpan

public class TaskItemUI : MonoBehaviour
{
    [Header("Data References")]
    private TaskData currentTaskData;
    private TaskManager taskManager; // Reference set by the panel managing this item

    [Header("UI Element References (Assign in Prefab)")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timerText; // Displays remaining time or state info
    [SerializeField] private TextMeshProUGUI feedbackText; // For contextual messages
    [SerializeField] private Button mainActionButton; // Start/Pause/Resume button
    [SerializeField] private TextMeshProUGUI mainActionButtonText;
    [SerializeField] private Button stopButton; // "Park" button (move to left panel)
    [SerializeField] private Button completeButton; // Force complete
    [SerializeField] private Button resetButton; // Reset timer to assigned time
    [SerializeField] private Button break10mButton;
    [SerializeField] private Button break30mButton;
    [SerializeField] private Button break1hButton;
    [SerializeField] private Button deleteButton; // Or maybe move this elsewhere? Add for now.

    // Cached strings for button text to avoid allocations
    private const string startTextKey = "Btn_Start"; // Keys for localization
    private const string pauseTextKey = "Btn_Pause";
    private const string resumeTextKey = "Btn_Resume";

    private bool isInitialized = false;

    public void Initialize(TaskData taskData, TaskManager manager)
    {
        currentTaskData = taskData;
        taskManager = manager;

        if (!ValidateReferences())
        {
            isInitialized = false;
            return;
        }

        SetupButtonListeners();
        UpdateDisplay(); // Initial display update

        // Subscribe to specific feedback for this task ONLY
        // Note: This requires TaskManager to potentially send targeted feedback.
        // Or, the managing panel could handle feedback distribution.
        // For simplicity now, feedbackText might be updated directly in UpdateDisplay.
        // taskManager.OnTaskFeedbackRequested += HandleFeedback; // Potential implementation

        isInitialized = true;
    }

    // Called by the managing panels when task data might have changed
    public void Refresh(TaskData taskData)
    {
        if (!isInitialized || currentTaskData.id != taskData.id)
        {
            // If not initialized or ID mismatch, re-initialize fully
            Initialize(taskData, taskManager);
            return;
        }
        // Otherwise, just update data and refresh display
        currentTaskData = taskData;
        UpdateDisplay();
    }


    private bool ValidateReferences()
    {
        if (taskManager == null) { Debug.LogError($"[{gameObject.name}] Task Manager reference is null!"); return false; }
        if (currentTaskData == null) { Debug.LogError($"[{gameObject.name}] Task Data is null!"); return false; }
        if (iconImage == null) { Debug.LogError($"[{gameObject.name}] Icon Image is not assigned!"); return false; }
        if (titleText == null) { Debug.LogError($"[{gameObject.name}] Title Text is not assigned!"); return false; }
        if (timerText == null) { Debug.LogError($"[{gameObject.name}] Timer Text is not assigned!"); return false; }
        if (feedbackText == null) { Debug.LogError($"[{gameObject.name}] Feedback Text is not assigned!"); return false; }
        if (mainActionButton == null) { Debug.LogError($"[{gameObject.name}] Main Action Button is not assigned!"); return false; }
        if (mainActionButtonText == null) { Debug.LogError($"[{gameObject.name}] Main Action Button Text is not assigned!"); return false; }
        if (stopButton == null) { Debug.LogError($"[{gameObject.name}] Stop Button is not assigned!"); return false; }
        if (completeButton == null) { Debug.LogError($"[{gameObject.name}] Complete Button is not assigned!"); return false; }
        if (resetButton == null) { Debug.LogError($"[{gameObject.name}] Reset Button is not assigned!"); return false; }
        if (break10mButton == null) { Debug.LogError($"[{gameObject.name}] Break 10m Button is not assigned!"); return false; }
        if (break30mButton == null) { Debug.LogError($"[{gameObject.name}] Break 30m Button is not assigned!"); return false; }
        if (break1hButton == null) { Debug.LogError($"[{gameObject.name}] Break 1h Button is not assigned!"); return false; }
        if (deleteButton == null) { Debug.LogError($"[{gameObject.name}] Delete Button is not assigned!"); return false; }

        return true;
    }

    private void SetupButtonListeners()
    {
        // Remove previous listeners to prevent duplicates if re-initialized
        mainActionButton.onClick.RemoveAllListeners();
        stopButton.onClick.RemoveAllListeners();
        completeButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();
        break10mButton.onClick.RemoveAllListeners();
        break30mButton.onClick.RemoveAllListeners();
        break1hButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        // Add new listeners
        mainActionButton.onClick.AddListener(OnMainActionButtonClicked);
        stopButton.onClick.AddListener(OnStopButtonClicked);
        completeButton.onClick.AddListener(OnCompleteButtonClicked);
        resetButton.onClick.AddListener(OnResetButtonClicked);
        break10mButton.onClick.AddListener(() => OnBreakButtonClicked(600f)); // 10 min * 60 sec
        break30mButton.onClick.AddListener(() => OnBreakButtonClicked(1800f)); // 30 min * 60 sec
        break1hButton.onClick.AddListener(() => OnBreakButtonClicked(3600f)); // 60 min * 60 sec
        // deleteButton.onClick.AddListener(OnDeleteButtonClicked); // Consider if delete is needed here
    }

    private void UpdateDisplay()
    {
        if (!isInitialized) return;

        titleText.text = currentTaskData.title;
        // Use TaskManager to get the actual icon sprite
        iconImage.sprite = taskManager.GetIconByIndex(currentTaskData.iconIndex);
        iconImage.enabled = (iconImage.sprite != null); // Show/hide icon

        // Clear feedback initially, specific actions might set it
        feedbackText.text = "";

        // Format timer (e.g., HH:MM:SS)
        TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, currentTaskData.remainingTime));
        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

        // Configure buttons and main action text based on state
        ConfigureButtonsForState(currentTaskData.state);
    }

    private void ConfigureButtonsForState(TaskState state)
    {
        // Default visibility (adjust as needed per state)
        mainActionButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(true); // "Park" button
        completeButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        break10mButton.gameObject.SetActive(true);
        break30mButton.gameObject.SetActive(true);
        break1hButton.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(true); // Decide if needed per state

        // State-specific adjustments
        switch (state)
        {
            case TaskState.Pending:
            case TaskState.Stopped: // States typically in the left panel
                mainActionButtonText.text = GetLocalizedText(startTextKey); // Show "Start"
                stopButton.gameObject.SetActive(false); // Cannot "Park" if already parked/pending
                break10mButton.gameObject.SetActive(false); // Cannot break if not active/paused
                break30mButton.gameObject.SetActive(false);
                break1hButton.gameObject.SetActive(false);
                timerText.text = "--:--:--"; // Or show assigned time?
                break;

            case TaskState.Active: // State in the right panel
                mainActionButtonText.text = GetLocalizedText(pauseTextKey); // Show "Pause"
                // All buttons might be visible here
                break;

            case TaskState.Paused: // State in the right panel
                mainActionButtonText.text = GetLocalizedText(resumeTextKey); // Show "Resume" / "Start"
                // All buttons might be visible here
                break;

            case TaskState.Break: // State in the right panel
                mainActionButtonText.text = GetLocalizedText(resumeTextKey); // Show "Resume" to interrupt break
                // Disable break buttons while already on break?
                break10mButton.interactable = false;
                break30mButton.interactable = false;
                break1hButton.interactable = false;
                // Optionally show break timer in timerText or feedbackText
                // You might need access to currentBreaks from TaskManager for this
                timerText.text = "BREAK"; // Placeholder
                break;

            case TaskState.Completed: // Should ideally not be displayed by this UI, but handle just in case
                 gameObject.SetActive(false); // Hide completed tasks from main lists
                // Or configure all buttons inactive etc.
                break;
        }

        // TODO: Add logic for timer finished state (showing Reset/Complete options)
        // This might involve checking if remainingTime <= 0 and state is Paused (as set by HandleTimerFinished)
         if (currentTaskData.remainingTime <= 0f && currentTaskData.state == TaskState.Paused)
         {
             HandleDisplayForTimerFinished();
         }
    }

     private void HandleDisplayForTimerFinished()
     {
         feedbackText.text = GetLocalizedText("Feedback_TimerFinished"); // Use a key
         // Decide which buttons to show: Reset? Complete?
         // Let's show Reset and Complete, hide others?
         mainActionButton.gameObject.SetActive(false);
         stopButton.gameObject.SetActive(false);
         break10mButton.gameObject.SetActive(false);
         break30mButton.gameObject.SetActive(false);
         break1hButton.gameObject.SetActive(false);

         resetButton.gameObject.SetActive(true);
         completeButton.gameObject.SetActive(true);
         // Delete button visibility? Up to design.
     }


    // --- Button Click Handlers ---

    private void OnMainActionButtonClicked()
    {
        if (!isInitialized) return;
        switch (currentTaskData.state)
        {
            case TaskState.Pending:
            case TaskState.Stopped:
            case TaskState.Paused:
                taskManager.ActivateTask(currentTaskData.id);
                break;
            case TaskState.Active:
                taskManager.PauseTask(currentTaskData.id);
                break;
             case TaskState.Break:
                 taskManager.InterruptBreakAndResume(currentTaskData.id);
                 break;
        }
        // UpdateDisplay(); // TaskManager events will trigger refresh via the Panel script
    }

    private void OnStopButtonClicked()
    {
        if (!isInitialized) return;
        taskManager.StopTask(currentTaskData.id);
    }

    private void OnCompleteButtonClicked()
    {
        if (!isInitialized) return;
         // Confirmation might be good here
        taskManager.MarkTaskAsCompleted(currentTaskData.id);
    }

     private void OnResetButtonClicked()
    {
        if (!isInitialized) return;
         // Confirmation might be good here
        taskManager.ResetTaskTimer(currentTaskData.id);
    }


    private void OnBreakButtonClicked(float durationSeconds)
    {
        if (!isInitialized) return;
        taskManager.StartBreak(currentTaskData.id, durationSeconds);
    }

    // Example for Delete - Implement if needed
    // private void OnDeleteButtonClicked()
    // {
    //     if (!isInitialized) return;
    //     // Confirmation recommended!
    //     // taskManager.RemoveTask(currentTaskData.id); // Need RemoveTask method in TaskManager
    //     // This GameObject should then be destroyed by the panel script
    // }


    // --- Feedback Handling ---
    // This needs a proper implementation based on how feedback is managed
    // Option 1: Specific event listener (requires TaskManager changes)
    // Option 2: Panel checks feedback queue on update and calls a method here
    public void ShowFeedback(string messageKey)
    {
        if(feedbackText != null)
        {
            feedbackText.text = GetLocalizedText(messageKey);
            // Optional: Add a timer to clear the feedback after a few seconds
        }
    }


    // Placeholder for localization - replace with your actual system
    private string GetLocalizedText(string key)
    {
        // In a real project, look up key in a localization dictionary
        return key.Replace("Btn_", "").Replace("Feedback_", "").Replace("_", " ");
    }


    // --- Cleanup ---
    // void OnDestroy()
    // {
        // Unsubscribe from events if necessary
        // if (taskManager != null)
        // {
        //     taskManager.OnTaskFeedbackRequested -= HandleFeedback;
        // }
    // }
}

// --- Summary Block ---
// ScriptRole: Controls the UI elements of a single task item prefab. Displays task data, handles button clicks, and updates appearance based on task state.
// RelatedScripts: TaskManager (receives commands from buttons), TaskData (data source), TaskListPanel/ActiveTaskPanel (instantiates and manages this UI item)
// UsesSO: None directly (gets icon via TaskManager referencing IconSetSO)
// ReceivesFrom: TaskListPanel/ActiveTaskPanel (calls Initialize/Refresh), User (button clicks)
// SendsTo: TaskManager (calls public methods like ActivateTask, PauseTask, etc.) 