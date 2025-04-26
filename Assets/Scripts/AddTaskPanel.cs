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
    [SerializeField] private IconSetSO iconSet; // Reference to the icon definitions

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
        // Validate IconSetSO reference
        if (iconSet == null) { Debug.LogError($"[{gameObject.name}] Icon Set SO not assigned!", this); return false; }

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
        int iconIndex = iconSet.GetRandomIconIndex(); // Get random icon using the cycle logic

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
            // Ensure we have the SO reference
            if (feedbackMessages == null) {
                 Debug.LogError($"[{gameObject.name}] Feedback Messages SO not assigned! Cannot show feedback.", this);
                 feedbackText.text = messageKey; // Show key as fallback
                 feedbackText.gameObject.SetActive(true);
                 return;
            }

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
                // Key not found, use key as text and schedule clear
                feedbackText.text = messageKey; // Show key as fallback
                feedbackText.gameObject.SetActive(true);
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), feedbackMessages.defaultMessageDuration);
            }
        }
        // Keep the debug log for development
        Debug.Log($"AddTaskPanel Feedback: {messageKey}");
    }

    private void ClearFeedback()
    {
        // Stop potentially pending Invoke calls to clear if we clear manually
        CancelInvoke(nameof(ClearFeedback));

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
// UsesSO: FeedbackMessagesSO, IconSetSO
// ReceivesFrom: User (input/clicks)
// SendsTo: TaskManager (via OnNewTaskRequested event)
