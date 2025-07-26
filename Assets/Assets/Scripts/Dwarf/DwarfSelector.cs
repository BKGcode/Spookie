using UnityEngine;
using System.Collections;

public class DwarfSelector : MonoBehaviour
{
    [SerializeField] private float holdDuration = 0.5f;
    private Coroutine holdCoroutine;
    private DwarfStats dwarfStats;

    private void Awake()
    {
        dwarfStats = GetComponent<DwarfStats>();
    }

    private void OnMouseDown()
    {
        holdCoroutine = StartCoroutine(HoldTimer());
    }

    private void OnMouseUp()
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
// Dependencies: DwarfStats, Collider2D, and a Physics2DRaycaster on the Camera.
// HandlesEvents: None
// TriggersEvents: GameEvents.OnDwarfSelected
// UsesSO: None
// NeedsSetup: Must be on the Dwarf prefab. The dwarf needs a Collider2D. 