using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IconSet", menuName = "Spookie/Task/Icon Set", order = 0)]
public class IconSetSO : ScriptableObject
{
    public List<Sprite> taskIcons = new List<Sprite>();

    public Sprite GetIconByIndex(int index)
    {
        if (taskIcons == null || index < 0 || index >= taskIcons.Count)
        {
            // Return null or a default placeholder sprite if index is invalid
            Debug.LogWarning($"Invalid icon index requested: {index}");
            return null; 
        }
        return taskIcons[index];
    }
}

// --- Summary Block ---
// ScriptRole: Holds a configurable list of Sprites to be used as icons for tasks. Provides safe access to icons by index.
// RelatedScripts: TaskManager (references this to get icons), TaskData (stores icon index)
// UsesSO: None 