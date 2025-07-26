using UnityEngine;

public class DwarfController : MonoBehaviour
{
    [SerializeField] private DwarfDataSO dwarfData;
    
    public DwarfState CurrentState { get; private set; }

    private void Awake()
    {
        CurrentState = dwarfData.CreateStateInstance();
        gameObject.name = $"Dwarf_{CurrentState.DwarfName}";
    }

    public void Initialize()
    {
        Debug.Log($"Initializing dwarf: {CurrentState.DwarfName}");
        // Future logic for initialization will go here.
    }
}

// ScriptRole: Manages the state and data of a single dwarf instance in the scene.
// Dependencies: DwarfDataSO
// NeedsSetup: Assign the corresponding DwarfDataSO asset. 