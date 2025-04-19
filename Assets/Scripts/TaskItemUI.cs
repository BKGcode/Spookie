using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class TaskItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI taskNameText;
    public Toggle completedToggle;
    public Button deleteButton;


    private int taskIndex;
    private Action<int, bool> onToggleCompleted;
    private Action<int> onDelete;


    /// <summary>
    /// Inicializa el ítem visual de la tarea.
    /// </summary>
    public void Setup(int index, TaskData data, Sprite icon, Action<int, bool> onCompleted, Action<int> onDeleteCallback)
    {
        taskIndex = index;


        taskNameText.text = data.title;
        iconImage.sprite = icon;
        completedToggle.isOn = data.isCompleted;


        onToggleCompleted = onCompleted;
        onDelete = onDeleteCallback;


        completedToggle.onValueChanged.RemoveAllListeners();
        completedToggle.onValueChanged.AddListener((bool value) => onToggleCompleted?.Invoke(taskIndex, value));


        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete?.Invoke(taskIndex));
    }


    private void OnDestroy()
    {
        completedToggle.onValueChanged.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
    }
}
