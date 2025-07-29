using UnityEngine;

/// <summary>
/// Defines a type of grass, primarily for visual purposes.
/// </summary>
[CreateAssetMenu(fileName = "GrassType_", menuName = "Spookie/Terrain/Grass Type")]
public class GrassTypeSO : ScriptableObject
{
    [Tooltip("The name of this grass type (e.g., 'Regular', 'With Flowers')")]
    public string GrassName;

    [Tooltip("The color to use when representing this grass in the custom terrain editor.")]
    public Color EditorColor = Color.green;

    [Tooltip("The actual material used to render the grass surface.")]
    public Material Material;
}

// ScriptRole: Defines a reusable type of grass for the top layer of the terrain.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Create instances in the Project via Assets > Create menu. Assign a name and a Material. 