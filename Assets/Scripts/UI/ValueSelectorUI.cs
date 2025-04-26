using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; // For UnityEvent

public class ValueSelectorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;

    [Header("Configuration")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 59;
    [SerializeField] private int stepValue = 1;
    [SerializeField] private int defaultValue = 0;
    [SerializeField] private bool wrapAround = true; // Wrap from max to min and vice-versa
    [SerializeField] private string numberFormat = "D2"; // "D2" for 00, 01, etc. "D" for 0, 1...

    [Space]
    [Tooltip("Optional event called when the value changes.")]
    public UnityEvent<int> OnValueChanged;

    private int _currentValue;
    public int CurrentValue
    {
        get => _currentValue;
        private set
        {
            int previousValue = _currentValue;
            // Clamp or wrap the value
            if (value < minValue)
            {
                _currentValue = wrapAround ? maxValue : minValue;
            }
            else if (value > maxValue)
            {
                _currentValue = wrapAround ? minValue : maxValue;
            }
            else
            {
                _currentValue = value;
            }

            // Ensure value aligns with step if necessary (might be complex, skip for now if step=1)
            // For steps > 1, clamping/wrapping needs careful consideration.
            // Let's assume step = 1 or 5 for minutes for now. If step=5, values should be 0, 5, 10...

            UpdateValueText();

            // Invoke event only if the value actually changed
            if (_currentValue != previousValue)
            {
                OnValueChanged?.Invoke(_currentValue);
            }
        }
    }

    void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        SetupButtonListeners();
        ResetValue(); // Set initial value
    }

    private bool ValidateReferences()
    {
        if (valueText == null) { Debug.LogError($"[{gameObject.name}] Value Text not assigned!", this); return false; }
        if (incrementButton == null) { Debug.LogError($"[{gameObject.name}] Increment Button not assigned!", this); return false; }
        if (decrementButton == null) { Debug.LogError($"[{gameObject.name}] Decrement Button not assigned!", this); return false; }
        return true;
    }

    private void SetupButtonListeners()
    {
        incrementButton.onClick.AddListener(Increment);
        decrementButton.onClick.AddListener(Decrement);
    }

    private void Increment()
    {
        CurrentValue += stepValue;
    }

    private void Decrement()
    {
        CurrentValue -= stepValue;
    }

    public void ResetValue()
    {
        CurrentValue = defaultValue;
        UpdateValueText(); // Ensure text is updated immediately on reset
    }

    private void UpdateValueText()
    {
        if (valueText != null)
        {
            valueText.text = _currentValue.ToString(numberFormat);
        }
    }
}

// --- Summary Block ---
// ScriptRole: Manages a simple UI component for selecting a numerical value using increment/decrement buttons.
// RelatedScripts: AddTaskPanel (uses this to select hours/minutes)
// UsesSO: None
// ReceivesFrom: User (button clicks)
// SendsTo: Other scripts via OnValueChanged event (optional) 