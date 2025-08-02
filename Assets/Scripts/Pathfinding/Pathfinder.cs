using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding
{
    public static class Pathfinder
    {
        public static List<PathNode> FindPath(Vector3 startPos, Vector3 endPos)
        {
            var grid = PathfindingGrid.Instance;
            if (grid == null)
            {
                Debug.LogError("Pathfinder: No PathfindingGrid instance found!");
                return null;
            }

            PathNode startNode = grid.WorldPointToNode(startPos);
            PathNode targetNode = grid.WorldPointToNode(endPos);

            if (startNode == null || targetNode == null)
            {
                Debug.LogWarning($"Pathfinder: Invalid start ({startPos}) or end ({endPos}) position.");
                return null;
            }

            // CRÍTICO: Validar que el nodo de inicio sea walkable
            if (!startNode.walkable)
            {
                Debug.LogWarning($"Pathfinder: Start node at {startPos} is not walkable!");
                return null;
            }

            // CRÍTICO: Si el nodo de destino no es walkable, buscar el más cercano
            if (!targetNode.walkable)
            {
                Debug.LogWarning($"Pathfinder: Target node at {endPos} is not walkable, searching for nearest walkable node");
                targetNode = grid.FindNearestWalkableNode(endPos);
                if (targetNode == null)
                {
                    Debug.LogWarning($"Pathfinder: No walkable node found near {endPos}");
                    return null;
                }
                Debug.Log($"Pathfinder: Using alternative target at {targetNode.worldPosition}");
            }

            Debug.Log($"Pathfinder: Starting pathfinding from {startPos} to {targetNode.worldPosition}");

            var openSet = new List<PathNode>();
            var closedSet = new HashSet<PathNode>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || 
                        openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    var path = RetracePath(startNode, targetNode);
                    Debug.Log($"Pathfinder: Path found with {path.Count} nodes");
                    return path;
                }

                foreach (PathNode neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }

            Debug.LogWarning($"Pathfinder: No path found from {startPos} to {targetNode.worldPosition}");
            return null;
        }

        private static List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            PathNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Add(startNode);
            path.Reverse();

            return path;
        }

        private static int GetDistance(PathNode nodeA, PathNode nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

            if (dstX > dstZ)
                return 14 * dstZ + 10 * (dstX - dstZ);
            return 14 * dstX + 10 * (dstZ - dstX);
        }
    }
}

// ScriptRole: Implements A* pathfinding algorithm using the PathfindingGrid
// Dependencies: PathfindingGrid, PathNode
// RelatedScripts: DwarfMovement
// NeedsSetup: None - Static utility class