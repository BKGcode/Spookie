using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TaskItemMinimal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button selectAreaButton;
    [SerializeField] private Image background;
    [SerializeField] private Toggle completedToggle;

    [Header("Data References")]
    [SerializeField] private TaskIconSO taskIconSet;

    [Header("Visuals Config")]
    [SerializeField] private Color normalBackgroundColor = Color.white;
    [SerializeField] private Color selectedBackgroundColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color completedTextColor = Color.grey;
    [SerializeField] private Color normalTextColor = Color.black;

    public event Action<int> DeleteRequested;
    public event Action<int> SelectRequested;
    public event Action<int> ToggleCompleteRequested;

    private int _taskIndex = -1;
    private TaskData _taskData;
    private bool _isInitialized = false;

    public void SetupMinimal(int index, TaskData data)
    {
        _taskIndex = index;
        _taskData = data;

        if (!ValidateReferences())
        {
            Debug.LogError($"[TaskItemMinimal:{_taskIndex}] Missing UI reference(s) on '{gameObject.name}'. Disabling item.", this);
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }
        if (_taskData == null)
        {
            Debug.LogError($"[TaskItemMinimal:{_taskIndex}] SetupMinimal called with null TaskData on '{gameObject.name}'. Disabling item.", this);
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }

        deleteButton.onClick.RemoveAllListeners();
        selectAreaButton.onClick.RemoveAllListeners();
        completedToggle.onValueChanged.RemoveAllListeners();

        deleteButton.onClick.AddListener(HandleDeleteClick);
        selectAreaButton.onClick.AddListener(HandleSelectClick);
        completedToggle.onValueChanged.AddListener(HandleToggleComplete);

        _isInitialized = true;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Initialized for task '{_taskData.title}'.", this);

        UpdateDataVisuals();
        SetSelectedHighlight(false);
    }

    public void UpdateData(TaskData data)
    {
        if (!_isInitialized || data == null) return;
        _taskData = data;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Updating data visuals for task '{_taskData.title}'.", this);
        UpdateDataVisuals();
    }

    public void SetSelectedHighlight(bool selected)
    {
        if (!_isInitialized || background == null) return;
        background.color = selected ? selectedBackgroundColor : normalBackgroundColor;
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        if (titleText == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] titleText is not assigned!", this); valid = false; }
        if (iconImage == null) { Debug.LogWarning($"[TaskItemMinimal:{_taskIndex}] iconImage is not assigned (Optional).", this); }
        if (deleteButton == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] deleteButton is not assigned!", this); valid = false; }
        if (selectAreaButton == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] selectAreaButton is not assigned!", this); valid = false; }
        if (background == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] background image is not assigned! Selection visuals will fail.", this); valid = false; }
        if (completedToggle == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] completedToggle is not assigned!", this); valid = false; }
        if (taskIconSet == null) { Debug.LogWarning($"[TaskItemMinimal:{_taskIndex}] taskIconSet (ScriptableObject) is not assigned. Icons will not be shown.", this); }
        return valid;
    }

    private void SetupIcon()
    {
        if (iconImage == null) return;
        Sprite taskIcon = null;
        if (taskIconSet != null && _taskData != null)
        {
            taskIcon = taskIconSet.GetIconByIndex(_taskData.iconIndex);
        }
        iconImage.sprite = taskIcon;
        iconImage.enabled = (taskIcon != null);
    }

    private void UpdateDataVisuals()
    {
        if (!_isInitialized) return;
        if (titleText != null && _taskData != null)
        {
            titleText.text = _taskData.title;
            UpdateCompletionVisuals(_taskData.isCompleted);
        }
        if (completedToggle != null && _taskData != null)
        {
            completedToggle.SetIsOnWithoutNotify(_taskData.isCompleted);
        }
        SetupIcon();
    }

    private void UpdateCompletionVisuals(bool isCompleted)
    {
        if (!_isInitialized || titleText == null) return;
        titleText.color = isCompleted ? completedTextColor : normalTextColor;
        titleText.fontStyle = isCompleted ? FontStyles.Strikethrough : FontStyles.Normal;
    }

    private void HandleDeleteClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Delete button clicked. Raising DeleteRequested event.", this);
        DeleteRequested?.Invoke(_taskIndex);
    }

    private void HandleSelectClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Select area clicked. Raising SelectRequested event.", this);
        SelectRequested?.Invoke(_taskIndex);
    }

    private void HandleToggleComplete(bool isOn)
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Toggle changed by user interaction to {isOn}. Raising ToggleCompleteRequested event.", this);
        UpdateCompletionVisuals(isOn);
        ToggleCompleteRequested?.Invoke(_taskIndex);
    }

    private void OnDestroy()
    {
        string taskTitle = (_taskData != null) ? _taskData.title : "Unknown/Not Initialized";
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] OnDestroy called for '{taskTitle}'. Cleaning up listeners.", this);
        if (deleteButton != null) deleteButton.onClick.RemoveAllListeners();
        if (selectAreaButton != null) selectAreaButton.onClick.RemoveAllListeners();
        if (completedToggle != null) completedToggle.onValueChanged.RemoveAllListeners();
        DeleteRequested = null;
        SelectRequested = null;
        ToggleCompleteRequested = null;
    }
}

    // --- Summary Block ---
    // ScriptRole: Controls a single minimal task item UI in the list. Displays basic info (title, icon, completion), handles interaction events (select, delete, toggle complete), and updates selection visuals based on external commands.
    // RelatedScripts: TaskListView, TaskUIManager, TaskData, TaskIconSO
    // UsesPrefabs: None directly (it IS the script on the prefab).
    // ReceivesFrom: TaskListView (SetupMinimal, UpdateData, SetSelectedHighlight)
    // SendsTo: TaskListView (via DeleteRequested, SelectRequested, ToggleCompleteRequested events)