using UnityEngine;
using System;

[RequireComponent(typeof(DwarfController), typeof(DwarfMovement))]
public class DwarfStateManager : MonoBehaviour
{
    public IState CurrentState { get; private set; }
    
    public DwarfController Controller { get; private set; }
    public DwarfMovement Movement { get; private set; }
    public DwarfSupervisor Supervisor { get; private set; }

    private void Awake()
    {
        Controller = GetComponent<DwarfController>();
        Movement = GetComponent<DwarfMovement>();
        Supervisor = GetComponent<DwarfSupervisor>();
    }

    public void StartFSM()
    {
        // The FSM now waits for the Supervisor's first command.
        Debug.Log("DwarfStateManager FSM ready and waiting for Supervisor.");
    }

    private void Update()
    {
        CurrentState?.OnUpdate();
    }

    public void ChangeState(IState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
        Debug.Log($"Executor changed state to: {newState?.GetType().Name ?? "null"}");
    }

    public void OnMacroBehaviorChanged(MacroBehavior newBehavior)
    {
        // Avoid changing to the same macro-behavior logic.
        // Also, PlayerOverride is handled by its own event, not this general method.
        if (CurrentStateMatchesBehavior(newBehavior) || newBehavior == MacroBehavior.PlayerOverride) return;

        switch (newBehavior)
        {
            case MacroBehavior.Working:
                ChangeState(new FindNearestTileState(this));
                break;
            case MacroBehavior.WanderingInCamp:
                ChangeState(new FindWanderPointState(this));
                break;
            case MacroBehavior.Sleeping:
                GoToSleep();
                break;
        }
    }

    private void HandleMiningOrder(DwarfController dwarf, Vector2Int targetPosition)
    {
        if (dwarf != Controller) return;
        if (CurrentState is SleepingState)
        {
            Debug.Log($"Dwarf '{Controller.CurrentState.DwarfName}' is sleeping and cannot be disturbed.");
            return;
        }

        Debug.Log($"Player order received! Overriding current task.");

        Action onPathSuccess = () =>
        {
            Action onMiningCompleted = () => Supervisor.ReportTaskCompleted();
            ChangeState(new MiningState(this, targetPosition, onMiningCompleted));
        };

        Action onPathFailure = () =>
        {
            Debug.LogWarning("Path to player-ordered target failed.");
            Supervisor.ReportTaskCompleted();
        };
        
        var pathfindingState = new PathfindingToTargetState(this, targetPosition, onPathSuccess, onPathFailure);
        ChangeState(pathfindingState);
    }
    
    private void GoToSleep()
    {
        Action onArrivalAtCamp = () => ChangeState(new SleepingState(this));
        var goHomeState = new PathfindingToTargetState(this, MapGenerator.CampfirePosition, onArrivalAtCamp, onArrivalAtCamp);
        ChangeState(goHomeState);
    }

    private bool CurrentStateMatchesBehavior(MacroBehavior behavior)
    {
        switch (behavior)
        {
            case MacroBehavior.Working:
                return CurrentState is MiningState || CurrentState is FindNearestTileState;
            case MacroBehavior.WanderingInCamp:
                return CurrentState is FindWanderPointState || CurrentState is WaitingInCampState;
            case MacroBehavior.Sleeping:
                return CurrentState is SleepingState;
            default:
                return false;
        }
    }
    
    // We keep listening for direct player orders
    private void OnEnable() => GameEvents.OnMiningOrderGiven += HandleMiningOrder;
    private void OnDisable() => GameEvents.OnMiningOrderGiven -= HandleMiningOrder;
} 