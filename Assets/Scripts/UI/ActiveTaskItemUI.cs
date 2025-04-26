using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Needed for TimeSpan formatting

public class ActiveTaskItemUI : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private IconSetSO iconSet;
    [SerializeField] private FeedbackMessagesSO feedbackMessages; // To get button texts, maybe?

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI feedbackText; // For messages like "Max tasks reached", "Break Finished", etc.

    [Header("Buttons")]
    [SerializeField] private Button startPauseButton;
    [SerializeField] private TextMeshProUGUI startPauseButtonText; // Text on the Start/Pause button
    [SerializeField] private Button resetButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button closeButton; // The 'X' button

    [Header("Break Duration Buttons")]
    [SerializeField] private Button break10MinButton;
    [SerializeField] private Button break30MinButton;
    [SerializeField] private Button break60MinButton;

    // Runtime Data
    private string currentTaskId;
    private TaskData currentTaskData; // Cache the data for updates

    // Constants for button text (better to get from FeedbackMessagesSO later)
    private const string START_TEXT = "Start";
    private const string PAUSE_TEXT = "Pause";

    void Start()
    {
        // Add listeners to all buttons
        startPauseButton.onClick.AddListener(HandleStartPauseClick);
        resetButton.onClick.AddListener(HandleResetClick);
        completeButton.onClick.AddListener(HandleCompleteClick);
        closeButton.onClick.AddListener(HandleCloseClick);

        // Add listeners for break duration buttons
        if (break10MinButton) break10MinButton.onClick.AddListener(() => HandleBreakDurationSelected(10 * 60));
        if (break30MinButton) break30MinButton.onClick.AddListener(() => HandleBreakDurationSelected(30 * 60));
        if (break60MinButton) break60MinButton.onClick.AddListener(() => HandleBreakDurationSelected(60 * 60));
    }

    public void Setup(TaskData taskData, TaskManager manager, IconSetSO icons, FeedbackMessagesSO messages)
    {
        currentTaskId = taskData.id;
        taskManager = manager; // Get reference from the parent UI manager
        iconSet = icons;
        feedbackMessages = messages;

        // Store initial data and update UI
        UpdateTaskData(taskData);

         // Clear any lingering feedback from previous use (if pooled)
        ClearFeedback();
    }

    // Method to update the UI based on new TaskData (called by ActiveTasksUI)
    public void UpdateTaskData(TaskData taskData)
    {
        currentTaskData = taskData;

        titleText.text = currentTaskData.title;

        // Update Icon
        if (iconSet != null && iconImage != null)
        {
            iconImage.sprite = iconSet.GetIconByIndex(currentTaskData.iconIndex);
            iconImage.enabled = (iconImage.sprite != null);
        }
        else if (iconImage != null)
        {
             iconImage.enabled = false;
        }

        // Update Timer Display
        UpdateTimeDisplay(currentTaskData.remainingTime);

        // Update Button States based on Task State
        UpdateButtonStates(currentTaskData.state);
    }

     // Called by ActiveTasksUI to display feedback messages for this specific task
    public void ShowFeedback(string messageKey)
    {
        if (feedbackText != null && feedbackMessages != null)
        {
            feedbackText.text = feedbackMessages.GetMessage(messageKey);
            feedbackText.gameObject.SetActive(true);
            // Optional: Auto-clear feedback after a delay
             Invoke(nameof(ClearFeedback), 3f);
        }
    }

     public void ClearFeedback()
     {
        if (feedbackText != null)
        {
             feedbackText.text = "";
             feedbackText.gameObject.SetActive(false);
        }
     }


    // --- Internal Update Logic ---

    void Update()
    {
        // Optimization: Only update timer if the task is active or in break
        if (currentTaskData != null && (currentTaskData.state == TaskState.Active || currentTaskData.state == TaskState.Break))
        {
             // We could get the latest data from TaskManager here, but it might be less performant.
             // Assuming TaskManager pushes updates via OnTaskListUpdated and ActiveTasksUI calls UpdateTaskData.
             // For smooth timer display, we might need to directly read remainingTime or handle it differently.
             // Let's update based on the cached data for now, assuming it's refreshed reasonably often.
            UpdateTimeDisplay(currentTaskData.remainingTime);
        }
    }

    private void UpdateTimeDisplay(float remainingSeconds)
    {
        if (timerText != null)
        {
            if (remainingSeconds < 0) remainingSeconds = 0;
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingSeconds);
            // Format as HH:MM:SS
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                            (int)timeSpan.TotalHours,
                                            timeSpan.Minutes,
                                            timeSpan.Seconds);
        }
    }

    private void UpdateButtonStates(TaskState currentState)
    {
        bool isPaused = currentState == TaskState.Paused;
        bool isActive = currentState == TaskState.Active;
        bool isBreak = currentState == TaskState.Break;
        bool isFinished = currentState == TaskState.Completed;
        bool canStartBreak = (isActive || isPaused) && !isFinished;

        // Start/Pause Button
        startPauseButton.interactable = (isActive || isPaused || isBreak) && !isFinished;
        startPauseButtonText.text = (isActive || isBreak) ? PAUSE_TEXT : START_TEXT;

        // Reset Button
        resetButton.interactable = !isFinished;

        // Break Duration Buttons
        if (break10MinButton) break10MinButton.interactable = canStartBreak;
        if (break30MinButton) break30MinButton.interactable = canStartBreak;
        if (break60MinButton) break60MinButton.interactable = canStartBreak;

        // Complete Button
        completeButton.interactable = !isFinished;

        // Close Button
        closeButton.interactable = !isFinished;
    }


    // --- Button Handlers ---

    private void HandleStartPauseClick()
    {
        if (taskManager == null || currentTaskData == null) return;

        switch (currentTaskData.state)
        {
            case TaskState.Paused:
                taskManager.ActivateTask(currentTaskId);
                break;
            case TaskState.Active:
                taskManager.PauseTask(currentTaskId);
                break;
            case TaskState.Break:
                taskManager.InterruptBreakAndResume(currentTaskId);
                break;
        }
        // UI state will update via OnTaskListUpdated -> ActiveTasksUI -> UpdateTaskData
    }

    private void HandleResetClick()
    {
        if (taskManager == null) return;
        taskManager.RequestResetTask(currentTaskId);
    }

    private void HandleBreakDurationSelected(float durationSeconds)
    {
        if (taskManager == null) return;
        taskManager.StartBreak(currentTaskId, durationSeconds);
    }

    private void HandleCompleteClick()
    {
        if (taskManager == null) return;
        taskManager.MarkTaskAsCompleted(currentTaskId);
    }

    private void HandleCloseClick()
    {
        if (taskManager == null) return;
        taskManager.ReturnTaskToPendingList(currentTaskId);
    }

    // Optional: Clean up listeners if the object is destroyed/pooled
    // void OnDestroy() { ... remove listeners ... }
}

// --- Summary Block ---
// ScriptRole: Represents a single active task item (right panel). Displays timer, feedback, and handles state changes via buttons (Start/Pause, Reset, Break Durations, Complete, Close).
// RelatedScripts: ActiveTasksUI (instantiates, updates, provides refs), TaskManager (receives calls), IconSetSO, FeedbackMessagesSO
// UsesSO: IconSetSO, FeedbackMessagesSO
// ReceivesFrom: ActiveTasksUI (Setup, UpdateTaskData, ShowFeedback calls), User (button clicks)
// SendsTo: TaskManager (ActivateTask, PauseTask, InterruptBreakAndResume, RequestResetTask, StartBreak, MarkTaskAsCompleted, ReturnTaskToPendingList)