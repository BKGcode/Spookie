using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        public static PathfindingGrid Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private LayerMask unwalkableMask;

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
            
            // The grid is now effectively 2D, with a single layer centered at y=0.
            Vector3 worldSize = new Vector3(terrainData.Width, 1, terrainData.Height);
            
            _gridSizeX = Mathf.RoundToInt(worldSize.x / _nodeDiameter);
            _gridSizeY = 1; // Force a single vertical layer
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
                        // Check for obstacles at block-height (Y=0.5)
                        Vector3 collisionCheckPoint = new Vector3(x, 0.5f, z);
                        bool isWalkable = !Physics.CheckSphere(collisionCheckPoint, NodeRadius, unwalkableMask);

                        // But create the actual path node at floor-height (Y=0)
                        Vector3 nodeWorldPosition = new Vector3(x, 0, z);
                        
                        if (!isWalkable)
                        {
                            Debug.DrawRay(collisionCheckPoint, Vector3.up * 2, Color.red, 10f);
                        }
                        _grid[x, y, z] = new PathNode(x, y, z, nodeWorldPosition, isWalkable);
                    }
                }
            }
            Debug.Log($"Pathfinding grid created. Size: {_gridSizeX}x{_gridSizeY}x{_gridSizeZ}");
        }

        public PathNode WorldPointToNode(Vector3 worldPosition)
        {
            // We can now safely use integer coordinates as the grid matches the world.
            int x = Mathf.RoundToInt(worldPosition.x);
            int y = 0; // Always use the single layer
            int z = Mathf.RoundToInt(worldPosition.z);
            
            // Boundary check
            x = Mathf.Clamp(x, 0, _gridSizeX - 1);
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
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = 0; // Always on the same plane
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < _gridSizeX && checkZ >= 0 && checkZ < _gridSizeZ)
                    {
                        neighbours.Add(_grid[checkX, checkY, checkZ]);
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