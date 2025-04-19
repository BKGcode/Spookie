// FILE: TaskIconSO.cs
using UnityEngine;

/// <summary>
/// ScriptableObject to hold an array of Sprites for task icons.
/// </summary>
[CreateAssetMenu(fileName = "TaskIconData", menuName = "ScriptableObjects/TaskIconSO", order = 1)]
public class TaskIconSO : ScriptableObject
{
    // Array of sprites to be used as icons for tasks. Assign in Inspector.
    public Sprite[] icons;
}
