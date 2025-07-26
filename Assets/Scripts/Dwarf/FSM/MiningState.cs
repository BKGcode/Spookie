using UnityEngine;
using System;

public class MiningState : IState
{
    private readonly DwarfStateManager stateManager;
    private readonly Vector2Int targetPosition;
    private readonly Action onTaskCompleted;
    private float miningTimer;

    public MiningState(DwarfStateManager manager, Vector2Int target, Action onTaskCompletedCallback = null)
    {
        stateManager = manager;
        targetPosition = target;
        onTaskCompleted = onTaskCompletedCallback;
    }

    public void OnEnter()
    {
        Debug.Log($"Dwarf '{stateManager.Controller.CurrentState.DwarfName}' has started mining at {targetPosition}.");
        stateManager.Controller.CurrentState.CurrentStatus = "Mining";
    }

    public void OnUpdate()
    {
        // Stamina check is now handled by the Supervisor's time-based logic.
        // The state's only job is to mine.

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
                Debug.Log($"Tile at {targetPosition} destroyed. Moving to occupy the space.");

                Action onArrivalAtMinedSpot = () => {
                    // AFTER moving, decide the next action based on the Supervisor's current orders.
                    stateManager.OnMacroBehaviorChanged(stateManager.Supervisor.CurrentMacroBehavior);
                };
                
                stateManager.ChangeState(new MoveToExactPositionState(stateManager, targetPosition, onArrivalAtMinedSpot));
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