using UnityEngine;
public class SummaryState : IGameState
{
    private readonly GameStateManager _manager;

    public SummaryState(GameStateManager manager) => _manager = manager;

    public void OnEnter()
    {
        Debug.Log("[SummaryState] Enter");
        // Show session summary, rewards, etc.
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        Debug.Log("[SummaryState] Exit");
        // Close summary, prepare for next config
    }
}
