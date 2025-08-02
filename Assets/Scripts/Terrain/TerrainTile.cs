using UnityEngine;
using SO;
using Terrain.Data;

namespace Terrain
{
    [System.Serializable]
    public struct TerrainTile
    {
        [SerializeField] private MaterialSO material;
        [SerializeField] private MiningState miningState;
        [SerializeField] private SpaceType specialProperty;

        public TerrainTile(MaterialSO initialMaterial)
        {
            material = initialMaterial;
            miningState = MiningState.Intact;
            specialProperty = SpaceType.None;
        }

        public bool IsMineable()
        {
            return material != null && miningState != MiningState.Mined;
        }

        public bool IsWalkable()
        {
            return miningState == MiningState.Mined && specialProperty == SpaceType.Empty;
        }
        
        public MaterialSO GetMaterial()
        {
            return material;
        }
        
        public void SetMaterial(MaterialSO newMaterial)
        {
            material = newMaterial;
        }

        public void SetSpecialProperty(SpaceType newProperty)
        {
            specialProperty = newProperty;
            Debug.Log($"TerrainTile: Special property set to {newProperty}.");
        }

        public void SetMiningState(MiningState newState)
        {
            if (miningState != newState)
            {
                miningState = newState;
                Debug.Log($"TerrainTile: Mining state changed to {newState}.");
            }
        }
        
        public MiningState GetMiningState()
        {
            return miningState;
        }

        public SpaceType GetSpecialProperty()
        {
            return specialProperty;
        }
    }
}
