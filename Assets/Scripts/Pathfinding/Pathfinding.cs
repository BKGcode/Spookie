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

    public List<Vector2Int> FindPath(Vector2Int startPosition, Vector2Int endPosition)
    {
        PathNode startNode = grid[startPosition.x, startPosition.y];
        PathNode endNode = grid[endPosition.x, endPosition.y];

        Debug.Log($"[Pathfinding] Request: From {startPosition} To {endPosition}. Is End Walkable? {endNode.isWalkable}");

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
                List<Vector2Int> path = ReconstructPath(endNode);
                Debug.Log($"[Pathfinding] Path Found: {path.Count} nodes.");
                return path;
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

    private List<Vector2Int> ReconstructPath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;
        while(currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();

        List<Vector2Int> vectorPath = new List<Vector2Int>();
        foreach(var node in path)
        {
            vectorPath.Add(node.gridPosition);
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