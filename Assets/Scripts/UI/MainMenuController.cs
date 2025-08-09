using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Minimal Main Menu controller: Play loads a target scene, Exit quits the app.
    /// KISS: references wired by Inspector; no Addressables; no pause involvement.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Scene loader utility used to load the gameplay scene.")]
        [SerializeField] private Game.Core.SceneLoader sceneLoader;
        [Tooltip("Quit handler for Exit button.")]
        [SerializeField] private Game.Core.QuitHandler quitHandler;

        [Header("Target Scene")]
        [Tooltip("Name of the gameplay scene to load when pressing Play.")]
        [SerializeField] private string gameplaySceneName = "Game";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public void Play()
        {
            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning("[MainMenu] gameplaySceneName is empty");
                #endif
                return;
            }
            if (sceneLoader != null)
            {
                // Prefer safe load if PauseManager is wired in SceneLoader; fallback to normal
                sceneLoader.LoadSceneSafelyByName(gameplaySceneName);
            }
            else
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning("[MainMenu] SceneLoader not assigned, using direct load via SceneManager");
                #endif
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
            }
        }

        public void Exit()
        {
            if (quitHandler != null)
            {
                quitHandler.Quit();
            }
            else
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Controlador mínimo de Menú Principal (Play/Exit).
// RelatedScripts: Game.Core.SceneLoader, Game.Core.QuitHandler
// UsesSO: No
// ReceivesFrom: Botones UI (Play/Exit)
// SendsTo: SceneLoader (LoadSceneSafelyByName), QuitHandler.Quit
// Adjuntar a: GameObject raíz del Main Menu. Asignar SceneLoader y QuitHandler por Inspector. Configurar gameplaySceneName.
