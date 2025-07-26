using UnityEngine;

public class DwarfMiningState : DwarfBaseState
{
    private float miningTimer;
    private const float MINING_INTERVAL = 1f;
    private Vector2Int? currentTargetTile;

    public DwarfMiningState(DwarfStateManager context) : base(context) { }

    public override void EnterState()
    {
        Debug.Log("Dwarf entered Mining state.");
        miningTimer = MINING_INTERVAL;
        FindTargetTile();
    }

    public override void UpdateState()
    {
        if (currentTargetTile == null)
        {
            Debug.Log("No target tile to mine. Returning to Idle.");
            context.TransitionToState(context.IdleState);
            return;
        }

        miningTimer -= Time.deltaTime;
        if (miningTimer <= 0)
        {
            MineTile();
            miningTimer = MINING_INTERVAL;
        }

        context.Stats.ConsumeStamina(Time.deltaTime);
        if (context.Stats.CurrentStamina <= 0)
        {
            Debug.Log("Dwarf is out of stamina. Returning to Idle (for now).");
            // Later, this would transition to a "Resting" state.
            context.TransitionToState(context.IdleState);
        }
    }

    public override void ExitState()
    {
        Debug.Log("Dwarf exited Mining state.");
    }

    private void FindTargetTile()
    {
        // Simple logic: find an adjacent, minable tile.
        // This is a placeholder for a more complex target-finding logic.
        Vector2Int dwarfPos = Vector2Int.RoundToInt(context.transform.position);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // We need a reference to the MapDataManager to check tiles.
        // For now, let's assume the target is passed in or known.
        // This part needs to be connected to the assignment logic.
        // Let's assume the DwarfStateManager holds the target.
        currentTargetTile = context.TargetTile;
    }

    private void MineTile()
    {
        if (currentTargetTile.HasValue)
        {
            var mapDataManager = Object.FindObjectOfType<MapDataManager>(); // Temporary solution
            if (mapDataManager != null)
            {
                mapDataManager.ApplyDamage(currentTargetTile.Value, context.Stats.BaseStats.miningPower);
                
                // If tile is destroyed, find a new one or go idle.
                if (mapDataManager.GetTileStateAt(currentTargetTile.Value) == null)
                {
                    currentTargetTile = null; 
                }
            }
        }
    }
}

// ScriptRole: Manages the dwarf's behavior while actively mining a tile. 