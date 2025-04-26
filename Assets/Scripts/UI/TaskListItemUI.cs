using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; // Needed for UnityAction

public class TaskListItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;

    // Data for this item
    private string currentTaskId;
    private IconSetSO iconSet; // Needed to fetch the icon

    // Actions to invoke when buttons are clicked
    private UnityAction<string> onSelectAction;
    private UnityAction<string> onDeleteAction;

    void Start()
    {
        // Add listeners to the buttons
        selectButton.onClick.AddListener(HandleSelectClick);
        deleteButton.onClick.AddListener(HandleDeleteClick);
    }

    public void Setup(TaskData taskData, IconSetSO icons, UnityAction<string> selectAction, UnityAction<string> deleteAction)
    {
        currentTaskId = taskData.id;
        iconSet = icons;
        onSelectAction = selectAction;
        onDeleteAction = deleteAction;

        // Update UI elements
        titleText.text = taskData.title;

        if (iconSet != null && iconImage != null)
        {
            iconImage.sprite = iconSet.GetIconByIndex(taskData.iconIndex);
            iconImage.enabled = (iconImage.sprite != null); // Show/hide if icon is valid
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false; // Hide if no icon set or image component
        }
    }

    private void HandleSelectClick()
    {
        // Invoke the action provided by TaskListUI, passing our task ID
        onSelectAction?.Invoke(currentTaskId);
    }

    private void HandleDeleteClick()
    {
        // Invoke the action provided by TaskListUI, passing our task ID
        onDeleteAction?.Invoke(currentTaskId);
    }

    // Optional: Method to clean up when the item is destroyed/recycled
    // void OnDestroy()
    // {
    //     selectButton.onClick.RemoveListener(HandleSelectClick);
    //     deleteButton.onClick.RemoveListener(HandleDeleteClick);
    // }
}

// --- Summary Block ---
// ScriptRole: Represents a single task item in the pending/stopped list (left panel). Displays icon, title, and handles select/delete button clicks.
// RelatedScripts: TaskListUI (instantiates and sets this up), TaskManager (indirectly called via actions), IconSetSO (used to get icon sprite)
// UsesSO: IconSetSO
// ReceivesFrom: TaskListUI (Setup call), User (button clicks)
// SendsTo: TaskListUI (via onSelectAction, onDeleteAction UnityActions) 