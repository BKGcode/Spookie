using System;
using UnityEngine;

public static class GameEvents
{
    // Called when a tile's health reaches zero and it's destroyed.
    public static event Action<Vector2Int> OnTileDestroyed;
    public static void ReportTileDestroyed(Vector2Int position) => OnTileDestroyed?.Invoke(position);

    // Called by the InputManager when the player assigns a dwarf to a target tile.
    public static event Action<DwarfController, Vector2Int> OnDwarfAssigned;
    public static void ReportDwarfAssigned(DwarfController dwarf, Vector2Int targetPosition) => OnDwarfAssigned?.Invoke(dwarf, targetPosition);

    // Called by the InputManager when the player explicitly orders a dwarf to mine a tile.
    public static event Action<DwarfController, Vector2Int> OnMiningOrderGiven;
    public static void ReportMiningOrderGiven(DwarfController dwarf, Vector2Int targetPosition) => OnMiningOrderGiven?.Invoke(dwarf, targetPosition);

    // Called when the player clicks a tile to mine, without a specific dwarf. The selected dwarf should act.
    public static event Action<Vector3> OnMineTile;
    public static void ReportMineTile(Vector3 worldPosition) => OnMineTile?.Invoke(worldPosition);

    // Called by DwarfInitializer after all initial setup is complete.
    public static event Action OnGameReady;
    public static void ReportGameReady() => OnGameReady?.Invoke();
    
    // -- Time Events --
    public static event Action OnDayStart;
    public static void ReportDayStart() => OnDayStart?.Invoke();

    public static event Action OnNightStart;
    public static void ReportNightStart() => OnNightStart?.Invoke();

    // -- UI Events --
    public static event Action<float, float> OnTimeUpdated; // current time, max time
    public static void ReportTimeUpdated(float currentTime, float maxTime) => OnTimeUpdated?.Invoke(currentTime, maxTime);

    // -- Dwarf Interaction Events --
    public static event Action<DwarfController> OnDwarfSelected;
    public static void ReportDwarfSelected(DwarfController dwarf) => OnDwarfSelected?.Invoke(dwarf);

    public static event Action OnDeselectAllDwarfs;
    public static void ReportDeselectAllDwarfs() => OnDeselectAllDwarfs?.Invoke();

    public static event Action<DwarfState> OnDwarfIsIdle;
    public static void ReportDwarfIsIdle(DwarfState dwarfState) => OnDwarfIsIdle?.Invoke(dwarfState);
}

// ScriptRole: Central hub for all game-related C# events.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: OnTileDestroyed 