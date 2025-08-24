using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Centraliza el estado del cursor para evitar conflictos entre sistemas (pausa, inventario, UI, gameplay).
    /// KISS: no usa eventos globales; simple contador de "locks" opcional + API directa.
    /// Uso básico: CursorStateController.Instance.SetLocked("Gameplay"); / SetFree("Inventario");
    /// Si varios sistemas piden lock, se mantiene hasta que todos liberen.
    /// </summary>
    [AddComponentMenu("Spookie/Core/Cursor State Controller")] 
    public class CursorStateController : MonoBehaviour
    {
        private static CursorStateController _instance;
        public static CursorStateController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CursorStateController>();
                    if (_instance == null)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("[CursorStateController] No instance found in scene. Create an empty GameObject and add this component.");
#endif
                    }
                }
                return _instance;
            }
        }

        [Header("Config")]
        [Tooltip("Bloquear y ocultar automáticamente el cursor al iniciar la escena (si no hay otros bloqueos).")]
        [SerializeField] private bool lockOnStart = true;

        [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        private int lockCounter = 0; // número de locks activos

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[CursorStateController] Duplicate instance destroyed.");
#endif
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            if (lockOnStart)
            {
                SetLocked("AutoStart");
            }
            else
            {
                ApplyState();
            }
        }

        /// <summary>Solicita que el cursor quede bloqueado/invisible. Debe emparejarse con ReleaseLock.</summary>
        public void SetLocked(string reason = null)
        {
            lockCounter++;
            if (showDebugLogs) Debug.Log($"[CursorStateController] Lock++ ({lockCounter}) {(reason ?? "")} ");
            ApplyState();
        }

        /// <summary>Libera una petición de lock. Cuando el contador llega a 0 se libera el cursor.</summary>
        public void SetFree(string reason = null)
        {
            lockCounter = Mathf.Max(0, lockCounter - 1);
            if (showDebugLogs) Debug.Log($"[CursorStateController] Lock-- ({lockCounter}) {(reason ?? "")} ");
            ApplyState();
        }

        /// <summary>Fuerza liberar todos los locks (ej: al abrir menú principal).</summary>
        public void ForceFreeAll(string reason = null)
        {
            lockCounter = 0;
            if (showDebugLogs) Debug.Log($"[CursorStateController] ForceFreeAll {(reason ?? "")} ");
            ApplyState();
        }

        /// <summary>Devuelve true si actualmente el cursor está bloqueado por al menos una petición.</summary>
        public bool IsLocked => lockCounter > 0;

        private void ApplyState()
        {
            if (IsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnDisable()
        {
            // No modificamos el contador; simplemente volvemos a aplicar el estado para evitar quedarse oculto si el objeto se desactiva.
            ApplyState();
        }
    }
}

// ScriptRole: Control centralizado del cursor con refcount simple
// RelatedScripts: MouseLook, PauseManager, InventoryController (cuando migre)
// UsesSO: No
// ReceivesFrom: Gameplay systems que solicitan lock (llamadas directas)
// SendsTo: Cursor API global
// Adjuntar a: GameObject raíz de la escena (por ejemplo, _Game or Systems). Marcar lockOnStart según flujo deseado.
// Referencias a asignar: ninguna.
// Ejemplo de uso:
//   CursorStateController.Instance.SetLocked("Gameplay");
//   CursorStateController.Instance.SetFree("Inventario cerrado");
// Notas: Cada llamada a SetLocked debe terminar en SetFree para mantener coherencia.
