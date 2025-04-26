using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // For TimeSpan

public class TaskHistoryItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI estimatedDurationText;
    [SerializeField] private TextMeshProUGUI completionDurationText; // Tiempo activo
    [SerializeField] private TextMeshProUGUI totalDurationText; // Tiempo activo + descansos
    // Consider adding completion date TextMeshProUGUI if needed

    [Header("Data References (Optional)")]
    [SerializeField] private IconSetSO iconSet; // Can be assigned here or passed in Setup

    // Cache Task ID if needed later, though not required by current spec
    // private string taskId;

    public void Setup(TaskData taskData, IconSetSO icons)
    {
        // taskId = taskData.id;
        iconSet = icons; // Ensure we have the icon set

        // Set Title
        titleText.text = taskData.title;

        // Set Icon
        if (iconSet != null && iconImage != null)
        {
            iconImage.sprite = iconSet.GetIconByIndex(taskData.iconIndex);
            iconImage.enabled = (iconImage.sprite != null);
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Set Durations using a helper function for formatting
        estimatedDurationText.text = FormatTime(taskData.assignedTime);
        completionDurationText.text = FormatTime(taskData.completionDurationSeconds);
        totalDurationText.text = FormatTime(taskData.totalDurationSeconds);
    }

    private string FormatTime(float totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
        // Format as HH:MM:SS or MM:SS depending on magnitude
        if (timeSpan.TotalHours >= 1)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else
        {
             return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}

// --- Summary Block ---
// ScriptRole: Displays the details of a single completed task in the history panel list.
// RelatedScripts: TaskHistoryUI (instantiates and calls Setup), TaskData (provides data), IconSetSO
// UsesSO: IconSetSO
// ReceivesFrom: TaskHistoryUI (Setup call)
// SendsTo: None 