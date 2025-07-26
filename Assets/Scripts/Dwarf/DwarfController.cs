using UnityEngine;

public class DwarfController : MonoBehaviour
{
    [SerializeField] private DwarfDataSO _dwarfData;
    public DwarfDataSO DwarfData => _dwarfData;
    
    public bool IsSelected { get; private set; }

    private void Awake()
    {
        gameObject.name = $"Dwarf_{_dwarfData.DwarfName}";
    }
    
    private void OnEnable()
    {
        GameEvents.OnDwarfSelected += HandleDwarfSelected;
        GameEvents.OnDeselectAllDwarfs += HandleDeselectAllDwarfs;
    }

    private void OnDisable()
    {
        GameEvents.OnDwarfSelected -= HandleDwarfSelected;
        GameEvents.OnDeselectAllDwarfs -= HandleDeselectAllDwarfs;
    }
    
    private void HandleDwarfSelected(DwarfController selectedDwarf)
    {
        IsSelected = (selectedDwarf == this);
    }

    private void HandleDeselectAllDwarfs()
    {
        IsSelected = false;
    }

    public void Initialize()
    {
        // This can be used for any post-instantiation setup if needed.
        Debug.Log($"Initialized {gameObject.name}");
    }
}

// ScriptRole: Manages the dwarf's data (SO) and its selection state.
// RelatedScripts: DwarfDataSO, DwarfStateManager, GameEvents
// UsesSO: DwarfDataSO
// ReceivesFrom: GameEvents (OnDwarfSelected, OnDeselectAllDwarfs)
// SendsTo: None 