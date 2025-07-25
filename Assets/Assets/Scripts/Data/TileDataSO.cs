using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTileData", menuName = "Spookie/Data/Tile Data")]
public class TileDataSO : ScriptableObject
{
    [Tooltip("Name of the tile type.")]
    public string tileName;

    [Tooltip("Health or durability of the tile.")]
    public int health;

    [Tooltip("The tile that remains after this one is destroyed.")]
    public TileBase backgroundTile;

    [Tooltip("Sprites showing progressive damage.")]
    public Sprite[] damageSprites;
}

// ScriptRole: Defines the properties of a specific type of tile.
// UsesSO: None
// NeedsSetup: Create instances from Assets menu and configure tile properties. 