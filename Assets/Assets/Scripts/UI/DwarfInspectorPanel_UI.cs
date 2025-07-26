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

    [Header("Settings")]
    [SerializeField] private float autoCloseDelay = 5f;

    private Coroutine closeCoroutine;
    private DwarfStats currentlyInspectedDwarf;

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

    private void HandleDwarfSelected(DwarfStats selectedDwarf)
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

    private void PopulateData(DwarfStats stats)
    {
        dwarfIcon.sprite = stats.dwarfIcon;
        nameText.text = stats.dwarfName;
        ageText.text = $"Age: {stats.age}";
        statusText.text = $"Status: {stats.currentStatus}";
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
// UsesSO: DwarfDataSO (indirectly via DwarfStats)
// NeedsSetup: Assign all UI components and create the panel prefab in the Canvas. 