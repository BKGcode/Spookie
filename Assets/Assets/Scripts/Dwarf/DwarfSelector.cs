using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DwarfSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float holdDuration = 0.5f;
    private Coroutine holdCoroutine;
    private DwarfStats dwarfStats;

    private void Awake()
    {
        dwarfStats = GetComponent<DwarfStats>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        holdCoroutine = StartCoroutine(HoldTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
    }

    private IEnumerator HoldTimer()
    {
        yield return new WaitForSeconds(holdDuration);
        
        Debug.Log($"Dwarf '{dwarfStats.dwarfName}' selected for inspection via hold.");
        GameEvents.ReportDwarfSelected(dwarfStats);
    }
}

// ScriptRole: Detects a 'click and hold' on a dwarf to trigger an inspection event.
// Dependencies: DwarfStats, Collider2D, and an EventSystem in the scene.
// HandlesEvents: None
// TriggersEvents: GameEvents.OnDwarfSelected
// UsesSO: None
// NeedsSetup: Must be on the Dwarf prefab. The dwarf needs a Collider2D. 