using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; // Needed for UnityEvent

public class AddTaskPanel : MonoBehaviour
{
    // Event triggered when the user confirms adding a new task
    // Parameters: title (string), iconIndex (int), totalSeconds (float)
    public UnityEvent<string, int, float> OnNewTaskRequested;

    [Header("System References")]
    [SerializeField] private FeedbackMessagesSO feedbackMessages; // Reference to message definitions

    [Header("UI References (Assign in Inspector)")]
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private ValueSelectorUI hoursSelector;
    [SerializeField] private ValueSelectorUI minutesSelector;
    // Add reference for Icon Selector UI later if needed
    // [SerializeField] private IconSelectorUI iconSelector;
    [SerializeField] private Button addTaskButton;
    [SerializeField] private TextMeshProUGUI feedbackText; // Optional feedback area

    // --- Reference removed ---
    // [Header("System References")]
    // [SerializeField] private TaskManager taskManager;

    // We might need the IconSetSO directly if we implement icon selection here
    // [SerializeField] private IconSetSO iconSet;


    void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        SetupButtonListeners();
        ClearFeedback();
    }

    private bool ValidateReferences()
    {
        // Removed TaskManager validation
        // if (taskManager == null) { Debug.LogError($"[{gameObject.name}] Task Manager reference not set!", this); return false; }
        if (titleInputField == null) { Debug.LogError($"[{gameObject.name}] Title Input Field not assigned!", this); return false; }
        if (hoursSelector == null) { Debug.LogError($"[{gameObject.name}] Hours Selector not assigned!", this); return false; }
        if (minutesSelector == null) { Debug.LogError($"[{gameObject.name}] Minutes Selector not assigned!", this); return false; }
        if (addTaskButton == null) { Debug.LogError($"[{gameObject.name}] Add Task Button not assigned!", this); return false; }
        // feedbackText is optional

        // Validate FeedbackMessagesSO reference
        if (feedbackMessages == null) { Debug.LogError($"[{gameObject.name}] Feedback Messages SO not assigned!", this); return false; }

        return true;
    }

    private void SetupButtonListeners()
    {
        addTaskButton.onClick.AddListener(TryAddTask);
    }

    private void TryAddTask()
    {
        ClearFeedback();

        // --- Input Validation ---
        string title = titleInputField.text;
        if (string.IsNullOrWhiteSpace(title))
        {
            ShowFeedback("Feedback_TitleRequired"); // Use keys for localization
            return;
        }

        int hours = hoursSelector.CurrentValue;
        int minutes = minutesSelector.CurrentValue;

        if (hours == 0 && minutes == 0)
        {
            ShowFeedback("Feedback_TimeRequired");
            return;
        }

        // --- Calculate Time ---
        float totalSeconds = (hours * 3600f) + (minutes * 60f);

        // --- Icon Selection (Simple version: use default/random) ---
        // TODO: Implement proper icon selection if needed. Using index 0 for now.
        int iconIndex = 0;
        // Or use a random index if IconSetSO is referenced and available
        // if (iconSet != null && iconSet.taskIcons.Count > 0) {
        //     iconIndex = Random.Range(0, iconSet.taskIcons.Count);
        // }

        // --- Invoke Event ---
        OnNewTaskRequested?.Invoke(title, iconIndex, totalSeconds);

        // --- Reset UI ---
        titleInputField.text = "";
        hoursSelector.ResetValue();
        minutesSelector.ResetValue();
        // iconSelector.ResetSelection(); // If implemented

        ShowFeedback("Feedback_TaskAdded");
        // Auto-clear feedback after a delay?
        // Invoke(nameof(ClearFeedback), 3f);
    }

    private void ShowFeedback(string messageKey)
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            // Get message from SO using the key
            feedbackText.text = feedbackMessages != null ? feedbackMessages.GetMessage(messageKey, messageKey) : messageKey;
        }
        // Keep the debug log for development
        Debug.Log($"AddTaskPanel Feedback: {messageKey}");
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }
    }

    // Placeholder for localization - REMOVED
    // private string GetLocalizedText(string key)
    // {
    //     return key.Replace("Feedback_", "").Replace("_", " ");
    // }
}

// --- Summary Block ---
// ScriptRole: Manages the UI panel for creating new tasks. Uses ValueSelectorUI for time, validates input, and invokes OnNewTaskRequested event.
// RelatedScripts: ValueSelectorUI, TaskManager (listens to event), FeedbackMessagesSO (provides UI text)
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: User (input/clicks)
// SendsTo: TaskManager (via OnNewTaskRequested event)
