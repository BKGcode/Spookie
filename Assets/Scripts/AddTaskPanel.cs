using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddTaskPanel : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskManager taskManager;
    // We might need the IconSetSO directly if we implement icon selection here
    // [SerializeField] private IconSetSO iconSet;

    [Header("UI References (Assign in Inspector)")]
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private ValueSelectorUI hoursSelector;
    [SerializeField] private ValueSelectorUI minutesSelector;
    // Add reference for Icon Selector UI later if needed
    // [SerializeField] private IconSelectorUI iconSelector;
    [SerializeField] private Button addTaskButton;
    [SerializeField] private TextMeshProUGUI feedbackText; // Optional feedback area

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
        if (taskManager == null) { Debug.LogError($"[{gameObject.name}] Task Manager reference not set!", this); return false; }
        if (titleInputField == null) { Debug.LogError($"[{gameObject.name}] Title Input Field not assigned!", this); return false; }
        if (hoursSelector == null) { Debug.LogError($"[{gameObject.name}] Hours Selector not assigned!", this); return false; }
        if (minutesSelector == null) { Debug.LogError($"[{gameObject.name}] Minutes Selector not assigned!", this); return false; }
        if (addTaskButton == null) { Debug.LogError($"[{gameObject.name}] Add Task Button not assigned!", this); return false; }
        // feedbackText is optional
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

        // --- Call TaskManager ---
        taskManager.AddNewTask(title, iconIndex, totalSeconds);

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
            feedbackText.text = GetLocalizedText(messageKey); // Use placeholder localization
        }
        Debug.Log($"AddTaskPanel Feedback: {messageKey}"); // Log for debugging
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }
    }

    // Placeholder for localization
    private string GetLocalizedText(string key)
    {
        return key.Replace("Feedback_", "").Replace("_", " ");
    }
}

// --- Summary Block ---
// ScriptRole: Manages the UI panel for creating new tasks, including title input and time selection using ValueSelectorUI components. Calls TaskManager to add the new task.
// RelatedScripts: TaskManager (receives AddNewTask call), ValueSelectorUI (provides selected time values)
// UsesSO: None directly (potentially IconSetSO if icon selection is added)
// ReceivesFrom: User (button clicks, input field entry)
// SendsTo: TaskManager (AddNewTask)
