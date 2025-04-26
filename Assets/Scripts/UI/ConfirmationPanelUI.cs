using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; // Needed for UnityAction

public class ConfirmationPanelUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskManager taskManager; // Needed to subscribe to the event
    [SerializeField] private FeedbackMessagesSO feedbackMessages;

    [Header("UI References")]
    [SerializeField] private GameObject panelRoot; // The root GameObject of the panel
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    // Action to execute when confirm is pressed
    private UnityAction currentConfirmAction;

    void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        // Subscribe to the TaskManager's confirmation request event
        taskManager.OnConfirmationRequired.AddListener(ShowConfirmation);

        // Add listeners to buttons
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(HandleCancel);

        // Start hidden
        HidePanel();
    }

    void OnDestroy()
    {
        // Unsubscribe
        if (taskManager != null)
        {
            taskManager.OnConfirmationRequired.RemoveListener(ShowConfirmation);
        }
        // Clean up button listeners to be safe
        confirmButton.onClick.RemoveListener(HandleConfirm);
        cancelButton.onClick.RemoveListener(HandleCancel);
    }

     private bool ValidateReferences()
    {
        if (taskManager == null) { Debug.LogError($"[{gameObject.name}] Task Manager reference not set!", this); return false; }
        if (feedbackMessages == null) { Debug.LogError($"[{gameObject.name}] Feedback Messages SO reference not set!", this); return false; }
        if (panelRoot == null) { Debug.LogError($"[{gameObject.name}] Panel Root GameObject not assigned!", this); return false; }
        if (messageText == null) { Debug.LogError($"[{gameObject.name}] Message Text component not assigned!", this); return false; }
        if (confirmButton == null) { Debug.LogError($"[{gameObject.name}] Confirm Button not assigned!", this); return false; }
        if (cancelButton == null) { Debug.LogError($"[{gameObject.name}] Cancel Button not assigned!", this); return false; }
        return true;
    }

    // Called by the TaskManager's event
    private void ShowConfirmation(string taskId, string messageKey, UnityAction onConfirmAction)
    {
        // We don't strictly need taskId here, but it's part of the event signature
        Debug.Log($"Confirmation requested for task {taskId} with key {messageKey}");

        currentConfirmAction = onConfirmAction;

        // Set the message text
        messageText.text = feedbackMessages.GetMessage(messageKey, "Are you sure?"); // Provide a fallback message

        // Show the panel
        ShowPanel();
    }

    private void HandleConfirm()
    {
        // Execute the stored action
        currentConfirmAction?.Invoke();

        // Hide the panel
        HidePanel();
    }

    private void HandleCancel()
    {
        // Just hide the panel
        HidePanel();
    }

    private void ShowPanel()
    {
        panelRoot.SetActive(true);
        // Optional: Bring to front if needed, or add visual effects
    }

    private void HidePanel()
    {
        panelRoot.SetActive(false);
        currentConfirmAction = null; // Clear the stored action
    }

    // Check for keyboard shortcuts when the panel is active
    void Update()
    {
        // Only process input if the panel is visible
        if (panelRoot != null && panelRoot.activeSelf)
        {
            // Confirm with Enter/Return
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                HandleConfirm();
            }
            // Cancel with Escape
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleCancel();
            }
        }
    }
}

// --- Summary Block ---
// ScriptRole: Displays a confirmation dialog when requested by TaskManager (e.g., for Reset/Delete). Shows a message and executes a specific action upon confirmation.
// RelatedScripts: TaskManager (sends OnConfirmationRequired event), FeedbackMessagesSO (gets message text)
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: TaskManager (OnConfirmationRequired event), User (button clicks)
// SendsTo: TaskManager (indirectly via invoking the received UnityAction on confirm) 