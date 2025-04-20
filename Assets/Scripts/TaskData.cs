using System;
using UnityEngine;

[Serializable]
public class TaskData
{
    public string title;
    public int iconIndex;
    public bool isCompleted;
    public bool isSelected; // Transient state, not typically saved/loaded unless needed

    public TaskState state = TaskState.Stopped;
    public float elapsedTime = 0f;
    public float breakDuration = 0f; // Total duration set for the current break
    public float breakElapsedTime = 0f; // Time elapsed in the current break

    // Constructor
    public TaskData(string taskTitle, int taskIconIndex = 0)
    {
        title = taskTitle;
        iconIndex = taskIconIndex;
        isCompleted = false;
        isSelected = false;
        state = TaskState.Stopped;
        elapsedTime = 0f;
        breakDuration = 0f;
        breakElapsedTime = 0f;
    }

    // Default constructor for serialization
    public TaskData() { }
}

