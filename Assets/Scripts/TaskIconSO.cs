using UnityEngine;

[CreateAssetMenu(fileName = "TaskIconSet", menuName = "Task Management/Task Icon Set", order = 1)]
public class TaskIconSO : ScriptableObject
{
    [Tooltip("Default icon used if the requested index is invalid or the icon at that index is missing.")]
    public Sprite defaultIcon;

    [Tooltip("Array of available icons for tasks.")]
    public Sprite[] icons;

    /// <summary>
    /// Retrieves the icon sprite for a given index. Falls back to the default icon if necessary.
    /// </summary>
    /// <param name="index">The index of the desired icon in the 'icons' array.</param>
    /// <returns>The requested Sprite, the default Sprite, or null if none are valid/available.</returns>
    public Sprite GetIconByIndex(int index)
    {
        // Check if index is valid and icon exists at that index
        if (icons != null && index >= 0 && index < icons.Length)
        {
            Sprite specificIcon = icons[index];
            if (specificIcon != null)
            {
                return specificIcon; // Found the specific icon
            }
            else
            {
                // Valid index, but the slot is empty
                Debug.LogWarning($"[TaskIconSO] Icon slot at index {index} is empty. Attempting to use default icon.", this);
                // Fall through to return default icon
            }
        }
        else if (icons == null)
        {
             Debug.LogWarning($"[TaskIconSO] 'icons' array is null. Attempting to use default icon.", this);
             // Fall through to return default icon
        }
        else
        {
             // Index is out of bounds
             Debug.LogWarning($"[TaskIconSO] Icon index {index} is out of bounds (Array Length: {icons.Length}). Attempting to use default icon.", this);
             // Fall through to return default icon
        }


        // --- Fallback to Default Icon ---
        if (defaultIcon != null)
        {
            return defaultIcon;
        }


        // --- No Icon Found ---
        // Log error only if we intended to get a specific icon (index was valid or array existed) but failed, AND no default was available.
        if ( (icons != null && index >= 0 && index < icons.Length) || // Index was valid, but icon was null
             (icons != null && (index < 0 || index >= icons.Length)) || // Index was invalid
             (icons == null && index != 0) ) // Array was null, index wasn't trivial 0
        {
             Debug.LogError($"[TaskIconSO] Failed to get icon for index {index} and no default icon is assigned. Returning null.", this);
        }
        else if (icons == null && index == 0)
        {
             Debug.LogError($"[TaskIconSO] Icon array is null, index requested was 0, and no default icon is assigned. Returning null.", this);
        }

        return null; // Absolute fallback
    }

    /// <summary>
    /// Gets a random valid index from the 'icons' array.
    /// </summary>
    /// <returns>A random valid index, or 0 if the array is invalid (caller should handle index 0 potentially being invalid).</returns>
    public int GetRandomIconIndex()
    {
        if (icons == null || icons.Length == 0)
        {
            Debug.LogWarning("[TaskIconSO] Icon array is null or empty. Cannot get random index. Returning 0.", this);
            return 0; // Return 0 as a fallback index
        }
        return Random.Range(0, icons.Length); // Max exclusive
    }
}

// --- Summary Block ---
// ScriptRole: A ScriptableObject holding task icons. Provides methods to retrieve specific, default, or random icons/indices.
// RelatedScripts: TaskSystem (Uses for validation), TaskListUI (Uses indirectly via items), TaskItemMinimal (Uses GetIconByIndex), TaskItemActive (Uses GetIconByIndex)

