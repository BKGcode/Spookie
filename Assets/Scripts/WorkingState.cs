using UnityEngine;
public class WorkingState : IGameState
{
    private readonly GameStateManager _manager;

    public WorkingState(GameStateManager manager) => _manager = manager;

    public void OnEnter()
    {
        Debug.Log("[WorkingState] Enter");
        // Trigger TimerSystem to start work timer
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        Debug.Log("[WorkingState] Exit");
        // Cleanup or pause timer
    }
}
