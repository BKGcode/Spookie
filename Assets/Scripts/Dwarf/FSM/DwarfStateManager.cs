using UnityEngine;
using Dwarf.FSM;

[RequireComponent(typeof(DwarfController), typeof(DwarfMovement))]
public class DwarfStateManager : MonoBehaviour
{
    public IState CurrentState { get; private set; }
    public Vector3 TargetPosition { get; set; }
    
    // Dependencies made public for states
    public DwarfController DwarfController { get; private set; }
    public DwarfMovement DwarfMovement { get; private set; }
    public MapGenerator MapGenerator { get; private set; }
    public Pathfinding Pathfinding { get; private set; }
    public DwarfSupervisor Supervisor { get; private set; }

    private void Awake()
    {
        DwarfController = GetComponent<DwarfController>();
        DwarfMovement = GetComponent<DwarfMovement>();
        Supervisor = GetComponent<DwarfSupervisor>();
        
        // Find scene-wide singletons
        MapGenerator = FindObjectOfType<MapGenerator>();
        Pathfinding = FindObjectOfType<Pathfinding>();
    }
    
    private void Start()
    {
        ChangeState(new WaitingInCampState(this));
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
        Debug.Log($"Dwarf state changed to: {CurrentState?.GetType().Name ?? "null"}");
    }

    private void HandleMiningOrder(Vector3 targetPosition)
    {
        if (DwarfController.IsSelected)
        {
            if (CurrentState is SleepingState)
            {
                Debug.Log($"Dwarf '{DwarfController.DwarfData.DwarfName}' is sleeping and cannot be disturbed.");
                return;
            }

            Debug.Log($"Player order received for mining! Overriding current task.");
            TargetPosition = targetPosition;
            ChangeState(new MoveToAndMineState(this));
        }
    }
    
    // We keep listening for direct player orders
    private void OnEnable() => GameEvents.OnMineTile += HandleMiningOrder;
    private void OnDisable() => GameEvents.OnMineTile -= HandleMiningOrder;
}
// ScriptRole: Manages the state of a dwarf using a Finite State Machine (FSM).
// Dependencies: DwarfController, DwarfMovement, DwarfSupervisor
// NeedsSetup: Must be on a GameObject with required components. Finds MapGenerator and Pathfinding in the scene.
// HandlesEvents: GameEvents.OnMineTile
// TriggersEvents: None
// UsesSO: None 