using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding
{
    public static class Pathfinder
    {
        public static List<PathNode> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            PathNode startNode = PathfindingGrid.Instance.WorldPointToNode(startPos);
            PathNode targetNode = PathfindingGrid.Instance.WorldPointToNode(targetPos);

            if (startNode == null || targetNode == null || !targetNode.isWalkable)
            {
                Debug.LogWarning("Pathfinder: Start or Target node is not valid or target is not walkable.");
                return null;
            }

            List<PathNode> openSet = new List<PathNode>();
            HashSet<PathNode> closedSet = new HashSet<PathNode>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (PathNode neighbour in PathfindingGrid.Instance.GetNeighbours(currentNode))
                {
                    if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }
            
            Debug.LogWarning("Pathfinder: Path not found.");
            return null; // No path found
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
            path.Reverse();
            return path;
        }

        private static int GetDistance(PathNode nodeA, PathNode nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
            int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

            if (dstX > dstY)
            {
                if (dstX > dstZ) return 14 * dstX + 10 * (dstY + dstZ);
                return 14 * dstZ + 10 * (dstX + dstY);
            }
            if (dstY > dstZ) return 14 * dstY + 10 * (dstX + dstZ);
            return 14 * dstZ + 10 * (dstX + dstY);
        }
    }
}

// ScriptRole: Implements the A* pathfinding algorithm to find a path between two points on the grid. 