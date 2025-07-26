using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap interactiveTilemap;
    
    private DwarfController selectedDwarf;
    private bool isGameReady = false;

    private void OnEnable()
    {
        GameEvents.OnGameReady += () => isGameReady = true;
    }

    private void OnDisable()
    {
        GameEvents.OnGameReady -= () => isGameReady = true;
    }

    private void Update()
    {
        if (!isGameReady) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        // Deselect dwarf with right-click
        if (Input.GetMouseButtonDown(1) && selectedDwarf != null)
        {
            DeselectDwarf();
        }
    }

    private void HandleMouseClick()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        // If we have a dwarf selected, the next click is an order or deselection
        if (selectedDwarf != null)
        {
            // If we click on a tile
            Vector3Int gridPosition = interactiveTilemap.WorldToCell(mousePosition);
            if (interactiveTilemap.GetTile(gridPosition) != null)
            {
                Debug.Log($"Player orders {selectedDwarf.name} to mine tile {gridPosition}.");
                GameEvents.ReportMiningOrderGiven(selectedDwarf, (Vector2Int)gridPosition);
                DeselectDwarf();
            }
            else
            {
                // Clicked somewhere else, deselect
                DeselectDwarf();
            }
        }
        else // If no dwarf is selected, try to select one
        {
            if (hit.collider != null && hit.collider.TryGetComponent<DwarfController>(out var dwarf))
            {
                SelectDwarf(dwarf);
            }
        }
    }

    private void SelectDwarf(DwarfController dwarf)
    {
        selectedDwarf = dwarf;
        // Here you would add visual feedback, e.g., highlight the dwarf
        Debug.Log($"Dwarf '{dwarf.name}' selected. Right-click to deselect or click a tile to give an order.");
        // We could also fire a UI event here
        GameEvents.ReportDwarfSelected(dwarf.CurrentState);
    }

    private void DeselectDwarf()
    {
        if(selectedDwarf == null) return;
        Debug.Log($"Dwarf '{selectedDwarf.name}' deselected.");
        // Here you would remove visual feedback
        selectedDwarf = null;
    }
}

// ScriptRole: Manages player input for selecting dwarves and issuing mining orders.
// Dependencies: Main Camera, Interactive Tilemap
// HandlesEvents: None
// TriggersEvents: GameEvents.OnMiningOrderGiven, GameEvents.OnDwarfSelected
// UsesSO: None
// NeedsSetup: Assign Main Camera and the interactive Tilemap. 