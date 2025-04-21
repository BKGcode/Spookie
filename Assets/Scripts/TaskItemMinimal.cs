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

    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.9f, 1f);
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
            Debug.LogError($"[TaskItemMinimal:{_taskIndex}] Missing one or more UI references on {gameObject.name}! Disabling item.", this);
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }
        if (_taskData == null)
        {
            Debug.LogError($"[TaskItemMinimal:{_taskIndex}] SetupMinimal called with null TaskData on {gameObject.name}! Disabling item.", this);
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }

        titleText.text = _taskData.title;
        completedToggle.SetIsOnWithoutNotify(_taskData.isCompleted);
        SetupIcon();
        deleteButton.onClick.RemoveAllListeners();
        selectAreaButton.onClick.RemoveAllListeners();
        completedToggle.onValueChanged.RemoveAllListeners();
        deleteButton.onClick.AddListener(HandleDeleteClick);
        selectAreaButton.onClick.AddListener(HandleSelectClick);
        completedToggle.onValueChanged.AddListener(HandleToggleComplete);
        _isInitialized = true;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Initialized for task '{_taskData.title}'.");
        UpdateVisualState();
    }

    private bool ValidateReferences()
    {
        bool valid = true;
        if (titleText == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] titleText is not assigned!", this); valid = false; }
        if (iconImage == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] iconImage is not assigned!", this); valid = false; }
        if (deleteButton == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] deleteButton is not assigned!", this); valid = false; }
        if (selectAreaButton == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] selectAreaButton is not assigned!", this); valid = false; }
        if (background == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] background is not assigned!", this); valid = false; }
        if (completedToggle == null) { Debug.LogError($"[TaskItemMinimal:{_taskIndex}] completedToggle is not assigned!", this); valid = false; }
        return valid;
    }

    private void SetupIcon()
    {
        if (iconImage == null) return;
        if (taskIconSet == null)
        {
            iconImage.enabled = false;
            return;
        }
        Sprite taskIcon = taskIconSet.GetIconByIndex(_taskData.iconIndex);
        iconImage.sprite = taskIcon;
        iconImage.enabled = (taskIcon != null);
    }

    public void UpdateData(TaskData data)
    {
        if (!_isInitialized || data == null || data == _taskData)
        {
            return;
        }
        _taskData = data;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Updating data for task '{_taskData.title}'.");
        titleText.text = _taskData.title;
        completedToggle.SetIsOnWithoutNotify(_taskData.isCompleted);
        SetupIcon();
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (!_isInitialized) return;
        if (background != null)
        {
            background.color = _taskData.isSelected ? selectedColor : normalColor;
        }
        if (titleText != null)
        {
            bool isCompleted = _taskData.isCompleted;
            titleText.color = isCompleted ? completedTextColor : normalTextColor;
            titleText.fontStyle = isCompleted ? FontStyles.Strikethrough : FontStyles.Normal;
        }
    }

    private void HandleDeleteClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Delete button clicked. Raising DeleteRequested event.");
        DeleteRequested?.Invoke(_taskIndex);
    }

    private void HandleSelectClick()
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Select area clicked. Raising SelectRequested event.");
        SelectRequested?.Invoke(_taskIndex);
    }

    private void HandleToggleComplete(bool isOn)
    {
        if (!_isInitialized) return;
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] Toggle changed by user to {isOn}. Raising ToggleCompleteRequested event.");
        UpdateCompletionVisuals(isOn);
        ToggleCompleteRequested?.Invoke(_taskIndex);
    }

    private void UpdateCompletionVisuals(bool isCompleted)
    {
        if (!_isInitialized || titleText == null) return;
        titleText.color = isCompleted ? completedTextColor : normalTextColor;
        titleText.fontStyle = isCompleted ? FontStyles.Strikethrough : FontStyles.Normal;
    }

    private void OnDestroy()
    {
        Debug.Log($"[TaskItemMinimal:{_taskIndex}] OnDestroy called for '{_taskData?.title ?? "Unknown"}'. Cleaning up listeners.");
        if (deleteButton != null) deleteButton.onClick.RemoveAllListeners();
        if (selectAreaButton != null) selectAreaButton.onClick.RemoveAllListeners();
        if (completedToggle != null) completedToggle.onValueChanged.RemoveAllListeners();
        DeleteRequested = null;
        SelectRequested = null;
        ToggleCompleteRequested = null;
    }
}
