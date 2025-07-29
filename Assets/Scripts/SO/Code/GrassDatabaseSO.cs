using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A database that holds all available grass types for terrain generation.
/// </summary>
[CreateAssetMenu(fileName = "GrassDatabase", menuName = "Spookie/Terrain/Grass Database")]
public class GrassDatabaseSO : ScriptableObject
{
    [Tooltip("The list of all grass types that can be used to decorate the terrain surface.")]
    public List<GrassTypeSO> GrassTypes;

    /// <summary>
    /// Gets a random grass type from the database.
    /// Returns null if the database is empty.
    /// </summary>
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

// ScriptRole: Holds a list of all available GrassTypeSO for the terrain system.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: GrassTypeSO
// NeedsSetup: Create one instance in the Project. Populate the 'GrassTypes' list with GrassTypeSO assets. 