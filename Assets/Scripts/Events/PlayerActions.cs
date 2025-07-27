using System;
using UnityEngine;

/// <summary>
/// A static class to hold all player-initiated action events.
/// Other systems can subscribe to these events to react to player input.
/// </summary>
public static class PlayerActions
{
    /// <summary>
    /// Fired when the player attempts to mine a tile.
    /// The Vector2Int payload contains the world coordinates (x, y) of the targeted tile.
    /// </summary>
    public static event Action<Vector2Int> OnMineAttempt;

    /// <summary>
    /// A helper method to safely invoke the OnMineAttempt event.
    /// </summary>
    public static void TriggerMineAttempt(Vector2Int tileCoords)
    {
        Debug.Log($"PlayerActions: Triggering OnMineAttempt for tile at {tileCoords}.");
        OnMineAttempt?.Invoke(tileCoords);
    }
}

// ScriptRole: Defines global events related to direct player actions.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: OnMineAttempt
// UsesSO: None
// NeedsSetup: None. This is a static class, no instantiation needed. 