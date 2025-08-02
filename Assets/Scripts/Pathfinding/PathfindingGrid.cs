using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        public static PathfindingGrid Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private LayerMask unwalkableMask;
        [SerializeField] private bool showDebugVisuals;

        private PathNode[,] _grid;
        private int _gridSizeX, _gridSizeZ;
        
        private const float NodeRadius = 0.5f;
        private float _nodeDiameter;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple PathfindingGrid instances found. Destroying duplicate on {gameObject.name}");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeGrid(int width, int height, HashSet<Vector2Int> rockPositions)
        {
            _nodeDiameter = NodeRadius * 2;
            
            // Convertir dimensiones del mundo a tamaño de grid
            _gridSizeX = Mathf.RoundToInt(width / _nodeDiameter);
            _gridSizeZ = Mathf.RoundToInt(height / _nodeDiameter);
            
            CreateGrid(rockPositions);
            Debug.Log($"PathfindingGrid initialized with dimensions: {_gridSizeX}x{_gridSizeZ}");
        }

        private void CreateGrid(HashSet<Vector2Int> rockPositions)
        {
            _grid = new PathNode[_gridSizeX, _gridSizeZ];

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int z = 0; z < _gridSizeZ; z++)
                {
                    Vector3 worldPosition = new Vector3(x * _nodeDiameter, 0, z * _nodeDiameter);
                    Vector3 collisionCheckPoint = worldPosition + Vector3.up * 0.5f;
                    
                    bool hasObstacle = Physics.CheckSphere(collisionCheckPoint, NodeRadius, unwalkableMask);
                    bool hasRock = rockPositions.Contains(new Vector2Int(x, z));
                    bool isWalkable = !hasObstacle && !hasRock;

                    _grid[x, z] = new PathNode(isWalkable, worldPosition, x, z);
                }
            }
        }

        public PathNode WorldPointToNode(Vector3 worldPosition)
        {
            float percentX = worldPosition.x / (_gridSizeX * _nodeDiameter);
            float percentZ = worldPosition.z / (_gridSizeZ * _nodeDiameter);
            
            percentX = Mathf.Clamp01(percentX);
            percentZ = Mathf.Clamp01(percentZ);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int z = Mathf.RoundToInt((_gridSizeZ - 1) * percentZ);

            // CRÍTICO: Validar que las coordenadas estén dentro del rango
            if (x < 0 || x >= _gridSizeX || z < 0 || z >= _gridSizeZ)
            {
                Debug.LogWarning($"PathfindingGrid: World position {worldPosition} maps to invalid grid coordinates ({x}, {z}). Grid size: {_gridSizeX}x{_gridSizeZ}");
                return null;
            }

            var node = _grid[x, z];
            Debug.Log($"PathfindingGrid: World position {worldPosition} maps to grid node ({x}, {z}) - Walkable: {node.walkable}");
            return node;
        }

        public void UpdateNodeWalkability(Vector3 worldPosition, bool isWalkable)
        {
            PathNode node = WorldPointToNode(worldPosition);
            if (node != null)
            {
                node.UpdateWalkable(isWalkable);
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
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < _gridSizeX && checkZ >= 0 && checkZ < _gridSizeZ)
                    {
                        neighbours.Add(_grid[checkX, checkZ]);
                    }
                }
            }
            return neighbours;
        }

        public PathNode FindNearestWalkableNode(Vector3 worldPosition)
        {
            PathNode targetNode = WorldPointToNode(worldPosition);
            if (targetNode != null && targetNode.walkable)
            {
                return targetNode;
            }

            // Si el nodo de destino no es walkable, buscar el más cercano
            Debug.Log($"PathfindingGrid: Target at {worldPosition} is not walkable, searching for nearest walkable node");
            
            // Obtener las coordenadas de grid del punto objetivo usando WorldPointToNode
            PathNode centerNode = WorldPointToNode(worldPosition);
            if (centerNode == null) return null;
            
            int centerX = centerNode.gridX;
            int centerZ = centerNode.gridZ;
            
            int searchRadius = 1;
            int maxSearchRadius = 15; // Aumentado para mayor cobertura
            
            while (searchRadius <= maxSearchRadius)
            {
                // Buscar en espiral desde el centro hacia afuera
                for (int x = centerX - searchRadius; x <= centerX + searchRadius; x++)
                {
                    for (int z = centerZ - searchRadius; z <= centerZ + searchRadius; z++)
                    {
                        // Solo verificar los bordes del radio actual
                        if (x == centerX - searchRadius || x == centerX + searchRadius || 
                            z == centerZ - searchRadius || z == centerZ + searchRadius)
                        {
                            // Verificar que las coordenadas estén dentro de los límites
                            if (x >= 0 && x < _gridSizeX && z >= 0 && z < _gridSizeZ)
                            {
                                PathNode searchNode = _grid[x, z];
                                if (searchNode.walkable)
                                {
                                    Debug.Log($"PathfindingGrid: Found nearest walkable node at grid ({x}, {z}) world {searchNode.worldPosition} (distance: {searchRadius})");
                                    return searchNode;
                                }
                            }
                        }
                    }
                }
                searchRadius++;
            }
            
            Debug.LogWarning($"PathfindingGrid: No walkable node found within {maxSearchRadius} radius of {worldPosition}");
            return null;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugVisuals || _grid == null) return;

            foreach (PathNode node in _grid)
            {
                Gizmos.color = node.walkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(node.worldPosition + Vector3.up * 0.5f, Vector3.one * (_nodeDiameter - 0.1f));
            }
        }
    }
}

// ScriptRole: Manages the 2D grid of PathNodes representing the walkable world.
// Dependencies: Requires a LayerMask for unwalkable objects.
// NeedsSetup: Attach to a GameObject in the scene, configure unwalkableMask in inspector.
// RelatedScripts: PathNode, TerrainManager
