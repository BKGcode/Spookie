using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IconSet", menuName = "Spookie/Task/Icon Set", order = 0)]
public class IconSetSO : ScriptableObject
{
    public List<Sprite> taskIcons = new List<Sprite>();
}

// --- Summary Block ---
// ScriptRole: Stores a configurable list of Sprites to be used as task icons throughout the application.
// RelatedScripts: TaskData (references an index), TaskManager (potentially uses it to get icons), TaskItemUI (displays the icon)
// UsesSO: None 