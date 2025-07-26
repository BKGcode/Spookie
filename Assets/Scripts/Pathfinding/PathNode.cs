using UnityEngine;

public class PathNode
{
    public Vector2Int gridPosition;
    public bool isWalkable;

    public int gCost; // Cost from the start node
    public int hCost; // Heuristic cost to the end node
    public int fCost; // gCost + hCost

    public PathNode parentNode;

    public PathNode(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
        this.isWalkable = true; // Assume walkable until proven otherwise
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}

// ScriptRole: A data container representing a single tile in the pathfinding grid.
// This is not a MonoBehaviour. 