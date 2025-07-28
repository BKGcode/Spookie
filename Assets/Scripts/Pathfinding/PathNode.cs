using UnityEngine;

namespace Pathfinding
{
    public class PathNode
    {
        public int gridX;
        public int gridY;
        public int gridZ;

        public Vector3 worldPosition;
        public bool isWalkable;

        public int gCost; // Distance from starting node
        public int hCost; // Heuristic distance to end node
        public PathNode parent;

        public int fCost => gCost + hCost;

        public PathNode(int x, int y, int z, Vector3 worldPos, bool walkable)
        {
            gridX = x;
            gridY = y;
            gridZ = z;
            worldPosition = worldPos;
            isWalkable = walkable;
        }
    }
}

// ScriptRole: Represents a single node in the 3D pathfinding grid. This is not a MonoBehaviour. 