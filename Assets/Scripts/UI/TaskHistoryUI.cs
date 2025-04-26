using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Para OrderByDescending y Take si TaskManager no lo hiciera
// using TMPro; // Uncomment if using TextMeshPro for Close button or title

public class TaskHistoryUI : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private IconSetSO iconSet;
    [SerializeField] private FeedbackMessagesSO feedbackMessages; // Para texto del botón cerrar, si es configurable

    [Header("UI References")]
    [SerializeField] private GameObject historyPanel; // El panel principal a mostrar/ocultar
    [SerializeField] private RectTransform contentArea; // El contenedor dentro del ScrollView donde instanciar items
    [SerializeField] private GameObject historyItemPrefab; // El prefab con TaskHistoryItemUI
    [SerializeField] private Button closeButton;
    // Consider adding a TextMeshProUGUI for a title like "Task History"

    [Header("Configuration")]
    [SerializeField] private int maxHistoryItems = 50; // Límite de elementos a mostrar

    private List<GameObject> instantiatedItems = new List<GameObject>(); // Para limpiar la lista

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
            // Opcional: Configurar texto del botón desde FeedbackMessagesSO
             // TextMeshProUGUI closeButtonText = closeButton.GetComponentInChildren<TextMeshProUGUI>();
             // if (closeButtonText != null && feedbackMessages != null)
             // {
             //    closeButtonText.text = feedbackMessages.GetMessage("Button_Close", "Close");
             // }
        }
        else
        {
            Debug.LogWarning("History Panel: Close Button not assigned.", this);
        }

        if (historyPanel != null)
        {
            historyPanel.SetActive(false); // Ocultar al inicio
        }
        else
        {
             Debug.LogError("History Panel: Panel GameObject not assigned.", this);
        }

        if (taskManager == null) Debug.LogError("History Panel: TaskManager not assigned.", this);
        if (iconSet == null) Debug.LogError("History Panel: IconSetSO not assigned.", this);
        if (contentArea == null) Debug.LogError("History Panel: Content Area RectTransform not assigned.", this);
        if (historyItemPrefab == null) Debug.LogError("History Panel: History Item Prefab not assigned.", this);
    }

    public void ShowPanel()
    {
        if (historyPanel == null || taskManager == null || contentArea == null || historyItemPrefab == null || iconSet == null)
        {
            Debug.LogError("Cannot show History Panel due to missing references.", this);
            return;
        }

        PopulateHistoryList();
        historyPanel.SetActive(true);
    }

    public void HidePanel()
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }
    }

    private void PopulateHistoryList()
    {
        // 1. Clear existing items
        foreach (GameObject item in instantiatedItems)
        {
            Destroy(item);
        }
        instantiatedItems.Clear();

        // 2. Get completed tasks from TaskManager
        List<TaskData> completedTasks = taskManager.GetCompletedTasks(maxHistoryItems);

        // 3. Instantiate new items
        if (completedTasks.Count == 0)
        {
            // Opcional: Mostrar un mensaje "No history yet"
            Debug.Log("Task History: No completed tasks found.");
            return;
        }

        foreach (TaskData taskData in completedTasks)
        {
            GameObject newItemGO = Instantiate(historyItemPrefab, contentArea);
            TaskHistoryItemUI itemUI = newItemGO.GetComponent<TaskHistoryItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(taskData, iconSet);
                instantiatedItems.Add(newItemGO);
            }
            else
            {
                Debug.LogError($"History Item Prefab is missing TaskHistoryItemUI script.", newItemGO);
                Destroy(newItemGO); // Clean up incorrect prefab instance
            }
        }

        // Opcional: Forzar actualización del layout si es necesario
        // LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea);
        // O ajustar el tamaño del contentArea si no usa VerticalLayoutGroup con ContentSizeFitter
    }

    // Opcional: Escuchar evento OnTaskCompleted de TaskManager para refrescar si el panel está visible
    // void OnEnable() { if(taskManager != null) taskManager.OnTaskCompleted += HandleTaskCompleted; }
    // void OnDisable() { if(taskManager != null) taskManager.OnTaskCompleted -= HandleTaskCompleted; }
    // private void HandleTaskCompleted(TaskData completedTask)
    // {
    //     if (historyPanel != null && historyPanel.activeSelf)
    //     {
    //         PopulateHistoryList(); // Refresh if visible
    //     }
    // }
}

// --- Summary Block ---
// ScriptRole: Manages the Task History UI Panel, displaying a list of completed tasks.
// RelatedScripts: TaskManager (gets data), TaskHistoryItemUI (instantiates), IconSetSO, FeedbackMessagesSO
// UsesSO: IconSetSO, FeedbackMessagesSO (optional for button text)
// ReceivesFrom: User (clicks button to show panel, clicks close button), TaskManager (gets data)
// SendsTo: TaskHistoryItemUI (Setup call) 