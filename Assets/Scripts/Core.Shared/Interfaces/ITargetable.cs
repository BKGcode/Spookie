using UnityEngine;

namespace Core.Shared
{
    /// <summary>
    /// Interface for objects that can be targeted by dwarfs
    /// </summary>
    public interface ITargetable
    {
        string name { get; }
        bool IsOccupied { get; }
        GameObject OccupiedBy { get; }
        bool IsOccupiedBy(GameObject occupier);
        void SetOccupancy(bool isOccupied, GameObject occupier = null);
        Transform transform { get; }
    }
}

// ScriptRole: Interface for objects that can be targeted by dwarfs
// Dependencies: None
// NeedsSetup: None - Interface definition