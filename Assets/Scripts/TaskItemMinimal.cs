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

    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color completedTextColor = Color.grey;
    [SerializeField] private Color normalTextColor = Color.white;

    private int _taskIndex;
    private TaskData _taskData;
    private Action<int> _onDeleteCallback;
    private Action<int> _onSelectCallback;
    private Action<int> _onToggleCompleteCallback;

    private bool _isInitialized = false;

    public void SetupMinimal(
        int index,
        TaskData data,
        Action<int> onDelete,
        Action<int> onSelect,
        Action<int> onToggleComplete
        )
    {
        _taskIndex = index;
        _taskData = data;
        _onDeleteCallback = onDelete;
        _onSelectCallback = onSelect;
        _onToggleCompleteCallback = onToggleComplete;

        if (titleText == null || deleteButton == null || selectAreaButton == null || background == null || completedToggle == null)
        {
            Debug.LogError($"[TaskItemMinimal] Missing UI references on {gameObject.name} for task '{data?.title ?? "UNKNOWN"}'!");
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }
        if (iconImage == null)
        {
        }

        titleText.text = _taskData.title;
        completedToggle.SetIsOnWithoutNotify(_taskData.isCompleted);

        if (iconImage) iconImage.enabled = false;

        deleteButton.onClick.RemoveAllListeners();
        selectAreaButton.onClick.RemoveAllListeners();
        completedToggle.onValueChanged.RemoveAllListeners();

        deleteButton.onClick.AddListener(HandleDeleteClick);
        selectAreaButton.onClick.AddListener(HandleSelectClick);
        completedToggle.onValueChanged.AddListener(HandleToggleComplete);

        _isInitialized = true;

        SetSelectedVisual(_taskData.isSelected);
        UpdateCompletionVisuals(_taskData.isCompleted);
    }

    public void SetSelectedVisual(bool isSelected)
    {
        if (!_isInitialized || background == null) return;
        background.color = isSelected ? selectedColor : normalColor;
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
        _onDeleteCallback?.Invoke(_taskIndex);
    }

    private void HandleSelectClick()
    {
        if (!_isInitialized) return;
        _onSelectCallback?.Invoke(_taskIndex);
    }

    private void HandleToggleComplete(bool isOn)
    {
        if (!_isInitialized) return;
        _onToggleCompleteCallback?.Invoke(_taskIndex);
        UpdateCompletionVisuals(isOn);
    }

    private void OnDestroy()
    {
        if (deleteButton != null) deleteButton.onClick.RemoveAllListeners();
        if (selectAreaButton != null) selectAreaButton.onClick.RemoveAllListeners();
        if (completedToggle != null) completedToggle.onValueChanged.RemoveAllListeners();
    }
}

