// ConfiguringState.cs
using UnityEngine;

public class ConfiguringState : IGameState
{
    private readonly GameStateManager _manager;

    public ConfiguringState(GameStateManager manager) => _manager = manager;

    public void OnEnter()
    {
        Debug.Log("[ConfiguringState] Enter");
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        Debug.Log("[ConfiguringState] Exit");
    }
}
