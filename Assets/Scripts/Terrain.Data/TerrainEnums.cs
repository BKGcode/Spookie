using UnityEngine;

namespace Terrain.Data
{
    public enum SpaceType
    {
        None,
        Empty,
        Treasure,
        Enemy
    }

    public enum GenerationPhase
    {
        MaterialBase,
        SpecialMaterials,
        Postprocess
    }

    public enum MiningState
    {
        Intact,
        Damaged,
        Mined
    }
}
