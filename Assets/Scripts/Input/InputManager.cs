using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap interactiveTilemap;
    
    private DwarfController _selectedDwarf;
    private bool _isGameReady = false;

    private void OnEnable()
    {
        GameEvents.OnGameReady += () => _isGameReady = true;
    }

    private void OnDisable()
    {
        GameEvents.OnGameReady -= () => _isGameReady = true;
    }

    private void Update()
    {
        if (!_isGameReady) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        if (Input.GetMouseButtonDown(1) && _selectedDwarf != null)
        {
            DeselectDwarf();
        }
    }

    private void HandleMouseClick()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        if (_selectedDwarf != null)
        {
            Vector3Int gridPosition = interactiveTilemap.WorldToCell(mouseWorldPos);
            if (interactiveTilemap.GetTile(gridPosition) != null)
            {
                Debug.Log($"Player orders {_selectedDwarf.name} to mine tile at {gridPosition}.");
                GameEvents.ReportMineTile(interactiveTilemap.GetCellCenterWorld(gridPosition));
                DeselectDwarf();
            }
            else
            {
                DeselectDwarf();
            }
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
            if (hit.collider != null && hit.collider.TryGetComponent<DwarfController>(out var dwarf))
            {
                SelectDwarf(dwarf);
            }
        }
    }

    private void SelectDwarf(DwarfController dwarf)
    {
        if (_selectedDwarf != null)
        {
            DeselectDwarf();
        }
        
        _selectedDwarf = dwarf;
        Debug.Log($"Dwarf '{dwarf.name}' selected.");
        GameEvents.ReportDwarfSelected(dwarf);
    }

    private void DeselectDwarf()
    {
        if(_selectedDwarf == null) return;
        Debug.Log($"Dwarf '{_selectedDwarf.name}' deselected.");
        _selectedDwarf = null;
        GameEvents.ReportDeselectAllDwarfs();
    }
}

// ScriptRole: Manages player input for selecting dwarves and issuing mining orders.
// Dependencies: Main Camera, Interactive Tilemap
// HandlesEvents: GameEvents.OnGameReady
// TriggersEvents: GameEvents.ReportMineTile, GameEvents.ReportDwarfSelected, GameEvents.ReportDeselectAllDwarfs
// UsesSO: None
// NeedsSetup: Assign Main Camera and the interactive Tilemap. 