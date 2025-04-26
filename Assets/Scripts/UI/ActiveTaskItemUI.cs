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

    [Header("Timer Colors")]
    [SerializeField] private Color normalTimerColor = Color.white; // Color por defecto para el temporizador normal
    [SerializeField] private Color breakTimerColor = Color.yellow; // Color por defecto para el temporizador de descanso

    [Header("Button Colors")]
    [SerializeField] private Color startButtonColor = Color.green; // Color para el botón cuando muestra "Start"
    [SerializeField] private Color pauseButtonColor = Color.red; // Color para el botón cuando muestra "Pause"

    [Header("Buttons")]
    [SerializeField] private Button startPauseButton;
    [SerializeField] private TextMeshProUGUI startPauseButtonText; // Text on the Start/Pause button
    [SerializeField] private Button resetButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button closeButton; // The 'X' button
    [SerializeField] private Button deleteButton; // New Delete button

    [Header("Break Duration Buttons")]
    [SerializeField] private Button break10MinButton;
    [SerializeField] private Button break30MinButton;
    [SerializeField] private Button break60MinButton;

    // Runtime Data
    private string currentTaskId;
    private TaskData currentTaskData; // Cache the data for updates
    private Image startPauseButtonImage; // Cache the Image component for color tinting

    // Constants for button text (better to get from FeedbackMessagesSO later)
    // REMOVED private const string START_TEXT = "Start";
    // REMOVED private const string PAUSE_TEXT = "Pause";

    void Awake() // Changed from Start to Awake for image caching
    {
        // Cache the button image component immediately
        if (startPauseButton != null)
        {
            startPauseButtonImage = startPauseButton.GetComponent<Image>();
            if (startPauseButtonImage == null) {
                 Debug.LogWarning($"[{gameObject.name}] Start/Pause Button does not have an Image component for tinting.", this);
            }
        }
    }

    void Start()
    {
        // Add listeners to all buttons
        startPauseButton.onClick.AddListener(HandleStartPauseClick);
        resetButton.onClick.AddListener(HandleResetClick);
        completeButton.onClick.AddListener(HandleCompleteClick);
        closeButton.onClick.AddListener(HandleCloseClick);
        if (deleteButton != null) deleteButton.onClick.AddListener(HandleDeleteClick);

        // Add listeners for break duration buttons
        if (break10MinButton) break10MinButton.onClick.AddListener(() => HandleBreakDurationSelected(10 * 60));
        if (break30MinButton) break30MinButton.onClick.AddListener(() => HandleBreakDurationSelected(30 * 60));
        if (break60MinButton) break60MinButton.onClick.AddListener(() => HandleBreakDurationSelected(60 * 60));

        // Cache the button image component - MOVED TO AWAKE
        // if (startPauseButton != null)
        // {
        //     startPauseButtonImage = startPauseButton.GetComponent<Image>();
        //     if (startPauseButtonImage == null) {
        //          Debug.LogWarning($"[{gameObject.name}] Start/Pause Button does not have an Image component for tinting.", this);
        //     }
        // }
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
            FeedbackMessage messageInfo = feedbackMessages.GetMessageInfo(messageKey);

            if (messageInfo != null)
            {
                feedbackText.text = messageInfo.message;
                feedbackText.gameObject.SetActive(true);

                // Cancel any previous auto-clear invoke
                CancelInvoke(nameof(ClearFeedback));

                // If message is not permanent, schedule auto-clear
                if (!messageInfo.isPermanent)
                {
                    Invoke(nameof(ClearFeedback), feedbackMessages.defaultMessageDuration);
                }
            }
            else
            {
                 // Key not found, clear feedback area
                 ClearFeedback();
            }
        }
    }

     public void ClearFeedback()
     {
        // Stop potentially pending Invoke calls to clear if we clear manually
        CancelInvoke(nameof(ClearFeedback));

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
            float timeToShow;
            Color colorToUse;

            // Determine which time and color to use based on state
            if (currentTaskData != null && currentTaskData.state == TaskState.Break)
            {
                timeToShow = currentTaskData.remainingBreakTime;
                colorToUse = breakTimerColor;
            }
            else
            {
                // Default to remaining task time for Active, Paused, etc.
                timeToShow = remainingSeconds; // Use the passed remainingSeconds (which comes from taskData.remainingTime)
                colorToUse = normalTimerColor;
            }

            if (timeToShow < 0) timeToShow = 0;
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeToShow);
            // Format as HH:MM:SS
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                            (int)timeSpan.TotalHours,
                                            timeSpan.Minutes,
                                            timeSpan.Seconds);

            // Apply the chosen color
            timerText.color = colorToUse;
        }
    }

    private void UpdateButtonStates(TaskState currentState)
    {
        bool isPaused = currentState == TaskState.Paused;
        bool isActive = currentState == TaskState.Active;
        bool isBreak = currentState == TaskState.Break;
        bool isFinished = currentState == TaskState.Completed;
        // Condition for enabling break buttons: Active or Paused, and not Finished.
        bool canStartBreak = (isActive || isPaused) && !isFinished;

        // Start/Pause Button
        startPauseButton.interactable = (isActive || isPaused || isBreak) && !isFinished;
        // Text: Show Pause only if Active, otherwise Start (for Paused and Break states)
        if (startPauseButtonText != null && feedbackMessages != null)
        {
             startPauseButtonText.text = isActive ? feedbackMessages.GetMessage("Button_Pause", "Pause") : feedbackMessages.GetMessage("Button_Start", "Start");
        }
        // Color: Tint based on whether it shows Start or Pause
        if (startPauseButtonImage != null)
        {
             startPauseButtonImage.color = isActive ? pauseButtonColor : startButtonColor;
        }

        // Reset Button
        resetButton.interactable = !isFinished;

        // Break Duration Buttons (Enabled only when Active or Paused)
        if (break10MinButton) break10MinButton.interactable = canStartBreak;
        if (break30MinButton) break30MinButton.interactable = canStartBreak;
        if (break60MinButton) break60MinButton.interactable = canStartBreak;

        // Complete Button
        completeButton.interactable = !isFinished;

        // Close Button
        closeButton.interactable = !isFinished;

        // Delete Button (Enable if not finished)
        if (deleteButton != null) deleteButton.interactable = !isFinished;
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

    // New handler for the delete button
    private void HandleDeleteClick()
    {
        if (taskManager == null) return;
        Debug.Log($"ActiveTaskItemUI: Delete requested for task {currentTaskId}");
        taskManager.RequestDeleteTask(currentTaskId);
    }

    // Optional: Clean up listeners if the object is destroyed/pooled
    // void OnDestroy() { ... remove listeners ... }
}

// --- Summary Block ---
// ScriptRole: Represents a single active task item (right panel). Displays timer, feedback, and handles state changes via buttons (Start/Pause, Reset, Break Durations, Complete, Close).
// RelatedScripts: ActiveTasksUI (instantiates, updates, provides refs), TaskManager (receives calls), IconSetSO, FeedbackMessagesSO
// UsesSO: IconSetSO, FeedbackMessagesSO
// ReceivesFrom: ActiveTasksUI (Setup, UpdateTaskData, ShowFeedback calls), User (button clicks)
// SendsTo: TaskManager (ActivateTask, PauseTask, InterruptBreakAndResume, RequestResetTask, StartBreak, MarkTaskAsCompleted, ReturnTaskToPendingList, RequestDeleteTask)