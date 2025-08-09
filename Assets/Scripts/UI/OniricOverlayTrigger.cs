using UnityEngine;
using UnityEngine.Serialization;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// Simple trigger to show an Oniric overlay when the player enters or interacts.
    /// One-shot and PlayerPrefs persistence supported via an id.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class OniricOverlayTrigger : MonoBehaviour
    {
        public enum FireMode { OnTriggerEnter, Manual }

        [Header("Target")]
        [SerializeField] private OniricOverlayController overlay;

        [Header("Content")]
        [TextArea(2,6)]
        [SerializeField] private string oniricText;
        [SerializeField] private AudioClip audioClip;
        [Tooltip("Unique id to persist as seen (PlayerPrefs). Leave empty to always show.")]
        [SerializeField] private string persistentId;
        [SerializeField] private bool oneShot = true;

        [Header("Rules")]
        [SerializeField] private FireMode mode = FireMode.OnTriggerEnter;
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool onlyOnceRuntime = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private bool _firedRuntime;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnValidate()
        {
            if (overlay == null)
            {
                overlay = FindObjectOfType<OniricOverlayController>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (mode != FireMode.OnTriggerEnter) return;
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
            TryFire();
        }

        public void TryFire()
        {
            if (onlyOnceRuntime && _firedRuntime) return;
            if (overlay == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showDebugLogs) Debug.LogWarning($"[OniricOverlayTrigger] Missing overlay on {name}");
                #endif
                return;
            }
            if (string.IsNullOrWhiteSpace(oniricText)) return;

            overlay.Show(oniricText, audioClip, -1f, persistentId, oneShot);
            _firedRuntime = true;
        }

        [ContextMenu("Fire (Editor Play)")]
        private void CtxFire()
        {
            TryFire();
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Dispara P.O. al entrar en trigger o manualmente. Persistencia por id.
// RelatedScripts: Game.UI.OniricOverlayController
// UsesSO: No
// ReceivesFrom: Collider trigger, eventos de guión (TryFire)
// SendsTo: OniricOverlayController.Show
