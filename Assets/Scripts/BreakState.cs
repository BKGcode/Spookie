using UnityEngine;
public class BreakState : IGameState
{
    private readonly GameStateManager _manager;

    public BreakState(GameStateManager manager) => _manager = manager;

    public void OnEnter()
    {
        Debug.Log("[BreakState] Enter");
        // Trigger TimerSystem to start break timer
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        Debug.Log("[BreakState] Exit");
        // Cleanup if required
    }
}
