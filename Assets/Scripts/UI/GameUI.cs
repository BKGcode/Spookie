using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dayNightText;
    public TextMeshProUGUI interactionText;
    public TextMeshProUGUI pressureText;
    public TextMeshProUGUI timeText;
    
    [Header("References")]
    public DayNightCycle dayNightCycle;
    public PlayerFPSController playerController;
    public FeedbackMessagesSO feedbackMessages;
    
    [Header("UI Settings")]
    public float interactionTextFadeTime = 0.5f;
    public Color dayColor = Color.white;
    public Color nightColor = Color.blue;
    public Color pressureColor = Color.red;
    
    private float interactionTextAlpha = 0f;
    private string currentInteractionMessage = "";
    
    private void Start()
    {
        Debug.Log("GameUI: Initializing");
        
        // Find references if not assigned
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        }
        
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerFPSController>();
        }
        
        // Subscribe to player interaction events
        if (playerController != null)
        {
            playerController.OnPlayerInteract += OnPlayerInteract;
        }
        
        // Initialize UI
        UpdateUI();
    }
    
    private void Update()
    {
        UpdateUI();
        UpdateInteractionText();
    }
    
    private void UpdateUI()
    {
        if (dayNightCycle == null) return;
        
        // Update day/night text
        if (dayNightText != null)
        {
            if (dayNightCycle.IsDayTime())
            {
                dayNightText.text = "Day";
                dayNightText.color = dayColor;
            }
            else if (dayNightCycle.IsNightTime())
            {
                dayNightText.text = "Night";
                dayNightText.color = nightColor;
            }
            else if (dayNightCycle.IsSleeping())
            {
                dayNightText.text = "Sleeping...";
                dayNightText.color = Color.gray;
            }
        }
        
        // Update pressure text
        if (pressureText != null)
        {
            float pressure = dayNightCycle.GetSleepPressure();
            if (pressure > 0f)
            {
                pressureText.text = $"Sleep Pressure: {pressure:P0}";
                pressureText.color = Color.Lerp(dayColor, pressureColor, pressure);
                pressureText.gameObject.SetActive(true);
            }
            else
            {
                pressureText.gameObject.SetActive(false);
            }
        }
        
        // Update time text
        if (timeText != null)
        {
            float progress = dayNightCycle.GetDayProgress();
            int minutes = Mathf.FloorToInt(progress * (dayNightCycle.IsDayTime() ? 5f : 1f));
            int seconds = Mathf.FloorToInt((progress * (dayNightCycle.IsDayTime() ? 5f : 1f) - minutes) * 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    private void UpdateInteractionText()
    {
        if (interactionText == null) return;
        
        // Fade out interaction text
        if (interactionTextAlpha > 0f)
        {
            interactionTextAlpha -= Time.deltaTime / interactionTextFadeTime;
            interactionTextAlpha = Mathf.Max(0f, interactionTextAlpha);
            
            Color textColor = interactionText.color;
            textColor.a = interactionTextAlpha;
            interactionText.color = textColor;
            
            if (interactionTextAlpha <= 0f)
            {
                interactionText.gameObject.SetActive(false);
            }
        }
    }
    
    private void OnPlayerInteract()
    {
        // Show interaction text
        if (interactionText != null)
        {
            interactionText.text = currentInteractionMessage;
            interactionTextAlpha = 1f;
            interactionText.gameObject.SetActive(true);
            
            Color textColor = interactionText.color;
            textColor.a = interactionTextAlpha;
            interactionText.color = textColor;
        }
    }
    
    public void ShowInteractionMessage(string message)
    {
        currentInteractionMessage = message;
        
        if (interactionText != null)
        {
            interactionText.text = message;
            interactionTextAlpha = 1f;
            interactionText.gameObject.SetActive(true);
            
            Color textColor = interactionText.color;
            textColor.a = interactionTextAlpha;
            interactionText.color = textColor;
        }
    }
    
    public void ShowSleepPressureMessage()
    {
        if (feedbackMessages != null)
        {
            string message = feedbackMessages.GetMessage("sleep_pressure_message");
            ShowInteractionMessage(message);
        }
    }
    
    public void ShowBedMessage(bool canSleep)
    {
        if (feedbackMessages != null)
        {
            string messageKey = canSleep ? "sleep_prompt" : "not_sleep_time_message";
            string message = feedbackMessages.GetMessage(messageKey);
            ShowInteractionMessage(message);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerController != null)
        {
            playerController.OnPlayerInteract -= OnPlayerInteract;
        }
    }
}

// ScriptRole: Updates game UI with real-time information
// Dependencies: TextMeshProUGUI components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: DayNightCycle, PlayerFPSController
// SendsTo: None (UI updates only) 