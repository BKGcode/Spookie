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
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Button closeButton;

    [Header("Data")]
    [SerializeField] private FeedbackMessagesSO feedbackMessages;

    [Header("Settings")]
    [SerializeField] private float autoCloseDelay = 5f;

    private Coroutine closeCoroutine;
    private DwarfState currentlyInspectedDwarf;

    private void Awake()
    {
        panelRoot.SetActive(false);
        closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        GameEvents.OnDwarfSelected += HandleDwarfSelected;
    }

    private void OnDisable()
    {
        GameEvents.OnDwarfSelected -= HandleDwarfSelected;
    }
    
    private void Update()
    {
        // Real-time update for stamina if the panel is visible
        if (panelRoot.activeSelf && currentlyInspectedDwarf != null)
        {
            staminaSlider.value = currentlyInspectedDwarf.CurrentStamina / currentlyInspectedDwarf.BaseStats.maxStamina;
        }
    }

    private void HandleDwarfSelected(DwarfState selectedDwarf)
    {
        currentlyInspectedDwarf = selectedDwarf;

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }

        PopulateData(selectedDwarf);
        panelRoot.SetActive(true);
        closeCoroutine = StartCoroutine(CloseAfterDelay(autoCloseDelay));
    }

    public void ClosePanel()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }
        panelRoot.SetActive(false);
        currentlyInspectedDwarf = null;
    }

    private void PopulateData(DwarfState stats)
    {
        dwarfIcon.sprite = stats.DwarfIcon;
        nameText.text = stats.DwarfName;
        
        string ageLabel = feedbackMessages.GetMessage("label_age", "Age: {0}");
        ageText.text = string.Format(ageLabel, stats.Age);

        string statusLabel = feedbackMessages.GetMessage("label_status", "Status: {0}");
        statusText.text = string.Format(statusLabel, feedbackMessages.GetMessage(stats.CurrentStatus, stats.CurrentStatus));

        staminaSlider.value = stats.CurrentStamina / stats.BaseStats.maxStamina;
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClosePanel();
    }
}

// ScriptRole: Manages the UI panel that displays detailed information about a selected dwarf.
// Dependencies: None
// HandlesEvents: GameEvents.OnDwarfSelected
// TriggersEvents: None
// UsesSO: DwarfDataSO (indirectly via DwarfState)
// NeedsSetup: Assign all UI components and create the panel prefab in the Canvas. 