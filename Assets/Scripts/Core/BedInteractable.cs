using UnityEngine;
using UnityEngine.Events;

public class BedInteractable : InteractableObject
{
    [Header("Bed Settings")]
    public Transform sleepPosition;
    public Transform wakePosition;
    public bool isSleeping = false;
    
    [Header("Bed Events")]
    public UnityEvent OnSleepStart;
    public UnityEvent OnSleepEnd;
    
    private DayNightCycle dayNightCycle;

    private void Start()
    {
        Debug.Log("BedInteractable: Initializing bed");
        
        // Find day/night cycle
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        
        if (dayNightCycle == null)
        {
            Debug.LogWarning("BedInteractable: DayNightCycle not found!");
        }
    }

    public override void Interact()
    {
        if (!CanInteract()) return;
        
        Debug.Log("BedInteractable: Player interacted with bed");
        
        // Check if it's time to sleep
        if (dayNightCycle != null && dayNightCycle.IsCycleComplete())
        {
            StartSleep();
        }
        else
        {
            // Show message that it's not time to sleep
            if (feedbackMessages != null)
            {
                string message = feedbackMessages.GetMessage("not_sleep_time_message");
                Debug.Log($"BedInteractable: {message}");
            }
        }
    }

    private void StartSleep()
    {
        if (isSleeping) return;
        
        Debug.Log("BedInteractable: Starting sleep sequence");
        isSleeping = true;
        
        // Move player to sleep position
        if (playerController != null && sleepPosition != null)
        {
            playerController.transform.position = sleepPosition.position;
            playerController.transform.rotation = sleepPosition.rotation;
        }
        
        // Disable player movement
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
        
        // Trigger sleep events
        OnSleepStart?.Invoke();
        
        // Start sleep in day/night cycle
        if (dayNightCycle != null)
        {
            dayNightCycle.ForceSleep();
        }
        
        // Play sleep sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("sleep_sound");
        }
    }

    protected override bool CanInteract()
    {
        if (!base.CanInteract()) return false;
        if (isSleeping) return false;
        if (dayNightCycle == null) return false;
        
        // Can only sleep when day cycle is complete
        return dayNightCycle.IsCycleComplete();
    }

    protected override void OnPlayerEnterRange()
    {
        Debug.Log("BedInteractable: Player entered bed range");
        
        if (dayNightCycle != null && dayNightCycle.IsCycleComplete())
        {
            // Show sleep prompt
            if (feedbackMessages != null)
            {
                string prompt = feedbackMessages.GetMessage("sleep_prompt");
                Debug.Log($"BedInteractable: {prompt}");
            }
        }
        else
        {
            // Show not time to sleep message
            if (feedbackMessages != null)
            {
                string message = feedbackMessages.GetMessage("bed_available_message");
                Debug.Log($"BedInteractable: {message}");
            }
        }
    }

    protected override void OnPlayerExitRange()
    {
        Debug.Log("BedInteractable: Player exited bed range");
    }

    public void WakeUp()
    {
        if (!isSleeping) return;
        
        Debug.Log("BedInteractable: Waking up");
        isSleeping = false;
        
        // Move player to wake position
        if (playerController != null && wakePosition != null)
        {
            playerController.transform.position = wakePosition.position;
            playerController.transform.rotation = wakePosition.rotation;
        }
        
        // Re-enable player movement
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }
        
        // Trigger wake events
        OnSleepEnd?.Invoke();
        
        // Play wake sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("wake_sound");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw sleep and wake positions
        if (sleepPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sleepPosition.position, 0.5f);
        }
        
        if (wakePosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wakePosition.position, 0.5f);
        }
        
        // Draw interaction range
        Gizmos.color = isInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}

// ScriptRole: Handles bed interaction and sleep mechanics
// Dependencies: InteractableObject, DayNightCycle
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: PlayerFPSController, DayNightCycle
// SendsTo: DayNightCycle, AudioManager via events 