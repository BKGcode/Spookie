using UnityEngine;
using System;

public class MiningState : IState
{
    private readonly DwarfStateManager stateManager;
    private readonly Vector2Int targetPosition;
    private float miningTimer;

    public MiningState(DwarfStateManager manager, Vector2Int target)
    {
        stateManager = manager;
        targetPosition = target;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' has started mining at {targetPosition}.");
        stateManager.Controller.CurrentState.CurrentStatus = "Mining";
    }

    public void OnUpdate()
    {
        // Check for exit conditions first
        if (stateManager.Controller.CurrentState.CurrentStamina <= 0)
        {
            Debug.Log("Dwarf is exhausted. Going home to rest.");
            Action onArrivalAtCamp = () => stateManager.ChangeState(new SleepingState(stateManager));
            var goHomeState = new PathfindingToTargetState(stateManager, MapGenerator.CampfirePosition, onArrivalAtCamp, onArrivalAtCamp);
            stateManager.ChangeState(goHomeState);
            return;
        }

        // Apply damage on a timer
        miningTimer += Time.deltaTime;
        if (miningTimer >= 1f) // Mine once per second
        {
            miningTimer = 0f;
            
            var dwarfStats = stateManager.Controller.CurrentState.BaseStats;
            Pathfinding.Instance.mapGenerator.ApplyDamage(targetPosition, dwarfStats.miningPower);
            
            stateManager.Controller.CurrentState.CurrentStamina -= 5; // Example stamina cost
            Debug.Log($"Dwarf mined. Stamina: {stateManager.Controller.CurrentState.CurrentStamina}");

            // Check if the tile was destroyed after mining
            if (Pathfinding.Instance.mapGenerator.GetTileDataAt(targetPosition) == null)
            {
                Debug.Log($"Tile at {targetPosition} destroyed. Task complete.");
                stateManager.ChangeState(new IdleState(stateManager));
            }
        }
    }

    public void OnExit()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' has finished mining.");
    }
}

// ScriptRole: Handles the dwarf's behavior while mining a tile.
// Dependencies: DwarfStateManager, MapGenerator (via Pathfinding)
// NeedsSetup: None. 