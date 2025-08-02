/*
Este archivo sirve como plantilla para los bloques de resumen que deben incluirse al final de cada script.
NO se usa en el código, es solo una referencia.

Formato del bloque de resumen:

// ScriptRole: [Descripción breve y clara del propósito principal del script]
// Dependencies: [Lista de componentes requeridos o sistemas externos]
// HandlesEvents: [Eventos a los que se suscribe, si aplica]
// TriggersEvents: [Eventos que dispara, si aplica]
// UsesSO: [ScriptableObjects utilizados, si aplica]
// Features: [Lista de características principales, si es complejo]
// NeedsSetup: [Pasos necesarios para configurar el script]

Ejemplos:

Para un MonoBehaviour simple:
// ScriptRole: Controls player movement and physics interactions
// Dependencies: Rigidbody2D, PlayerInput
// HandlesEvents: InputSystem.OnMove, InputSystem.OnJump
// TriggersEvents: OnJumpStart, OnLand
// UsesSO: PlayerStatsSO
// NeedsSetup: Attach to player prefab, assign PlayerStatsSO in inspector

Para un ScriptableObject:
// ScriptRole: Stores and manages player statistics and upgrades
// Dependencies: None
// UsesSO: None
// Features:
//   - Stat modification with validation
//   - Upgrade path management
//   - Progress saving/loading
// NeedsSetup: Create via Assets > Create > Game > Player Stats

Para una clase de utilidad:
// ScriptRole: Provides helper methods for path finding calculations
// Dependencies: None
// Features:
//   - A* pathfinding implementation
//   - Path smoothing and optimization
//   - Cached results for performance
// Usage: PathFinder.FindPath(start, end, options);

Notas importantes:
1. Mantener las descripciones breves y claras
2. Incluir solo las secciones relevantes
3. Usar viñetas para listas largas
4. Agregar ejemplos de uso si es necesario
5. Documentar dependencias críticas
*/

// Este archivo es solo una plantilla y no debe compilarse
#if false
namespace Core.Shared
{
    public class ScriptSummaryTemplate
    {
        // Este es un archivo de plantilla, no implementar
    }
}
#endif

// ScriptRole: Provides a template for standardized script documentation blocks
// Dependencies: None
// Features:
//   - Standardized format for script documentation
//   - Examples for different script types
//   - Clear guidelines for usage
// Usage: Copy relevant sections to new scripts and fill in the details