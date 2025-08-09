using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    /// <summary>
    /// Debug helper to enqueue a LowerMessage or show an Oniric overlay from buttons or optional input actions.
    /// KISS: Inspector-first, safe to remove in production.
    /// </summary>
    [AddComponentMenu("Spookie/Debug Enqueue Controller")]
    public class DebugEnqueueController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private LowerMessageController lowerMessage;
        [SerializeField] private OniricOverlayController oniric;

        [Header("Lower Message (debug)")]
        [TextArea(2, 4)]
        [SerializeField] private string lowerText = "Mensaje de prueba";
        [SerializeField] private AudioClip lowerAudio;
        [Tooltip("<=0 usa el default del controlador")] 
        [SerializeField] private float lowerAutoCloseSeconds = -1f;
        [Tooltip("Si true, ignora cooldown global del banner para este disparo")] 
        [SerializeField] private bool lowerRepeatable = false;

        [Header("Oniric (debug)")]
        [TextArea(2, 6)]
        [SerializeField] private string oniricText = "Pensamiento Onírico de prueba";
        [SerializeField] private AudioClip oniricAudio;
        [Tooltip("ID opcional para one-shot persistente")] 
        [SerializeField] private string oniricId;
        [SerializeField] private bool oniricOneShot = false;
        [Tooltip("<=0 requiere input para cerrar; -1 usa default")] 
        [SerializeField] private float oniricAutoCloseSeconds = -1f;

        [Header("Input (optional)")]
        [Tooltip("Acción para encolar LowerMessage (p.ej., F1)")]
        [SerializeField] private InputActionReference enqueueLowerAction;
        [Tooltip("Acción para lanzar Oniric (p.ej., F2)")]
        [SerializeField] private InputActionReference showOniricAction;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void OnEnable()
        {
            TryEnable(enqueueLowerAction, OnEnqueueLowerStarted);
            TryEnable(showOniricAction, OnShowOniricStarted);
        }

        private void OnDisable()
        {
            TryDisable(enqueueLowerAction, OnEnqueueLowerStarted);
            TryDisable(showOniricAction, OnShowOniricStarted);
        }

        private void TryEnable(InputActionReference a, System.Action<InputAction.CallbackContext> cb)
        {
            if (a == null) return;
            try { a.action.started += cb; a.action.Enable(); } catch { }
        }
        private void TryDisable(InputActionReference a, System.Action<InputAction.CallbackContext> cb)
        {
            if (a == null) return;
            try { a.action.started -= cb; a.action.Disable(); } catch { }
        }

        private void OnEnqueueLowerStarted(InputAction.CallbackContext ctx) => EnqueueLower();
        private void OnShowOniricStarted(InputAction.CallbackContext ctx) => ShowOniric();

        [ContextMenu("Enqueue Lower (Debug)")]
        public void EnqueueLower()
        {
            if (lowerMessage == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning("[DebugEnqueue] LowerMessageController no asignado");
                #endif
                return;
            }
            if (string.IsNullOrWhiteSpace(lowerText)) return;
            // Usar la sobrecarga repeatable para compat con cooldown global
            lowerMessage.EnqueueText(lowerText, lowerAudio, lowerAutoCloseSeconds, lowerRepeatable);
        }

        [ContextMenu("Show Oniric (Debug)")]
        public void ShowOniric()
        {
            if (oniric == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning("[DebugEnqueue] OniricOverlayController no asignado");
                #endif
                return;
            }
            if (string.IsNullOrWhiteSpace(oniricText)) return;
            oniric.Show(oniricText, oniricAudio, oniricAutoCloseSeconds, oniricId, oniricOneShot);
        }

        private void OnValidate()
        {
            if (lowerMessage == null) lowerMessage = FindObjectOfType<LowerMessageController>();
            if (oniric == null) oniric = FindObjectOfType<OniricOverlayController>();
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Utilidad de debug para encolar mensajes inferiores y lanzar P.O. vía botones/atajos.
// RelatedScripts: Game.UI.LowerMessageController, Game.UI.OniricOverlayController
// UsesSO: No
// ReceivesFrom: Botones UI, InputActionReference (opcional)
// SendsTo: LowerMessageController.EnqueueText, OniricOverlayController.Show
// Adjuntar a: GameObject en escena (solo debug). Asignar referencias o dejar autodetección en OnValidate.
