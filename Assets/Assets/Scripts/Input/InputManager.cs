using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    private enum SelectionState { SelectingDwarf, SelectingTile }
    private SelectionState currentState = SelectionState.SelectingDwarf;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap interactiveTilemap;
    [SerializeField] private int dwarvesToAssign = 2;
    
    private DwarfStateManager selectedDwarf;
    private int assignedDwarvesCount = 0;

    private void Update()
    {
        if (assignedDwarvesCount >= dwarvesToAssign)
        {
            this.enabled = false; // Disable after all assignments are done
            Debug.Log("All dwarves assigned. InputManager disabled.");
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        if (currentState == SelectionState.SelectingDwarf)
        {
            TrySelectDwarf(mousePosition);
        }
        else if (currentState == SelectionState.SelectingTile)
        {
            TrySelectTile(mousePosition);
        }
    }

    private void TrySelectDwarf(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null && hit.collider.TryGetComponent<DwarfStateManager>(out var dwarf))
        {
            selectedDwarf = dwarf;
            currentState = SelectionState.SelectingTile;
            Debug.Log($"Dwarf '{dwarf.name}' selected. Now select a target tile.");
        }
    }

    private void TrySelectTile(Vector2 position)
    {
        Vector3Int gridPosition = interactiveTilemap.WorldToCell(position);
        
        // Basic check to see if it's a valid mining target (not empty)
        if (interactiveTilemap.GetTile(gridPosition) != null)
        {
            Debug.Log($"Target tile at {gridPosition} selected for dwarf '{selectedDwarf.name}'.");
            GameEvents.ReportDwarfAssigned(selectedDwarf, (Vector2Int)gridPosition);
            
            assignedDwarvesCount++;
            selectedDwarf = null;
            currentState = SelectionState.SelectingDwarf;
        }
        else
        {
            Debug.Log("Invalid tile selected. Please select a minable tile.");
        }
    }
}

// ScriptRole: Manages player input for selecting dwarves and assigning them tasks.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: GameEvents.OnDwarfAssigned
// UsesSO: None
// NeedsSetup: Assign Main Camera, the interactive Tilemap, and set the number of dwarves to assign. 