using UnityEngine;
using System.Collections.Generic;

namespace Terrain
{
    [System.Serializable]
    public class TerrainData
    {
        private TerrainTile[,] tileGrid;
        private Dictionary<Vector2Int, TerrainTile> modifiedTiles;
        private int width;
        private int height;
        private int mapSeed;

        public int Width => width;
        public int Height => height;
        public int MapSeed => mapSeed;
        
        public List<Vector2Int> DwarfSpawnPoints { get; private set; }
        public List<Vector2Int> BedSpawnPoints { get; private set; }

        public TerrainData(int width, int height, int seed)
        {
            this.width = width;
            this.height = height;
            this.mapSeed = seed;
            this.tileGrid = new TerrainTile[width, height];
            this.modifiedTiles = new Dictionary<Vector2Int, TerrainTile>();
            this.DwarfSpawnPoints = new List<Vector2Int>();
            this.BedSpawnPoints = new List<Vector2Int>();
        }
        
        public TerrainTile GetTile(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                Debug.LogError($"TerrainData: GetTile position ({x},{y}) is out of bounds.");
                return default;
            }
            return tileGrid[x, y];
        }

        public void SetTile(int x, int y, TerrainTile tile)
        {
            if (!IsValidPosition(x, y))
            {
                Debug.LogError($"TerrainData: SetTile position ({x},{y}) is out of bounds.");
                return;
            }
            tileGrid[x, y] = tile;
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public List<(int, int)> GetNeighbors(int x, int y)
        {
            var neighbors = new List<(int, int)>();
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (IsValidPosition(nx, ny))
                {
                    neighbors.Add((nx, ny));
                }
            }
            return neighbors;
        }
    }
}
