using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "GrassDatabase", menuName = "Spookie/Terrain/Grass Database")]
    public class GrassDatabaseSO : ScriptableObject
    {
        [Tooltip("The list of all grass types that can be used to decorate the terrain surface.")]
        public List<GrassTypeSO> GrassTypes;

        public GrassTypeSO GetRandomGrassType()
        {
            if (GrassTypes == null || GrassTypes.Count == 0)
            {
                Debug.LogWarning("GrassDatabaseSO: The database is empty. Cannot retrieve a grass type.");
                return null;
            }
            
            int randomIndex = Random.Range(0, GrassTypes.Count);
            return GrassTypes[randomIndex];
        }
    }
}
