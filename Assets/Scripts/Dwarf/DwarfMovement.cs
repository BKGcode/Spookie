using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

[RequireComponent(typeof(DwarfController))]
public class DwarfMovement : MonoBehaviour
{
    public event Action OnPathCompleted;
    
    private DwarfController controller;
    private List<Vector2Int> currentPath;
    private int pathIndex;
    private bool isMoving;

    private void Awake()
    {
        controller = GetComponent<DwarfController>();
    }

    private void Update()
    {
        if (!isMoving || currentPath == null || pathIndex >= currentPath.Count) return;

        Vector3 targetPosition = Pathfinding.Instance.GridToWorldPosition(currentPath[pathIndex]);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, controller.CurrentState.BaseStats.movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                isMoving = false;
                DwarfRegistry.UpdateDwarfPosition(controller, Pathfinding.Instance.WorldToGridPosition(transform.position));
                OnPathCompleted?.Invoke();
                Debug.Log($"Dwarf '{controller.CurrentState.DwarfName}' reached the end of the path.");
            }
        }
    }

    public void FollowPath(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"Dwarf '{controller.CurrentState.DwarfName}' received an empty or null path.");
            OnPathCompleted?.Invoke();
            return;
        }

        currentPath = path;
        pathIndex = 0;
        isMoving = true;
        Debug.Log($"Dwarf '{controller.CurrentState.DwarfName}' starting path with {path.Count} nodes.");
    }

    public void Stop()
    {
        isMoving = false;
        currentPath = null;
        pathIndex = 0;
        Debug.Log($"Dwarf '{controller.CurrentState.DwarfName}' has stopped moving.");
    }
}

// ScriptRole: Handles the physical movement of the dwarf along a given path.
// Dependencies: DwarfController, Pathfinding
// TriggersEvents: OnPathCompleted
// NeedsSetup: Attach to the dwarf prefab. 