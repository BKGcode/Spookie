using System.Collections.Generic;
using UnityEngine;

public static class DwarfRegistry
{
    private static readonly Dictionary<DwarfController, Vector2Int> dwarfPositions = new Dictionary<DwarfController, Vector2Int>();

    public static void RegisterDwarf(DwarfController dwarf, Vector2Int position)
    {
        if (!dwarfPositions.ContainsKey(dwarf))
        {
            dwarfPositions.Add(dwarf, position);
            Debug.Log($"[DwarfRegistry] Registered {dwarf.name} at {position}");
        }
    }

    public static void UnregisterDwarf(DwarfController dwarf)
    {
        if (dwarfPositions.ContainsKey(dwarf))
        {
            dwarfPositions.Remove(dwarf);
            Debug.Log($"[DwarfRegistry] Unregistered {dwarf.name}");
        }
    }

    public static void UpdateDwarfPosition(DwarfController dwarf, Vector2Int newPosition)
    {
        if (dwarfPositions.ContainsKey(dwarf))
        {
            dwarfPositions[dwarf] = newPosition;
        }
        else
        {
            RegisterDwarf(dwarf, newPosition);
        }
    }

    public static bool IsTileOccupied(Vector2Int position)
    {
        return dwarfPositions.ContainsValue(position);
    }

    public static void ClearRegistry()
    {
        dwarfPositions.Clear();
        Debug.Log("[DwarfRegistry] Registry cleared.");
    }
}

// ScriptRole: Static utility to track the positions of all active dwarves.
// Dependencies: None
// NeedsSetup: None. It's a static class. 