public class TileState
{
    public TileDataSO TileData { get; }
    public int CurrentHealth { get; set; }

    public TileState(TileDataSO tileData)
    {
        TileData = tileData;
        CurrentHealth = tileData.health;
    }
}

// ScriptRole: A simple data container for the runtime state of a tile. 