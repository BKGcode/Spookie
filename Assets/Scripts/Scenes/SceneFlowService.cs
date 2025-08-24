using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Save;

namespace Game.Scenes
{
    /// <summary>
    /// Servicio ligero para cargar escena objetivo si difiere de la actual. Sin Addressables (escenas fijas Build Settings).
    /// </summary>
    [AddComponentMenu("Spookie/Scenes/Scene Flow Service")]
    public class SceneFlowService : MonoBehaviour
    {
        public static SceneFlowService Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool LoadSceneIfNeeded(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            if (SceneManager.GetActiveScene().name == sceneName) return false; // ya estamos
            try
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                Log($"Scene loaded: {sceneName}");
                return true;
            }
            catch (System.Exception ex)
            {
                Log($"Load error {sceneName}: {ex.Message}");
                return false;
            }
        }

        private void Log(string msg)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.Log($"[SceneFlowService] {msg}");
#endif
        }
    }
}

// ScriptRole: Carga escenas secuenciales cuando cambia el día.
// RelatedScripts: SaveGameManager, DayChapterSequenceSO
// UsesSO: No
// ReceivesFrom: Day/Night transition logic
// SendsTo: SceneManager
// Adjuntar: GameObject persistente (_Systems). Crear si no existe SaveGameManager (ambos pueden convivir).