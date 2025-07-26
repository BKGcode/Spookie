using UnityEngine;
using Dwarf.FSM;

[RequireComponent(typeof(DwarfController), typeof(DwarfStateManager))]
public class DwarfSupervisor : MonoBehaviour
{
    private DwarfStateManager _stateManager;
    private bool _hasPlayerOrder;

    private void Awake()
    {
        _stateManager = GetComponent<DwarfStateManager>();
    }
    
    private void OnEnable()
    {
        GameEvents.OnMineTile += HandlePlayerOrder;
        GameEvents.OnNightStart += GoToSleepIfNeeded;
    }

    private void OnDisable()
    {
        GameEvents.OnMineTile -= HandlePlayerOrder;
        GameEvents.OnNightStart -= GoToSleepIfNeeded;
    }

    private void GoToSleepIfNeeded()
    {
        // Don't interrupt player orders
        if (_hasPlayerOrder) return;
        
        // If already sleeping, do nothing
        if (_stateManager.CurrentState is SleepingState) return;

        Debug.Log($"Supervisor orders {gameObject.name} to sleep.");
        _stateManager.ChangeState(new SleepingState(_stateManager));
    }

    private void HandlePlayerOrder(Vector3 targetPosition)
    {
        // If this dwarf is selected, it has a player order.
        if (_stateManager.DwarfController.IsSelected)
        {
            _hasPlayerOrder = true;
        }
    }
    
    public void ReportTaskCompleted()
    {
        // Called by states when a task (like mining) is fully completed.
        _hasPlayerOrder = false;
    }
}

// ScriptRole: The high-level "Supervisor" brain for a dwarf, mainly for handling sleep cycles.
// Dependencies: DwarfController, DwarfStateManager
// HandlesEvents: GameEvents.OnMineTile, GameEvents.OnNightStart
// NeedsSetup: Attach to the dwarf prefab. 