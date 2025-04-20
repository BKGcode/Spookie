using System;
using UnityEngine;

[Serializable]
public class TaskData
{
    public string title;
    public int iconIndex;
    public bool isCompleted;
    public bool isSelected;

    public TaskState state = TaskState.Stopped;
    public float elapsedTime = 0f;
    public float breakDuration = 0f;
    public float breakElapsedTime = 0f;

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

    public TaskData() { }
}

// Remember to have TaskState defined, e.g.:
// public enum TaskState { Stopped, Running, Paused, OnBreak }
