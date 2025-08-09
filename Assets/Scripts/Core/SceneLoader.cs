using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    /// <summary>
    /// Simple scene loading wrapper for fixed scenes. Synchronous loads by name or index.
    /// KISS: no Addressables, no async in this slice.
    /// </summary>
    [AddComponentMenu("Spookie/Scene Loader")]
    public class SceneLoader : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

    [Header("Integration (optional)")]
    [Tooltip("If assigned, safe load will use this PauseManager to gate inputs during scene transitions.")]
    [SerializeField] private PauseManager pauseManager;

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

        // Safe variants (optional pause gating)
        public void LoadSceneSafelyByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) { LogWarn("Empty scene name"); return; }
            bool usedPause = false;
            try
            {
                if (pauseManager != null)
                {
                    pauseManager.RequestPause(PauseReason.Transition);
                    usedPause = true;
                }
            }
            catch { }

            if (showDebugLogs) Debug.Log($"[SceneLoader] Safe load by name: {sceneName}");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            if (usedPause)
            {
                try { pauseManager.ReleasePause(PauseReason.Transition); } catch { }
            }
        }

        public void ReloadCurrentSafely()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) { LogWarn("Active scene invalid"); return; }
            bool usedPause = false;
            try
            {
                if (pauseManager != null)
                {
                    pauseManager.RequestPause(PauseReason.Transition);
                    usedPause = true;
                }
            }
            catch { }

            if (showDebugLogs) Debug.Log($"[SceneLoader] Safe reload: {scene.name}");
            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);

            if (usedPause)
            {
                try { pauseManager.ReleasePause(PauseReason.Transition); } catch { }
            }
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
