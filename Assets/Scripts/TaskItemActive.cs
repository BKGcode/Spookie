// FILE: TaskItemActive.cs
using UnityEngine;
using UnityEngine.UI; // Required for Button, Image
using TMPro; // Required for TextMeshProUGUI
using System; // Required for Action<int>

/// <summary>
/// Represents a single ACTIVE task item in the right-side list.
/// Displays detailed info (timer) and handles timer controls and closing via delegates.
/// </summary>
public class TaskItemActive : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage; // Optional icon display
    [SerializeField] private TextMeshProUGUI timeText; // Displays elapsed time
    [SerializeField] private Button toggleTimerButton; // Play/Pause functionality
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton; // Deselects the task

    // Optional: References for changing button visuals (e.g., Play/Pause icon)
    [SerializeField] private Image toggleTimerIcon;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;


    private int _taskIndex;
    private TaskData _taskData;
    private Action<int> _onToggleTimerCallback;
    private Action<int> _onResetCallback;
    private Action<int> _onCloseCallback;

    private bool _isInitialized = false;

    /// <summary>
    /// Initializes the active task item with data and callbacks.
    /// </summary>
    /// <param name="index">The index of the task in the TaskSystem list.</param>
    /// <param name="data">The TaskData object.</param>
    /// <param name="onToggleTimer">Callback for the Play/Pause button.</param>
    /// <param name="onReset">Callback for the Reset button.</param>
    /// <param name="onClose">Callback for the Close button.</param>
    public void SetupActive(int index, TaskData data, Action<int> onToggleTimer, Action<int> onReset, Action<int> onClose)
    {
        _taskIndex = index;
        _taskData = data;
        _onToggleTimerCallback = onToggleTimer;
        _onResetCallback = onReset;
        _onCloseCallback = onClose;

        // --- Basic Validation ---
        if (titleText == null || timeText == null || toggleTimerButton == null || resetButton == null || closeButton == null)
        {
            Debug.LogError($"[TaskItemActive] Missing UI references on {gameObject.name} for task '{data?.title ?? "UNKNOWN"}'!");
            return;
        }
         if (iconImage == null) Debug.LogWarning($"[TaskItemActive] Optional iconImage not assigned on {gameObject.name}.");
         if (toggleTimerIcon == null || playIcon == null || pauseIcon == null) Debug.LogWarning($"[TaskItemActive] Optional timer button icons not assigned on {gameObject.name}.");


        // --- Update UI Elements ---
        titleText.text = _taskData.title;
        UpdateTimeDisplay(_taskData.elapsedTime);
        UpdateToggleButtonVisuals(_taskData.isTimerRunning);
        // TODO: Set iconImage.sprite based on _taskData.iconIndex (similar to Minimal item)
        if (iconImage) iconImage.enabled = false; // Simple approach

        // --- Setup Button Listeners ---
        toggleTimerButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        toggleTimerButton.onClick.AddListener(HandleToggleTimerClick);
        resetButton.onClick.AddListener(HandleResetClick);
        closeButton.onClick.AddListener(HandleCloseClick);

        _isInitialized = true;
    }

    /// <summary>
    /// Updates the displayed elapsed time. Called by TaskListUI during Refresh or potentially more often.
    /// </summary>
    public void UpdateTimeDisplay(float elapsedTime)
    {
        if (!_isInitialized || timeText == null) return;
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
        // Format as MM:SS or HH:MM:SS depending on duration
        if (timeSpan.TotalHours >= 1)
            timeText.text = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        else
            timeText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    /// <summary>
    /// Updates the Play/Pause button's icon. Called during Setup and potentially when timer state changes.
    /// </summary>
    public void UpdateToggleButtonVisuals(bool isRunning)
    {
        if (!_isInitialized || toggleTimerIcon == null || playIcon == null || pauseIcon == null) return;
        toggleTimerIcon.sprite = isRunning ? pauseIcon : playIcon;
    }


    private void HandleToggleTimerClick()
    {
        if (!_isInitialized) return;
        _onToggleTimerCallback?.Invoke(_taskIndex);
        // Note: Visuals (button icon, time) will be updated by TaskListUI during the next RefreshUI call
        // triggered by the OnTaskListChanged event from TaskSystem.
    }

    private void HandleResetClick()
    {
        if (!_isInitialized) return;
        _onResetCallback?.Invoke(_taskIndex);
        // Visuals updated by RefreshUI.
    }

    private void HandleCloseClick()
    {
        if (!_isInitialized) return;
        _onCloseCallback?.Invoke(_taskIndex);
        // This item will be destroyed by RefreshUI when the task is deselected.
    }

    private void OnDestroy()
    {
        if (toggleTimerButton != null) toggleTimerButton.onClick.RemoveAllListeners();
        if (resetButton != null) resetButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
    }
}
