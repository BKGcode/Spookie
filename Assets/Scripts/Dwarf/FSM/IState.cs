using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}

// ScriptRole: Defines the contract for all states in the Finite State Machine.
// Dependencies: None
// NeedsSetup: None. It's an interface. 