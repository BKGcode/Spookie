using UnityEngine;
public class PausedState : IGameState
{
    private readonly GameStateManager _manager;

    public PausedState(GameStateManager manager) => _manager = manager;

    public void OnEnter()
    {
        Debug.Log("[PausedState] Enter");
        // Pause TimerSystem, show Pause UI
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        Debug.Log("[PausedState] Exit");
        // Resume or handle resume via state change
    }
}
