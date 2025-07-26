using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(DwarfController))]
public class DwarfMovement : MonoBehaviour
{
    private Action _onPathCompleted;
    private Action _onPathFailed;
    
    private DwarfController _dwarfController;
    private List<Vector3> _currentPath;
    private int _pathIndex;
    private bool _isMoving;
    private Vector3Int _lastMovementDirection;

    private void Awake()
    {
        _dwarfController = GetComponent<DwarfController>();
    }

    private void Update()
    {
        if (!_isMoving || _currentPath == null || _pathIndex >= _currentPath.Count) return;

        Vector3 targetPosition = _currentPath[_pathIndex];
        Vector3 currentPosition = transform.position;

        transform.position = Vector3.MoveTowards(currentPosition, targetPosition, _dwarfController.DwarfData.movementSpeed * Time.deltaTime);

        Vector3Int newDirection = Vector3Int.RoundToInt((targetPosition - currentPosition).normalized);
        if(newDirection != Vector3Int.zero)
        {
            _lastMovementDirection = newDirection;
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _pathIndex++;
            if (_pathIndex >= _currentPath.Count)
            {
                Stop();
                _onPathCompleted?.Invoke();
                Debug.Log($"Dwarf '{_dwarfController.DwarfData.DwarfName}' reached the end of the path.");
            }
        }
    }

    public void FollowPath(List<Vector3> path, Action onCompleted, Action onFailed)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"Dwarf '{_dwarfController.DwarfData.DwarfName}' received an empty or null path.");
            _onPathFailed?.Invoke();
            return;
        }

        _currentPath = path;
        _pathIndex = 0;
        _isMoving = true;
        _onPathCompleted = onCompleted;
        _onPathFailed = onFailed;
        Debug.Log($"Dwarf '{_dwarfController.DwarfData.DwarfName}' starting path with {_currentPath.Count} nodes.");
    }

    public void Stop()
    {
        _isMoving = false;
        _currentPath = null;
        _pathIndex = 0;
        _lastMovementDirection = Vector3Int.zero;
        Debug.Log($"Dwarf '{_dwarfController.DwarfData.DwarfName}' has stopped moving.");
    }

    public bool IsAtDestination()
    {
        return !_isMoving;
    }

    public Vector3Int GetLastMovementDirection()
    {
        return _lastMovementDirection;
    }
}

// ScriptRole: Handles the physical movement of the dwarf along a given path.
// RelatedScripts: DwarfController, Pathfinding, DwarfStateManager
// UsesSO: DwarfDataSO
// ReceivesFrom: DwarfStateManager (via method calls)
// SendsTo: None (uses Actions for callbacks) 