using UnityEngine;
using System;

[RequireComponent(typeof(DwarfController), typeof(DwarfMovement))]
public class DwarfStateManager : MonoBehaviour
{
    public IState CurrentState { get; private set; }
    
    // References to be used by states
    public DwarfController Controller { get; private set; }
    public DwarfMovement Movement { get; private set; }
    // Example: public Animator Animator { get; private set; }

    private void Awake()
    {
        // Initialize references
        Controller = GetComponent<DwarfController>();
        Movement = GetComponent<DwarfMovement>();
        // Animator = GetComponent<Animator>();
        Debug.Log("DwarfStateManager awakened.");
    }

    private void OnEnable()
    {
        GameEvents.OnDwarfAssigned += HandleDwarfAssigned;
    }

    private void OnDisable()
    {
        GameEvents.OnDwarfAssigned -= HandleDwarfAssigned;
    }

    private void Start()
    {
        // Set the initial state to start wandering
        ChangeState(new FindWanderPointState(this));
        Debug.Log("DwarfStateManager started. Initial state set to FindWanderPoint.");
    }

    private void Update()
    {
        CurrentState?.OnUpdate();
    }

    public void ChangeState(IState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
        Debug.Log($"Dwarf state changed to: {newState.GetType().Name}");
    }

    private void HandleDwarfAssigned(DwarfController dwarf, Vector2Int targetPosition)
    {
        // Check if this is the dwarf that was assigned the task
        if (dwarf == Controller)
        {
            Debug.Log($"Task received: Move to {targetPosition}");

            // Action to perform when pathfinding succeeds
            Action onPathSuccess = () => {
                // Check if the target tile is minable
                if (Pathfinding.Instance.mapGenerator.GetTileDataAt(targetPosition) != null)
                {
                    ChangeState(new MiningState(this, targetPosition));
                }
                else
                {
                    Debug.Log("Target reached, but it's not minable. Going back to wandering.");
                    ChangeState(new FindWanderPointState(this));
                }
            };

            // Action to perform when pathfinding fails
            Action onPathFailure = () => {
                Debug.LogWarning("Pathfinding failed. Returning to wandering.");
                ChangeState(new FindWanderPointState(this));
            };

            var pathfindingState = new PathfindingToTargetState(this, targetPosition, onPathSuccess, onPathFailure);
            ChangeState(pathfindingState);
        }
    }
} 