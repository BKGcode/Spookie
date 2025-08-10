using System;
using System.Collections.Generic;
using UnityEngine;
using PlayerController; // IInteractable
using Game.UI; // LowerMessageController, OniricOverlayController
using Game.Core; // PauseManager, PauseReason
using Game.Core; // MessageRuntimeSettingsSO via provider
using Game.Messages; // MessageDBSO, MessageType

namespace Game.Interaction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/DB Text Interactable (Unified)")]
    public class DBTextInteractable : MonoBehaviour, IInteractable
    {
        public enum FireMode { Interaction, Trigger }
        public enum ChannelPolicy { Auto, Lower, Oniric }
    public enum TriBool { UseDB, ForceFalse, ForceTrue }

        [Header("Databases")]
        [Tooltip("Message database with ids → keys/type/audio/flags.")]
        [SerializeField] private MessageDBSO messageDB;

        [Header("Message")]
        [Tooltip("Message id to resolve in MessageDB.")]
        [SerializeField] private string messageId;
        [Tooltip("Where to display. Auto reads DB.type; override to force.")]
        [SerializeField] private ChannelPolicy channel = ChannelPolicy.Auto;

    [Header("Overrides (optional)")]
    [Tooltip("Autoclose seconds override. <0 = use DB/controller.")]
    [SerializeField] private float autoCloseSecondsOverride = -1f;
    [Tooltip("Typewriter CPS override (when supported). <=0 = controller default.")]
    [SerializeField] private float typewriterCpsOverride = -1f;
    [Tooltip("OneShot policy: UseDB, ForceFalse o ForceTrue.")]
    [SerializeField] private TriBool oneShotPolicy = TriBool.UseDB;
    [Tooltip("Persistence key override (PlayerPrefs). Empty = use DB persistentId or messageId.")]
    [SerializeField] private string persistentIdOverride = string.Empty;

    // Integration (auto): se resuelven automáticamente en Reset/OnValidate/Awake. Oculto para simplificar el flujo.
    [SerializeField, HideInInspector] private LowerMessageController lowerMessage;
    [SerializeField, HideInInspector] private OniricOverlayController oniricOverlay;
    [Tooltip("Optional PauseManager to gate by UI/Transition/Oniric reasons.")]
    [SerializeField, HideInInspector] private PauseManager pauseManager;

        [Header("Rules")]
        [SerializeField] private FireMode fireMode = FireMode.Interaction;
        [SerializeField] private string requiredTag = "Player";
    [Tooltip("Cooldown seconds between fires (0 = none, <0 = use global settings if available)")]
    [SerializeField] private float cooldownSeconds = -1f;

        [Header("Outline (optional)")]
        [Tooltip("URP outline material. If not set, highlighting does nothing (no error).")]
        [SerializeField] private Material outlineMaterial;
        [Tooltip("Renderers to outline; auto-collect child MeshRenderer/SkinnedMeshRenderer if empty.")]
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();
        [SerializeField] private bool restoreOriginalOnUnhighlight = true;

        [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        // State
        private bool _firedRuntime;
        private float _lastFireTime = -999f;
        private readonly List<Material[]> _originalMats = new List<Material[]>();
        private bool _highlighted;

        private const string PP_BannerPrefix = "Spookie.MsgSeen.";

        public string MessageId { get => messageId; set => messageId = value; }

        private void Reset()
        {
            if (pauseManager == null) pauseManager = FindObjectOfType<PauseManager>();
            if (lowerMessage == null) lowerMessage = FindObjectOfType<LowerMessageController>();
            if (oniricOverlay == null) oniricOverlay = FindObjectOfType<OniricOverlayController>();
            // Ensure collider for raycast/trigger
            var col = GetComponent<Collider>();
            if (col == null) col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = (fireMode == FireMode.Trigger);

            // Auto collect renderers
            AutoCollectRenderers();
        }

        private void OnValidate()
        {
            if (pauseManager == null) pauseManager = FindObjectOfType<PauseManager>();
            if (lowerMessage == null) lowerMessage = FindObjectOfType<LowerMessageController>();
            if (oniricOverlay == null) oniricOverlay = FindObjectOfType<OniricOverlayController>();
            // Keep collider trigger state in sync
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = (fireMode == FireMode.Trigger);
        }

        private void Awake()
        {
            CacheOriginalMats();
        }

        private void OnDisable()
        {
            if (_highlighted) SetHighlighted(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (fireMode != FireMode.Trigger) return;
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
            TryFire();
        }

        public void Interact(GameObject interactor)
        {
            if (fireMode != FireMode.Interaction)
            {
                TryFire();
                return;
            }
            TryFire();
        }

        public string GetInteractionPrompt()
        {
            return string.IsNullOrWhiteSpace(messageId) ? "Interact" : "Leer";
        }

        public void SetHighlighted(bool enabled)
        {
            if (_highlighted == enabled) return;
            _highlighted = enabled;
            if (outlineMaterial == null) return; // no-op if not configured
            if (targetRenderers == null || targetRenderers.Count == 0) AutoCollectRenderers();

            if (enabled)
            {
                for (int i = 0; i < targetRenderers.Count; i++)
                {
                    var r = targetRenderers[i]; if (r == null) continue;
                    var mats = r.materials;
                    bool has = false; for (int m = 0; m < mats.Length; m++) { if (mats[m] == outlineMaterial) { has = true; break; } }
                    if (!has)
                    {
                        Array.Resize(ref mats, mats.Length + 1);
                        mats[mats.Length - 1] = outlineMaterial;
                        r.materials = mats;
                    }
                }
            }
            else
            {
                for (int i = 0; i < targetRenderers.Count; i++)
                {
                    var r = targetRenderers[i]; if (r == null) continue;
                    if (restoreOriginalOnUnhighlight && i < _originalMats.Count && _originalMats[i] != null && _originalMats[i].Length > 0)
                    {
                        r.materials = _originalMats[i];
                    }
                    else
                    {
                        var mats = r.materials; int count = 0;
                        for (int m = 0; m < mats.Length; m++) if (mats[m] != outlineMaterial) count++;
                        if (count != mats.Length)
                        {
                            var newMats = new Material[count]; int w = 0;
                            for (int m = 0; m < mats.Length; m++) { if (mats[m] == outlineMaterial) continue; newMats[w++] = mats[m]; }
                            r.materials = newMats;
                        }
                    }
                }
            }
        }

        private void AutoCollectRenderers()
        {
            targetRenderers = new List<Renderer>();
            GetComponentsInChildren(true, targetRenderers);
            for (int i = targetRenderers.Count - 1; i >= 0; i--)
            {
                if (!(targetRenderers[i] is MeshRenderer) && !(targetRenderers[i] is SkinnedMeshRenderer))
                    targetRenderers.RemoveAt(i);
            }
            CacheOriginalMats();
        }

        private void CacheOriginalMats()
        {
            _originalMats.Clear();
            if (!restoreOriginalOnUnhighlight) return;
            if (targetRenderers == null) return;
            foreach (var r in targetRenderers)
            {
                _originalMats.Add(r != null ? r.materials : Array.Empty<Material>());
            }
        }

        private void TryFire()
        {
            if (string.IsNullOrWhiteSpace(messageId)) return;
            if (pauseManager != null && pauseManager.IsPaused)
            {
                var reasons = pauseManager.GetActiveReasons();
                bool hasUI = ContainsReason(reasons, PauseReason.UI);
                bool hasTransition = ContainsReason(reasons, PauseReason.Transition);
                // Allow during Oniric for queueing; but block UI/Transition
                if (hasUI || hasTransition)
                {
                    if (showDebugLogs)
                    {
                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log($"[DBTextInteractable] Gated by {(hasTransition ? "Transition" : "UI")} on {name}");
                        #endif
                    }
                    return;
                }
            }

            // One-shot persistence
            var entry = FindEntryById(messageDB, messageId);
            if (entry == null) { Warn($"Id not found: '{messageId}'"); return; }

            bool oneShot = oneShotPolicy == TriBool.UseDB ? entry.oneShot : (oneShotPolicy == TriBool.ForceTrue);
            string persistKey = !string.IsNullOrEmpty(persistentIdOverride) ? persistentIdOverride : (string.IsNullOrEmpty(entry.persistentIdOverride) ? messageId : entry.persistentIdOverride);
            if (oneShot && !string.IsNullOrEmpty(persistKey))
            {
                if (PlayerPrefs.GetInt(PP_BannerPrefix + persistKey, 0) == 1) return;
            }

            // Cooldown (local override or global default)
            float cd = cooldownSeconds;
            if (cd < 0f)
            {
                var gs = MessageRuntimeSettingsProvider.Get();
                if (gs != null) cd = gs.globalCooldownSeconds;
            }
            if (cd > 0f && Time.time - _lastFireTime < cd) return;

            // Resolve channel
            var chan = channel;
            if (chan == ChannelPolicy.Auto)
            {
                chan = (entry.type == MessageType.Oniric) ? ChannelPolicy.Oniric : ChannelPolicy.Lower;
            }

            // Resolve key
            string key = string.IsNullOrWhiteSpace(entry.key) ? $"msg.{(entry.type == MessageType.Lower ? "lower" : "oniric")}.{messageId}" : entry.key;
            // Defaults with global fallback
            float autoClose = (autoCloseSecondsOverride >= 0f) ? autoCloseSecondsOverride :
                              (entry.autoCloseSeconds >= 0f ? entry.autoCloseSeconds : (MessageRuntimeSettingsProvider.Get()?.defaultAutoCloseSeconds ?? -1f));
            float cps = (typewriterCpsOverride > 0f) ? typewriterCpsOverride :
                        (entry.typewriterCps > 0f ? entry.typewriterCps : (MessageRuntimeSettingsProvider.Get()?.defaultTypewriterCps ?? -1f));

            if (chan == ChannelPolicy.Oniric)
            {
                if (oniricOverlay == null) { Warn("No OniricOverlayController set"); return; }
                oniricOverlay.ShowKey(key, entry.audio, autoClose, persistKey, oneShot);
            }
            else
            {
                if (lowerMessage == null) { Warn("No LowerMessageController set"); return; }
                bool repeatable = true; // Simplificado: siempre repetible (gating por cooldown/persistencia)
                if (cps > 0f) lowerMessage.EnqueueKey(key, entry.audio, autoClose, repeatable, cps);
                else lowerMessage.EnqueueKey(key, entry.audio, autoClose, repeatable);
            }

            _firedRuntime = true;
            _lastFireTime = Time.time;
            if (oneShot && !string.IsNullOrEmpty(persistKey))
            {
                PlayerPrefs.SetInt(PP_BannerPrefix + persistKey, 1);
                PlayerPrefs.Save();
            }
        }

        private static bool ContainsReason(System.Collections.Generic.IReadOnlyCollection<PauseReason> set, PauseReason r)
        {
            if (set == null) return false; foreach (var it in set) { if (it == r) return true; } return false;
        }

        private static MessageDBSO.Entry FindEntryById(MessageDBSO db, string id)
        {
            if (!db || string.IsNullOrEmpty(id)) return null;
            var m = typeof(MessageDBSO).GetMethod("FindById", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (m != null) { return m.Invoke(db, new object[] { id }) as MessageDBSO.Entry; }
            var field = typeof(MessageDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = field?.GetValue(db) as List<MessageDBSO.Entry>;
            if (list == null) return null;
            for (int i = 0; i < list.Count; i++) { var e = list[i]; if (e == null) continue; if (string.Equals(e.id, id, StringComparison.Ordinal)) return e; }
            return null;
        }

        private void Warn(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.LogWarning($"[DBTextInteractable] {msg}");
            #endif
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Interactuable único que resuelve messageId→key en MessageDB y encola Lower/Oníric; incluye outline opcional.
// RelatedScripts: PlayerController.PlayerInteraction, Game.UI.LowerMessageController, Game.UI.OniricOverlayController
// UsesSO: Sí (MessageDBSO). Localization la resuelven los controladores vía EnqueueKey/ShowKey.
// ReceivesFrom: PlayerInteraction.Interact o Collider trigger
// SendsTo: LowerMessageController.EnqueueKey / OniricOverlayController.ShowKey
// Adjuntar a: Cualquier objeto investigable. Asignar MessageDB y controladores en Inspector. Outline material opcional.
