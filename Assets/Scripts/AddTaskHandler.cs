using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddTaskHandler : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TaskSystem taskSystem; // Reference to the core task logic

    [Header("UI References")]
    [SerializeField] private TMP_InputField newTaskInputField;
    [SerializeField] private Button addTaskButton;

    void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError($"[{nameof(AddTaskHandler)}] Critical references missing. Disabling component.", this);
            enabled = false;
            return;
        }
        SetupInputListeners();
        Debug.Log($"[{nameof(AddTaskHandler)}] Initialized.");
    }

    private bool ValidateReferences()
    {
        bool isValid = true;
        if (taskSystem == null) { Debug.LogError($"[{nameof(AddTaskHandler)}] Task System reference is missing!", this); isValid = false; }
        if (newTaskInputField == null) { Debug.LogError($"[{nameof(AddTaskHandler)}] New Task Input Field reference is missing!", this); isValid = false; }
        if (addTaskButton == null) { Debug.LogError($"[{nameof(AddTaskHandler)}] Add Task Button reference is missing!", this); isValid = false; }
        return isValid;
    }

    private void SetupInputListeners()
    {
        // Ensure listeners are clean before adding
        addTaskButton.onClick.RemoveAllListeners();
        addTaskButton.onClick.AddListener(HandleAddTask);

        newTaskInputField.onSubmit.RemoveAllListeners();
        newTaskInputField.onSubmit.AddListener((text) => HandleAddTask()); // Add task on Enter key
    }

    private void HandleAddTask()
    {
        string taskTitle = newTaskInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(taskTitle))
        {
            Debug.LogWarning($"[{nameof(AddTaskHandler)}] Add task requested with empty title. Ignoring.", this);
            return; // Ignore empty input
        }

        if (taskSystem != null)
        {
            Debug.Log($"[{nameof(AddTaskHandler)}] Add Task requested for title: '{taskTitle}'. Forwarding to TaskSystem.", this);
            taskSystem.AddTask(taskTitle); // Delegate task creation to the system

            // Clear input and refocus for usability
            newTaskInputField.text = "";
            newTaskInputField.Select();
            newTaskInputField.ActivateInputField();
        }
        else
        {
             Debug.LogError($"[{nameof(AddTaskHandler)}] Cannot add task - TaskSystem reference is null!", this);
        }
    }

    // --- Summary Block ---
    // ScriptRole: Handles the UI interaction (input field and button) for adding new tasks.
    // RelatedScripts: TaskSystem, TaskUIManager
    // UsesSO: None
    // ReceivesFrom: UI (Button Click, InputField Submit)
    // SendsTo: TaskSystem (AddTask)
}

