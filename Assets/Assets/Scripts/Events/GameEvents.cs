using System;
using UnityEngine;

public static class GameEvents
{
    // Called when a tile's health reaches zero and it's destroyed.
    public static event Action<Vector2Int> OnTileDestroyed;
    public static void ReportTileDestroyed(Vector2Int position) => OnTileDestroyed?.Invoke(position);

    // Called by the InputManager when the player assigns a dwarf to a target tile.
    public static event Action<DwarfStateManager, Vector2Int> OnDwarfAssigned;
    public static void ReportDwarfAssigned(DwarfStateManager dwarf, Vector2Int targetPosition) => OnDwarfAssigned?.Invoke(dwarf, targetPosition);
}

// ScriptRole: Central hub for all game-related C# events.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: OnTileDestroyed 