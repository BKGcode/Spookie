using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        public static PathfindingGrid Instance { get; private set; }

        private PathNode[,,] _grid;
        private int _gridSizeX, _gridSizeY, _gridSizeZ;
        
        private const float NodeRadius = 0.5f;
        private float _nodeDiameter;
        private Vector3 _worldBottomLeft;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void InitializeGrid(TerrainData terrainData)
        {
            _nodeDiameter = NodeRadius * 2;
            
            // Assuming block size is 1x1x1
            Vector3 worldSize = new Vector3(terrainData.Width, 20, terrainData.Height); // Using Height for Z, and a fixed vertical size for now
            
            _gridSizeX = Mathf.RoundToInt(worldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(worldSize.y / _nodeDiameter);
            _gridSizeZ = Mathf.RoundToInt(worldSize.z / _nodeDiameter);
            
            CreateGrid(worldSize);
        }

        private void CreateGrid(Vector3 worldSize)
        {
            _grid = new PathNode[_gridSizeX, _gridSizeY, _gridSizeZ];
            _worldBottomLeft = transform.position - Vector3.right * worldSize.x / 2 - Vector3.up * worldSize.y / 2 - Vector3.forward * worldSize.z / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    for (int z = 0; z < _gridSizeZ; z++)
                    {
                        Vector3 worldPoint = _worldBottomLeft + Vector3.right * (x * _nodeDiameter + NodeRadius) + Vector3.up * (y * _nodeDiameter + NodeRadius) + Vector3.forward * (z * _nodeDiameter + NodeRadius);
                        
                        // Let's check for colliders at this position to determine walkability
                        bool isWalkable = !Physics.CheckSphere(worldPoint, NodeRadius);
                        _grid[x, y, z] = new PathNode(x, y, z, worldPoint, isWalkable);
                    }
                }
            }
            Debug.Log($"Pathfinding grid created. Size: {_gridSizeX}x{_gridSizeY}x{_gridSizeZ}");
        }

        public PathNode WorldPointToNode(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x - transform.position.x + (_gridSizeX * NodeRadius)) / (_gridSizeX * _nodeDiameter);
            float percentY = (worldPosition.y - transform.position.y + (_gridSizeY * NodeRadius)) / (_gridSizeY * _nodeDiameter);
            float percentZ = (worldPosition.z - transform.position.z + (_gridSizeZ * NodeRadius)) / (_gridSizeZ * _nodeDiameter);

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);
            percentZ = Mathf.Clamp01(percentZ);

            int x = Mathf.FloorToInt((_gridSizeX) * percentX);
            int y = Mathf.FloorToInt((_gridSizeY) * percentY);
            int z = Mathf.FloorToInt((_gridSizeZ) * percentZ);
            
            // Boundary check
            x = Mathf.Clamp(x, 0, _gridSizeX - 1);
            y = Mathf.Clamp(y, 0, _gridSizeY - 1);
            z = Mathf.Clamp(z, 0, _gridSizeZ - 1);

            return _grid[x, y, z];
        }

        public void UpdateNodeWalkability(Vector3 worldPosition, bool isWalkable)
        {
            PathNode node = WorldPointToNode(worldPosition);
            if (node != null)
            {
                node.isWalkable = isWalkable;
            }
        }
        
        public List<PathNode> GetNeighbours(PathNode node)
        {
            List<PathNode> neighbours = new List<PathNode>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 && z == 0) continue;

                        int checkX = node.gridX + x;
                        int checkY = node.gridY + y;
                        int checkZ = node.gridZ + z;

                        if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY && checkZ >= 0 && checkZ < _gridSizeZ)
                        {
                            neighbours.Add(_grid[checkX, checkY, checkZ]);
                        }
                    }
                }
            }
            return neighbours;
        }
    }
}

// ScriptRole: Manages the 3D grid of PathNodes representing the walkable world.
// Dependencies: None.
// NeedsSetup: Attach to a single GameObject in the scene (like TimeManager). Define the 'worldSize' to match your playable area. 