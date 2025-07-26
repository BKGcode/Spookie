using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DwarfInspectorPanel_UI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image dwarfIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Button closeButton;

    [Header("Data")]
    [SerializeField] private FeedbackMessagesSO feedbackMessages;

    private DwarfController _currentlyInspectedDwarf;

    private void Awake()
    {
        panelRoot.SetActive(false);
        closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        GameEvents.OnDwarfSelected += HandleDwarfSelected;
        GameEvents.OnDeselectAllDwarfs += ClosePanel;
    }

    private void OnDisable()
    {
        GameEvents.OnDwarfSelected -= HandleDwarfSelected;
        GameEvents.OnDeselectAllDwarfs -= ClosePanel;
    }
    
    private void Update()
    {
        // In a real game, stamina would be part of a DwarfStats component.
        // For now, we'll just show a full bar as we removed direct state access.
        if (panelRoot.activeSelf && _currentlyInspectedDwarf != null)
        {
            staminaSlider.value = 1f; 
        }
    }

    private void HandleDwarfSelected(DwarfController selectedDwarf)
    {
        _currentlyInspectedDwarf = selectedDwarf;

        PopulateData(selectedDwarf);
        panelRoot.SetActive(true);
    }

    public void ClosePanel()
    {
        panelRoot.SetActive(false);
        _currentlyInspectedDwarf = null;
    }

    private void PopulateData(DwarfController dwarf)
    {
        var dwarfData = dwarf.DwarfData;
        
        // Icon logic may need to be adjusted based on how it's stored.
        // dwarfIcon.sprite = dwarfData.GetRandomIcon();
        
        nameText.text = dwarfData.DwarfName;
        
        // Status is now on the FSM, we can show the state name
        var stateManager = dwarf.GetComponent<DwarfStateManager>();
        if (stateManager != null && stateManager.CurrentState != null)
        {
            string statusKey = stateManager.CurrentState.GetType().Name;
            statusText.text = feedbackMessages.GetMessage(statusKey, statusKey);
        }

        staminaSlider.value = 1f; // Default to full for now
    }
}

// ScriptRole: Manages the UI panel that displays detailed information about a selected dwarf.
// Dependencies: DwarfStateManager
// HandlesEvents: GameEvents.OnDwarfSelected, GameEvents.OnDeselectAllDwarfs
// TriggersEvents: None
// UsesSO: FeedbackMessagesSO, DwarfDataSO (indirectly via DwarfController)
// NeedsSetup: Assign all UI components and create the panel prefab in the Canvas. 