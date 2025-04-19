// FILE: TaskItemMinimal.cs
using UnityEngine;
using UnityEngine.UI; // Required for Button, Image
using TMPro; // Required for TextMeshProUGUI
using System; // Required for Action<int>

/// <summary>
/// Represents a single task item in the main management list (left side).
/// Displays basic info and handles delete/select actions via delegates.
/// </summary>
public class TaskItemMinimal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage; // Optional icon display
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button selectAreaButton; // Button covering the item for selection
    [SerializeField] private Image background; // To change color on selection

    [Header("Selection Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.9f, 1f); // Light blue

    private int _taskIndex;
    private TaskData _taskData;
    private Action<int> _onDeleteCallback;
    private Action<int> _onSelectCallback;

    private bool _isInitialized = false;

    /// <summary>
    /// Initializes the minimal task item with data and callbacks.
    /// </summary>
    /// <param name="index">The index of the task in the TaskSystem list.</param>
    /// <param name="data">The TaskData object.</param>
    /// <param name="onDelete">Callback function when delete button is pressed.</param>
    /// <param name="onSelect">Callback function when the item is selected.</param>
    public void SetupMinimal(int index, TaskData data, Action<int> onDelete, Action<int> onSelect)
    {
        _taskIndex = index;
        _taskData = data;
        _onDeleteCallback = onDelete;
        _onSelectCallback = onSelect;

        // --- Basic Validation ---
        if (titleText == null || deleteButton == null || selectAreaButton == null || background == null)
        {
            Debug.LogError($"[TaskItemMinimal] Missing UI references on {gameObject.name} for task '{data?.title ?? "UNKNOWN"}'!");
            return;
        }
        if (iconImage == null)
        {
             Debug.LogWarning($"[TaskItemMinimal] Optional iconImage not assigned on {gameObject.name}.");
        }

        // --- Update UI Elements ---
        titleText.text = _taskData.title;
        // TODO: Set iconImage.sprite based on _taskData.iconIndex and TaskIconSO (might need access or pre-loaded sprites)
        // For now, we can disable it if not set:
        if (iconImage) iconImage.enabled = false; // Simple approach for now

        // --- Setup Button Listeners ---
        // Remove previous listeners to prevent duplicates if reused
        deleteButton.onClick.RemoveAllListeners();
        selectAreaButton.onClick.RemoveAllListeners();

        // Add new listeners using the captured index
        deleteButton.onClick.AddListener(HandleDeleteClick);
        selectAreaButton.onClick.AddListener(HandleSelectClick);

        _isInitialized = true;

        // --- Set Initial Visual State ---
        SetSelectedVisual(_taskData.isSelected);
    }

    /// <summary>
    /// Updates the background color based on the selection state.
    /// </summary>
    public void SetSelectedVisual(bool isSelected)
    {
        if (!_isInitialized || background == null) return; // Don't update if not setup or missing background
        background.color = isSelected ? selectedColor : normalColor;
    }

    private void HandleDeleteClick()
    {
        if (!_isInitialized) return;
        // Invoke the callback provided by TaskListUI, passing our task index
        _onDeleteCallback?.Invoke(_taskIndex);
    }

    private void HandleSelectClick()
    {
        if (!_isInitialized) return;
        // Invoke the callback provided by TaskListUI, passing our task index
        _onSelectCallback?.Invoke(_taskIndex);
    }

    // Optional: Called when the GameObject is about to be destroyed
    private void OnDestroy()
    {
        // Clean up listeners if necessary, although usually handled by object destruction
        if (deleteButton != null) deleteButton.onClick.RemoveAllListeners();
        if (selectAreaButton != null) selectAreaButton.onClick.RemoveAllListeners();
    }
}
