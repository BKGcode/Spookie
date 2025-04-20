using UnityEngine;

[CreateAssetMenu(fileName = "TaskIconSet", menuName = "Task Management/Task Icon Set", order = 1)]
public class TaskIconSO : ScriptableObject
{
    public Sprite defaultIcon; // Optional: An icon to use if index is invalid or not found
    public Sprite[] icons;

    public Sprite GetIconByIndex(int index)
    {
        if (icons != null && index >= 0 && index < icons.Length)
        {
            return icons[index];
        }
        // Return default icon if index is out of bounds or icons array is null/empty
        if (defaultIcon != null)
        {
             Debug.LogWarning($"[TaskIconSO] Icon index {index} out of bounds or invalid. Returning default icon.");
             return defaultIcon;
        }
        // Return null if no default icon is set and index is invalid
        Debug.LogError($"[TaskIconSO] Icon index {index} out of bounds or invalid, and no default icon set. Returning null.");
        return null;
    }
}

