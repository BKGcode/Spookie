using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Minimal quit handler callable from a UI button. In editor, stops play mode; in build, quits app.
    /// </summary>
    [AddComponentMenu("Spookie/Quit Handler")]
    public class QuitHandler : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public void Quit()
        {
            if (showDebugLogs) Debug.Log("[QuitHandler] Quit requested");
            #if UNITY_EDITOR
            // In editor, stop play mode
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Salir del juego desde UI.
// RelatedScripts: —
// UsesSO: No
// ReceivesFrom: Botón UI
// SendsTo: Application.Quit (o para editor, detener Play)
