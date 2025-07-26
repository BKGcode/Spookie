    using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTileData", menuName = "Spookie/Data/Tile Data")]
public class TileDataSO : ScriptableObject
{
    public string tileName;
    public TileBase tile; // This is the main tile for pathfinding and identification
    public TileBase foregroundTile; // The visual tile for the foreground
    public TileBase backgroundTile; // The visual tile for the background
    public int hardness = 100;
    public bool isMineable = true;
}

// ScriptRole: Defines the properties of a specific type of map tile.
// UsesSO: None
// NeedsSetup: Create instances from Assets menu and configure tile properties. 