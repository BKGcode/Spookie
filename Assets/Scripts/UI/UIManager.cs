using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private Slider clockSlider;
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private Image cycleIcon;
    [SerializeField] private Sprite dayIcon;
    [SerializeField] private Sprite nightIcon;

    private void OnEnable()
    {
        GameEvents.OnTimeUpdated += UpdateClockUI;
        GameEvents.OnDayStart += SetDayIcon;
        GameEvents.OnNightStart += SetNightIcon;
    }

    private void OnDisable()
    {
        GameEvents.OnTimeUpdated -= UpdateClockUI;
        GameEvents.OnDayStart -= SetDayIcon;
        GameEvents.OnNightStart -= SetNightIcon;
    }

    private void UpdateClockUI(float currentTime, float maxTime)
    {
        if (clockSlider != null)
        {
            clockSlider.value = currentTime / maxTime;
        }

        if (clockText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            clockText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void SetDayIcon()
    {
        if (cycleIcon != null)
        {
            cycleIcon.sprite = dayIcon;
        }
    }

    private void SetNightIcon()
    {
        if (cycleIcon != null)
        {
            cycleIcon.sprite = nightIcon;
        }
    }
}

// ScriptRole: Manages all UI elements in the game.
// Dependencies: None
// HandlesEvents: GameEvents.OnTimeUpdated, GameEvents.OnDayStart, GameEvents.OnNightStart
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Assign the UI elements (Slider, Text, Image) and icons in the Inspector. 