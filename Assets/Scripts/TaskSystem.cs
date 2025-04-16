using System.Collections.Generic;
using UnityEngine;

public class TaskSystem : MonoBehaviour
{
    private List<string> tasks = new List<string>();

    public void AddTask(string task)
    {
        if (!string.IsNullOrEmpty(task))
        {
            tasks.Add(task);
            Debug.Log($"Task added: {task}");
        }
    }

    public void CompleteTask(int index)
    {
        if (index >= 0 && index < tasks.Count)
        {
            Debug.Log($"Task completed: {tasks[index]}");
            tasks.RemoveAt(index);
        }
        else
        {
            Debug.LogWarning("Invalid task index.");
        }
    }

    public List<string> GetTasks()
    {
        return tasks;
    }
}
