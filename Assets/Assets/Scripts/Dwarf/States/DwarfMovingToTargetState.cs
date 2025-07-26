using UnityEngine;

public class DwarfMovingToTargetState : DwarfBaseState
{
    private Vector2Int targetTilePosition;

    public DwarfMovingToTargetState(DwarfStateManager context, Vector2Int targetPosition) : base(context)
    {
        this.targetTilePosition = targetPosition;
    }

    public override void EnterState()
    {
        Debug.Log($"Dwarf moving to target tile: {targetTilePosition}");
        Vector3 worldPosition = context.Movement.transform.position; // This needs to be improved
        // For now, we move to the center of the tile. A proper implementation would find an adjacent spot.
        worldPosition = new Vector3(targetTilePosition.x + 0.5f, targetTilePosition.y + 0.5f, 0);

        context.Movement.MoveTo(worldPosition);
        context.Movement.OnTargetReached += OnTargetReached;
    }

    public override void UpdateState()
    {
        // Movement is handled by DwarfMovement, so nothing to do here.
    }

    public override void ExitState()
    {
        context.Movement.OnTargetReached -= OnTargetReached;
    }

    private void OnTargetReached()
    {
        // Once the target is reached, transition to the Mining state.
        context.TransitionToState(context.MiningState);
    }
}

// ScriptRole: Manages the dwarf's behavior while moving to a specific target tile. 