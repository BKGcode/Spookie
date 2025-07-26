using System;
using UnityEngine;

public class DwarfMovement : MonoBehaviour
{
    private Vector3 targetPosition;
    private float movementSpeed;
    private bool isMoving = false;
    public event Action OnTargetReached;

    public void Initialize(float speed)
    {
        movementSpeed = speed;
    }

    public void MoveTo(Vector3 target)
    {
        this.targetPosition = target;
        isMoving = true;
        Debug.Log($"Dwarf starting movement towards {targetPosition}");
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            Debug.Log("Dwarf has reached the target.");
            OnTargetReached?.Invoke();
        }
    }
}

// ScriptRole: Handles the physical movement of the dwarf from one point to another.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: OnTargetReached (C# event)
// UsesSO: None
// NeedsSetup: This component is managed by DwarfStateManager. 