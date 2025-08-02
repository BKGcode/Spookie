using UnityEngine;
using UnityEngine.Events;
using SO;

namespace Terrain
{
    /// <summary>
    /// Contains data for the OnTileMined event.
    /// </summary>
    [System.Serializable]
    public struct TileMinedEventData
    {
        public int x;
        public int y;
        public MaterialSO material;
        public bool wasCompletelyMined;
    }

    /// <summary>
    /// Static class holding all public events related to the terrain system.
    /// </summary>
    public static class TerrainEvents
    {
        [System.Serializable] public class TileMinedEvent : UnityEvent<TileMinedEventData> { }

        /// <summary>
        /// Fired when the terrain generation process is complete.
        /// Passes the generated TerrainData.
        /// </summary>
        public static UnityEvent<TerrainData> OnTerrainGenerated = new UnityEvent<TerrainData>();

        /// <summary>
        /// Fired when a tile is successfully mined.
        /// Passes data about the mined tile.
        /// </summary>
        public static TileMinedEvent OnTileMined = new TileMinedEvent();
        
        /// <summary>
        /// Fired when a new material is revealed by mining.
        /// Passes the MaterialSO of the revealed material.
        /// </summary>
        public static UnityEvent<MaterialSO> OnMaterialRevealed = new UnityEvent<MaterialSO>();
    }
}


// ScriptRole: Defines all public UnityEvents for the terrain system and their associated data structures.
// Dependencies: MaterialSO
// HandlesEvents: None
// TriggersEvents: This class is a container for events triggered by other systems (e.g., TerrainManager).
// UsesSO: MaterialSO
// NeedsSetup: None. This is a static class for event definitions.