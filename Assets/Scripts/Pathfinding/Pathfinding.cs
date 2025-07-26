using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }

    public MapGenerator mapGenerator { get; private set; }
    [SerializeField] private Tilemap foregroundTilemap;

    private PathNode[,] grid;
    private int gridWidth;
    private int gridHeight;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

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

        mapGenerator = FindObjectOfType<MapGenerator>();
        GameEvents.OnTileDestroyed += OnTileDestroyed;
    }

    private void OnDestroy()
    {
        GameEvents.OnTileDestroyed -= OnTileDestroyed;
    }

    public void CreateGrid()
    {
        gridWidth = mapGenerator.MapWidth;
        gridHeight = mapGenerator.MapHeight;
        grid = new PathNode[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = new PathNode(new Vector2Int(x, y));
                // A tile is NOT walkable if there is something in the foreground
                grid[x, y].isWalkable = foregroundTilemap.GetTile(new Vector3Int(x, y, 0)) == null;
            }
        }
        Debug.Log($"Pathfinding grid created. Size: {gridWidth}x{gridHeight}");
    }

    public void SetWalkable(Vector2Int position, bool isWalkable)
    {
        if (position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight)
        {
            grid[position.x, position.y].isWalkable = isWalkable;
            Debug.Log($"Pathfinding node at {position} set to walkable: {isWalkable}");
        }
    }

    public PathNode GetNode(Vector2Int position)
    {
        if (position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight)
        {
            return grid[position.x, position.y];
        }
        return null;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return (Vector2Int)foregroundTilemap.WorldToCell(worldPosition);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return foregroundTilemap.GetCellCenterWorld((Vector3Int)gridPosition);
    }

    private void OnTileDestroyed(Vector2Int position)
    {
        SetWalkable(position, true);
    }

    public Vector3Int? FindNearestWalkableNode(Vector3Int targetCell)
    {
        PathNode targetNode = GetNode(new Vector2Int(targetCell.x, targetCell.y));
        if (targetNode != null && targetNode.isWalkable)
        {
            return targetCell;
        }

        // Check neighbors
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int neighborPos = new Vector2Int(targetCell.x + x, targetCell.y + y);
                PathNode neighborNode = GetNode(neighborPos);
                if (neighborNode != null && neighborNode.isWalkable)
                {
                    return new Vector3Int(neighborNode.gridPosition.x, neighborNode.gridPosition.y, 0);
                }
            }
        }

        Debug.LogWarning($"Target ({targetCell.x}, {targetCell.y}) has no walkable neighbours.");
        return null;
    }
    
    public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        Vector2Int startPosition = WorldToGridPosition(startWorldPos);
        Vector2Int endPosition = WorldToGridPosition(endWorldPos);

        PathNode startNode = GetNode(startPosition);
        PathNode endNode = GetNode(endPosition);

        // If the target tile is not walkable, this should have been handled by the calling State
        if (endNode == null || !endNode.isWalkable)
        {
            Debug.LogWarning($"Target {endPosition} is not walkable. Pathfinding requires a walkable target.");
            return null;
        }

        Debug.Log($"[Pathfinding] Request: From {startPosition} To {endNode.gridPosition}. Is End Walkable? {endNode.isWalkable}");

        List<PathNode> openList = new List<PathNode> { startNode };
        HashSet<PathNode> closedList = new HashSet<PathNode>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x,y].gCost = int.MaxValue;
                grid[x,y].CalculateFCost();
                grid[x,y].parentNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return ReconstructPath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                
                // Treat occupied tiles as non-walkable for this path calculation
                if (!neighbourNode.isWalkable || DwarfRegistry.IsTileOccupied(neighbourNode.gridPosition)) 
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.parentNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // Path not found
        Debug.LogWarning($"Path not found from {startPosition} to {endPosition}");
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = currentNode.gridPosition.x + x;
                int checkY = currentNode.gridPosition.y + y;

                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    neighbourList.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbourList;
    }

    private List<Vector3> ReconstructPath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;
        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();

        List<Vector3> vectorPath = new List<Vector3>();
        foreach (var node in path)
        {
            vectorPath.Add(GridToWorldPosition(node.gridPosition));
        }

        return vectorPath;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int yDistance = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode FindNearestWalkableNeighbour(PathNode originalNode)
    {
        PathNode bestNeighbour = null;
        // Simple approach: return the very first valid neighbour we find.
        // A better approach might calculate distance, but this is fine for now.
        foreach (var neighbour in GetNeighbourList(originalNode))
        {
            if (neighbour.isWalkable && !DwarfRegistry.IsTileOccupied(neighbour.gridPosition))
            {
                bestNeighbour = neighbour;
                break; // Found a valid spot, no need to check further for this simple implementation.
            }
        }
        return bestNeighbour;
    }

    public Vector2Int? FindNearestWalkableTileOfType(Vector2Int startPosition, TileDataSO targetType)
    {
        // This is a simple, non-optimized search. For a large map, a more efficient algorithm would be needed.
        Queue<PathNode> searchQueue = new Queue<PathNode>();
        HashSet<PathNode> visitedNodes = new HashSet<PathNode>();
        
        searchQueue.Enqueue(GetNode(startPosition));
        visitedNodes.Add(GetNode(startPosition));

        while (searchQueue.Count > 0)
        {
            PathNode currentNode = searchQueue.Dequeue();

            // Check if this tile is of the target type
            TileDataSO tileData = mapGenerator.GetTileDataAt(currentNode.gridPosition);
            if (tileData == targetType)
            {
                return currentNode.gridPosition;
            }

            foreach (var neighbour in GetNeighbourList(currentNode))
            {
                if (!visitedNodes.Contains(neighbour))
                {
                    visitedNodes.Add(neighbour);
                    searchQueue.Enqueue(neighbour);
                }
            }
        }
        return null; // No tile of the target type found
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}


// ScriptRole: A Singleton service that calculates the optimal path between two points on the grid using A*.
// Dependencies: MapGenerator, a foreground Tilemap.
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Attach to a manager GameObject and assign MapGenerator and Foreground Tilemap references. 