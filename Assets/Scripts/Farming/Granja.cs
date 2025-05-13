using UnityEngine;

public class Granja : MonoBehaviour
{
    // Representa el edificio principal de la granja o el silo donde se entregan las cosechas.
    // Podría tener un ID o propiedades si hay varias granjas/silos.
    void Awake()
    {
        Debug.Log($"Granja: {gameObject.name} inicializada.");
    }
}

// ScriptRole: Represents a Farmhouse/Barn/Silo, a destination for Farmers when delivering harvested crops.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Attach to a GameObject representing the main farm building/silo in the scene. 