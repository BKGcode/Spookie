using UnityEngine;
using SO;
using Terrain.Data;

namespace Terrain
{
    public static class TerrainDataFactory
    {
        public static TerrainData CreateFromSaveData(TerrainSaveData saveData, MaterialDatabase materialDatabase)
        {
            if (saveData == null || materialDatabase == null)
            {
                Debug.LogError("TerrainDataFactory: Cannot create terrain. SaveData or MaterialDatabase is null.");
                return null;
            }

            var terrainData = new TerrainData(saveData.Width, saveData.Height, saveData.MapSeed);

            if (saveData.DwarfSpawnPoints != null)
            {
                terrainData.DwarfSpawnPoints.AddRange(saveData.DwarfSpawnPoints);
            }
            if (saveData.BedSpawnPoints != null)
            {
                terrainData.BedSpawnPoints.AddRange(saveData.BedSpawnPoints);
            }

            if (saveData.AllTiles == null || saveData.AllTiles.Count != saveData.Width * saveData.Height)
            {
                Debug.LogError("TerrainDataFactory: Tile data is corrupted or does not match terrain dimensions. Returning empty terrain.");
                return terrainData;
            }

            int tileIndex = 0;
            for (int y = 0; y < saveData.Height; y++)
            {
                for (int x = 0; x < saveData.Width; x++)
                {
                    var savedTile = saveData.AllTiles[tileIndex];

                    MaterialSO material = materialDatabase.GetMaterialByGuid(savedTile.MaterialGuid);
                    if (material == null)
                    {
                        if (!string.IsNullOrEmpty(savedTile.MaterialGuid))
                        {
                            Debug.LogWarning($"Could not find material with GUID '{savedTile.MaterialGuid}'. Using base material.");
                        }
                        material = materialDatabase.GetBaseMaterial();
                    }

                    TerrainTile newTile = new TerrainTile(material);
                    newTile.SetMiningState(savedTile.MiningState);
                    
                    // Si el tile está minado, asegurarnos de que tenga SpaceType.Empty
                    if (savedTile.MiningState == MiningState.Mined)
                    {
                        newTile.SetSpecialProperty(SpaceType.Empty);
                    }
                    else
                    {
                        newTile.SetSpecialProperty(savedTile.SpecialProperty);
                    }

                    terrainData.SetTile(x, y, newTile);
                    tileIndex++;
                }
            }

            Debug.Log("TerrainDataFactory: Created TerrainData from save data successfully.");
            return terrainData;
        }
    }
}
