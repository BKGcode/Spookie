using UnityEngine;
using Terrain.Data;

namespace SO
{
    [CreateAssetMenu(fileName = "NewTerrainLevel", menuName = "Spookie/Terrain/Terrain Level Asset")]
    public class TerrainDataAsset : ScriptableObject
    {
        [Tooltip("The actual data snapshot for this terrain level.")]
        public TerrainSaveData LevelData;
    }
}
