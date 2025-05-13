using UnityEngine;

public class Pozo : MonoBehaviour
{
    // Podría tener un ID o propiedades específicas si hay varios tipos de pozos.
    // Por ahora, solo sirve como un objeto identificable.
    void Awake()
    {
        Debug.Log($"Pozo: {gameObject.name} inicializado.");
    }
}

// ScriptRole: Represents a Well, a destination for Farmers when performing a Water task.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Attach to a GameObject representing a well in the scene. 