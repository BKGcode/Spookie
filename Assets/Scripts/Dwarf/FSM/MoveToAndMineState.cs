using UnityEngine;
using System.Collections.Generic;

namespace Dwarf.FSM
{
    public class MoveToAndMineState : IState
    {
        private readonly DwarfStateManager _stateManager;
        private readonly DwarfMovement _dwarfMovement;
        private readonly MapGenerator _mapGenerator;
        private readonly Pathfinding _pathfinding;

        private Vector3 _targetPosition;
        private Vector3Int _tileToMine;
        private bool _hasPath;

        public MoveToAndMineState(DwarfStateManager stateManager)
        {
            _stateManager = stateManager;
            _dwarfMovement = stateManager.DwarfMovement;
            _mapGenerator = stateManager.MapGenerator;
            _pathfinding = stateManager.Pathfinding;
        }

        public void OnEnter()
        {
            Debug.Log("Entering MoveToAndMineState.");
            _targetPosition = _stateManager.TargetPosition;
            _tileToMine = _mapGenerator.GetForegroundTilemap().WorldToCell(_targetPosition);
            _hasPath = false;

            FindPathToTarget();
        }

        public void OnUpdate()
        {
            if (!_hasPath)
            {
                // If we don't have a path, we shouldn't be in this state. Exit.
                Debug.LogWarning("No path found, exiting mining state.");
                _stateManager.ChangeState(new WaitingInCampState(_stateManager));
                return;
            }

            // Wait until the dwarf reaches the destination to start mining
            if (_dwarfMovement.IsAtDestination())
            {
                MineTile();
            }
        }

        public void OnExit()
        {
            _dwarfMovement.Stop();
            Debug.Log("Exiting MoveToAndMineState.");
        }

        private void FindPathToTarget()
        {
            // Find a walkable neighbor to the tile we want to mine
            Vector3Int? walkableNeighbor = _pathfinding.FindNearestWalkableNode(_tileToMine);

            if (walkableNeighbor.HasValue)
            {
                var path = _pathfinding.FindPath(_dwarfMovement.transform.position, _mapGenerator.GetForegroundTilemap().GetCellCenterWorld(walkableNeighbor.Value));
                if (path != null && path.Count > 0)
                {
                    _hasPath = true;
                    _dwarfMovement.FollowPath(path, OnPathCompleted, OnPathFailed);
                }
                else
                {
                    OnPathFailed();
                }
            }
            else
            {
                OnPathFailed();
            }
        }

        private void OnPathCompleted()
        {
            Debug.Log("Path completed. Ready to mine.");
        }

        private void OnPathFailed()
        {
            Debug.LogWarning("Pathfinding failed in MoveToAndMineState.");
            _hasPath = false;
        }
        
        private void MineTile()
        {
            // Ensure we are close enough to the tile to mine it
            if (Vector3.Distance(_dwarfMovement.transform.position, _mapGenerator.GetForegroundTilemap().GetCellCenterWorld(_tileToMine)) > 2.0f) {
                 // Too far, something went wrong, transition to waiting
                _stateManager.ChangeState(new WaitingInCampState(_stateManager));
                return;
            }

            Debug.Log($"Mining tile at {_tileToMine}");
            _mapGenerator.ApplyDamage(new Vector2Int(_tileToMine.x, _tileToMine.y), 100); // Apply enough damage to destroy it

            // Find the next adjacent tile to mine to create a tunnel
            Vector3Int? nextTile = FindNextAdjacentMineableTile(_tileToMine);
            if (nextTile.HasValue)
            {
                Debug.Log($"Found next tile to mine at {nextTile.Value}");
                // Set the next target and re-enter this state to handle movement and mining
                _stateManager.TargetPosition = _mapGenerator.GetForegroundTilemap().GetCellCenterWorld(nextTile.Value);
                _stateManager.ChangeState(new MoveToAndMineState(_stateManager));
            }
            else
            {
                Debug.Log("No more adjacent tiles to mine. Returning to camp.");
                _stateManager.ChangeState(new WaitingInCampState(_stateManager));
            }
        }

        private Vector3Int? FindNextAdjacentMineableTile(Vector3Int currentTile)
        {
            Vector3Int[] neighbors = {
                currentTile + Vector3Int.up,
                currentTile + Vector3Int.down,
                currentTile + Vector3Int.left,
                currentTile + Vector3Int.right
            };

            List<Vector3Int> mineableNeighbors = new List<Vector3Int>();

            foreach (var neighbor in neighbors)
            {
                if (_mapGenerator.IsMineable(neighbor.x, neighbor.y))
                {
                    mineableNeighbors.Add(neighbor);
                }
            }

            if (mineableNeighbors.Count > 0)
            {
                // To make tunnels more natural, let's try to continue in the same direction first
                Vector3Int lastMoveDirection = _stateManager.DwarfMovement.GetLastMovementDirection();
                Vector3Int preferredNextTile = currentTile + lastMoveDirection;
                if(lastMoveDirection != Vector3Int.zero && mineableNeighbors.Contains(preferredNextTile))
                {
                    return preferredNextTile;
                }

                // Otherwise, return a random mineable neighbor
                return mineableNeighbors[Random.Range(0, mineableNeighbors.Count)];
            }
            
            return null;
        }
    }
}
// ScriptRole: Manages the entire process of moving to a target tile and mining it, including finding subsequent adjacent tiles.
// RelatedScripts: DwarfStateManager, DwarfMovement, MapGenerator, Pathfinding
// UsesSO: None
// ReceivesFrom: DwarfStateManager
// SendsTo: DwarfStateManager 