using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    
    [Header("Data")]
    [SerializeField] private FeedbackMessagesSO feedbackMessages;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 4f;

    private Coroutine closeCoroutine;

    private void Awake()
    {
        notificationPanel.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnDwarfIsIdle += ShowIdleDwarfNotification;
    }

    private void OnDisable()
    {
        GameEvents.OnDwarfIsIdle -= ShowIdleDwarfNotification;
    }

    private void ShowIdleDwarfNotification(DwarfState dwarf)
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }

        string messageFormat = feedbackMessages.GetMessage("notification_dwarf_idle", "{0} is idle!");
        notificationText.text = string.Format(messageFormat, dwarf.DwarfName);
        
        notificationPanel.SetActive(true);
        closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        notificationPanel.SetActive(false);
    }
}

// ScriptRole: Displays temporary on-screen notifications to the player.
// Dependencies: None
// HandlesEvents: GameEvents.OnDwarfIsIdle
// TriggersEvents: None
// UsesSO: FeedbackMessagesSO
// NeedsSetup: Assign UI Panel/Text and FeedbackMessagesSO asset. Create the UI prefab. 