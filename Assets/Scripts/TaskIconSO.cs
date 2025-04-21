using UnityEngine;

[CreateAssetMenu(fileName = "TaskIconSet", menuName = "Task Management/Task Icon Set", order = 1)]
public class TaskIconSO : ScriptableObject
{
    public Sprite defaultIcon;
    public Sprite[] icons;

    public Sprite GetIconByIndex(int index)
    {
        if (icons != null && index >= 0 && index < icons.Length)
        {
            Sprite icon = icons[index];
            if (icon != null)
            {
                return icon;
            }
            else
            {
                Debug.LogWarning($"[TaskIconSO] Icon at index {index} is null. Returning default icon if available.");
            }
        }

        if (defaultIcon != null)
        {
            // Removed redundant log from original if branch led here
            if (!(icons != null && index >= 0 && index < icons.Length))
            {
                 Debug.LogWarning($"[TaskIconSO] Icon index {index} out of bounds or invalid (Array Length: {(icons != null ? icons.Length : 0)}). Returning default icon.");
            }
            return defaultIcon;
        }

        // Only log error if index was actually invalid AND no default exists
        if (!(icons != null && index >= 0 && index < icons.Length))
        {
            Debug.LogError($"[TaskIconSO] Icon index {index} out of bounds or invalid (Array Length: {(icons != null ? icons.Length : 0)}), and no default icon set. Returning null.");
        }
        // If icon at valid index was null and no default exists
        else if (icons != null && icons[index] == null)
        {
             Debug.LogError($"[TaskIconSO] Icon at index {index} is null, and no default icon set. Returning null.");
        }

        return null;
    }

    // --- Added Method ---
    public int GetRandomIconIndex()
    {
        if (icons == null || icons.Length == 0)
        {
            Debug.LogWarning("[TaskIconSO] Icon array is null or empty. Cannot get random index. Returning 0.");
            // Returning 0 implies the defaultIcon might be used if available, or an error if index 0 is invalid later.
            return 0;
        }
        return Random.Range(0, icons.Length);
    }
    // --- End Added Method ---
}
