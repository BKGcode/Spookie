using UnityEngine;

public enum MacroBehavior
{
    Working,
    WanderingInCamp,
    Sleeping,
    PlayerOverride
}

[RequireComponent(typeof(DwarfController), typeof(DwarfStateManager))]
public class DwarfSupervisor : MonoBehaviour
{
    private DwarfStateManager stateManager;
    private GameClockManager clockManager;

    public MacroBehavior CurrentMacroBehavior { get; private set; }
    private bool hasPlayerOrder;

    private void Awake()
    {
        stateManager = GetComponent<DwarfStateManager>();
    }

    private void Start()
    {
        // Find the clock manager once
        clockManager = GameClockManager.Instance;
        if (clockManager == null)
        {
            Debug.LogError("GameClockManager not found in the scene!");
            enabled = false;
        }
    }
    
    private void OnEnable()
    {
        GameEvents.OnMiningOrderGiven += HandlePlayerOrder;
    }

    private void OnDisable()
    {
        GameEvents.OnMiningOrderGiven -= HandlePlayerOrder;
    }

    private void Update()
    {
        if (clockManager == null) return;

        MacroBehavior newBehavior;

        if (hasPlayerOrder)
        {
            newBehavior = MacroBehavior.PlayerOverride;
        }
        else if (clockManager.IsWorkingHours)
        {
            newBehavior = MacroBehavior.Working;
        }
        else if (clockManager.IsWanderingHours)
        {
            newBehavior = MacroBehavior.WanderingInCamp;
        }
        else // IsSleepingHours
        {
            newBehavior = MacroBehavior.Sleeping;
        }
        
        SetMacroBehavior(newBehavior);
    }
    
    private void SetMacroBehavior(MacroBehavior behavior)
    {
        if (CurrentMacroBehavior == behavior) return;
        
        CurrentMacroBehavior = behavior;
        Debug.Log($"Supervisor changed Macro Behavior to: {behavior}");
        
        // Notify the StateManager (Executor) about the change
        stateManager.OnMacroBehaviorChanged(behavior);
    }

    private void HandlePlayerOrder(DwarfController dwarf, Vector2Int targetPosition)
    {
        if (dwarf == GetComponent<DwarfController>())
        {
            hasPlayerOrder = true;
            // The state manager will receive the order through its own event handler
        }
    }
    
    public void ReportTaskCompleted()
    {
        hasPlayerOrder = false;
    }
}

// ScriptRole: The high-level "Supervisor" brain layer for a dwarf.
// Dependencies: DwarfController, DwarfStateManager, GameClockManager
// HandlesEvents: GameEvents.OnMiningOrderGiven
// NeedsSetup: Attach to the dwarf prefab. 