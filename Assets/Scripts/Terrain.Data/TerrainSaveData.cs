using UnityEngine;
using System.Collections.Generic;

namespace Terrain.Data
{
    [System.Serializable]
    public class SavedTileData
    {
        public string MaterialGuid;
        public MiningState MiningState;
        public SpaceType SpecialProperty;
    }

    [System.Serializable]
    public class TerrainSaveData
    {
        public string GameVersion;
        public int MapSeed;
        public int Width;
        public int Height;
        public List<SavedTileData> AllTiles { get; private set; } = new List<SavedTileData>();
        public List<Vector2Int> DwarfSpawnPoints { get; private set; } = new List<Vector2Int>();
        public List<Vector2Int> BedSpawnPoints { get; private set; } = new List<Vector2Int>();

        public TerrainSaveData() {}

        public TerrainSaveData(int seed, int width, int height, string createdBy)
        {
            MapSeed = seed;
            Width = width;
            Height = height;
            GameVersion = createdBy;
            AllTiles = new List<SavedTileData>(width * height);
            DwarfSpawnPoints = new List<Vector2Int>();
            BedSpawnPoints = new List<Vector2Int>();
        }
    }
}
