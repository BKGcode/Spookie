using UnityEngine;

/// <summary>
/// A simple test script that detects mouse clicks and translates them
/// into world coordinates to fire a mine attempt event.
/// </summary>
public class ClickToMine : MonoBehaviour
{
    [Tooltip("The camera used to cast rays from the screen into the world.")]
    [SerializeField] private Camera mainCamera;

    void Awake()
    {
        if (mainCamera == null)
        {
            Debug.LogError("ClickToMine: Camera reference is not set. Please assign it in the Inspector.", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            HandleMouseClick(Input.mousePosition);
        }
    }

    private void HandleMouseClick(Vector3 mousePosition)
    {
        // For a 2D-style game on the XZ plane, we can raycast against a conceptual plane.
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // A plane at y=0

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            
            // Convert world position to integer grid coordinates
            int x = Mathf.RoundToInt(worldPosition.x);
            int y = Mathf.RoundToInt(worldPosition.z);
            
            Vector2Int tileCoords = new Vector2Int(x, y);
            
            // Fire the global event for any system to hear
            PlayerActions.TriggerMineAttempt(tileCoords);
        }
    }
}


// ScriptRole: Translates player mouse clicks into mining attempt events.
// Dependencies: A camera assigned in the Inspector.
// HandlesEvents: None
// TriggersEvents: PlayerActions.OnMineAttempt
// UsesSO: None
// NeedsSetup: Attach to any active GameObject. Assign the main scene camera to the 'mainCamera' field. 