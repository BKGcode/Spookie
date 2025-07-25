using System;
using UnityEngine;

public static class GameEvents
{
    // Called when a tile's health reaches zero and it's destroyed.
    public static event Action<Vector2Int> OnTileDestroyed;
    public static void ReportTileDestroyed(Vector2Int position) => OnTileDestroyed?.Invoke(position);
}

// ScriptRole: Central hub for all game-related C# events.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: OnTileDestroyed 