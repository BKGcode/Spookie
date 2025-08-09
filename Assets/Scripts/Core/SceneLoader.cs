using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    /// <summary>
    /// Simple scene loading wrapper for fixed scenes. Synchronous loads by name or index.
    /// KISS: no Addressables, no async in this slice.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) { LogWarn("Empty scene name"); return; }
            if (showDebugLogs) Debug.Log($"[SceneLoader] Load by name: {sceneName}");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadSceneByIndex(int buildIndex)
        {
            if (buildIndex < 0) { LogWarn($"Invalid build index: {buildIndex}"); return; }
            if (showDebugLogs) Debug.Log($"[SceneLoader] Load by index: {buildIndex}");
            SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
        }

        public void ReloadCurrent()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) { LogWarn("Active scene invalid"); return; }
            if (showDebugLogs) Debug.Log($"[SceneLoader] Reload: {scene.name}");
            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
        }

        private void LogWarn(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[SceneLoader] {msg}");
            #endif
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Capa mínima para cargar escenas por nombre/índice o recargar la actual.
// RelatedScripts: —
// UsesSO: No
// ReceivesFrom: Botones UI, controladores de menú
// SendsTo: SceneManager.LoadScene
