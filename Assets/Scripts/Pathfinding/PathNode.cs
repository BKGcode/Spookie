using UnityEngine;

namespace Pathfinding
{
    public class PathNode
    {
        // Grid coordinates (2D)
        public int gridX;
        public int gridZ;  // Usando Z en lugar de Y para mantener consistencia con Unity
        
        // World position (3D pero Y siempre será 0)
        public Vector3 worldPosition;
        
        // Pathfinding properties
        private bool _walkable;
        public bool walkable 
        { 
            get => _walkable;
            set
            {
                if (_walkable != value)
                {
                    _walkable = value;
                    Debug.Log($"Node at ({gridX}, {gridZ}) walkable updated to: {_walkable}");
                }
            }
        }
        public bool isWalkable => _walkable;
        public int gCost;
        public int hCost;
        public PathNode parent;

        public int fCost => gCost + hCost;

        public PathNode(bool walkable, Vector3 worldPosition, int gridX, int gridZ)
        {
            this._walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public void UpdateWalkable(bool isWalkable)
        {
            if (walkable != isWalkable)
            {
                walkable = isWalkable;
                Debug.Log($"Node at ({gridX}, {gridZ}) walkable updated to: {walkable}");
            }
        }
    }
}

// ScriptRole: Represents a single node in the 2D pathfinding grid (viewed from above)
// RelatedScripts: PathfindingGrid, TerrainGrid
// NeedsSetup: None - Used internally by the pathfinding system