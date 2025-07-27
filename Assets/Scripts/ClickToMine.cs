using UnityEngine;

/// <summary>
/// A simple test script that detects mouse clicks and translates them
/// into world coordinates to fire a mine attempt event.
/// </summary>
public class ClickToMine : MonoBehaviour
{
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ClickToMine: Main Camera is not found. Please ensure your camera is tagged 'MainCamera'.", this);
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
// Dependencies: A camera tagged as 'MainCamera' in the scene.
// HandlesEvents: None
// TriggersEvents: PlayerActions.OnMineAttempt
// UsesSO: None
// NeedsSetup: Attach this script to any active GameObject in the scene (e.g., a 'GameManager' or the camera itself). 