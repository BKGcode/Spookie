using UnityEngine;
using Dwarf.FSM;

public class WaitingInCampState : IState
{
    private readonly DwarfStateManager _stateManager;
    private float _waitTimer;
    private readonly float _waitDuration;

    public WaitingInCampState(DwarfStateManager manager)
    {
        _stateManager = manager;
        _waitDuration = Random.Range(2f, 5f); // Wait for 2 to 5 seconds
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{_stateManager.DwarfController.DwarfData.DwarfName}' is waiting in camp for {_waitDuration:F1}s.");
        // If there's a status property to update, it would be done here.
    }

    public void OnUpdate()
    {
        _waitTimer += Time.deltaTime;
        if (_waitTimer >= _waitDuration)
        {
            _stateManager.ChangeState(new FindWanderPointState(_stateManager));
        }
    }

    public void OnExit()
    {
        // No cleanup needed for this simple state.
    }
}

// ScriptRole: Waits for a short duration in the camp before wandering again.
// Dependencies: DwarfStateManager
// NeedsSetup: None. 