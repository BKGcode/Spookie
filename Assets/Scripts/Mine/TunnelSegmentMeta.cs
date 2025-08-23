using UnityEngine;
using System.Collections.Generic; // List<>
#if UNITY_EDITOR
using UnityEditor; // PrefabUtility + context menu utilities (editor-only guarded)
#endif

namespace Game.Mine
{
    /// <summary>
    /// Metadata wrapper for a tunnel segment prefab. Holds anchor references and precomputed geometric info
    /// so the manager does not rely on names or repeated calculations. Face/cluster integration comes later.
    /// KISS: only length + forward + yawDelta now (yaw projected on global up).
    /// </summary>
    [AddComponentMenu("Spookie/Mine/Tunnel Segment Meta")]
    public class TunnelSegmentMeta : MonoBehaviour
    {
        [Header("Anchors (assign in prefab)")]
        [Tooltip("Entry anchor transform (start of segment). Used as alignment origin when spawning.")]
        [SerializeField] private Transform entryAnchor;
        [Tooltip("Exit anchor transform (end of segment). New segments align their entry to this.")]
        [SerializeField] private Transform exitAnchor;
        [Tooltip("Optional socket where the mining face/cluster prefab will be parented.")]
        [SerializeField] private Transform faceSocket;

    [Header("Layer Sockets (Internal Mining)")]
    [Tooltip("Si está activo, se generan sockets internos automatizados para capas Active/Preview dentro del segmento.")]
    [SerializeField] private bool autoGenerateLayerSockets = true;
    [Tooltip("Distancia desde el EntryAnchor al primer socket.")]
    [Min(0f)] [SerializeField] private float firstLayerOffset = 0.8f;
    [Tooltip("Margen antes del ExitAnchor donde ya no se colocan sockets.")]
    [Min(0f)] [SerializeField] private float endClearance = 0.6f;
    [Tooltip("Espaciado deseado entre sockets consecutivos (Active→Preview).")]
    [Min(0.1f)] [SerializeField] private float desiredLayerSpacing = 2.5f;
    [Tooltip("Si está activo, ajusta ligeramente el spacing para que el último socket encaje de forma uniforme (siempre que quepan ≥2).")]
    [SerializeField] private bool adaptiveFit = false;
    [Tooltip("Opcional: sockets definidos manualmente (si se asigna alguno y autoGenerateLayerSockets está desactivado, se usarán estos).")]
    [SerializeField] private List<Transform> manualLayerSockets = new List<Transform>();
    [Tooltip("Gizmos para sockets generados.")]
    [SerializeField] private Color socketsGizmoColor = new Color(1f, 0.65f, 0.2f, 0.75f);

        [Header("Debug / Validation")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.7f);
        [Tooltip("Warn if segment length is outside this (soft) range.")]
        [SerializeField] private Vector2 suggestedLengthRange = new Vector2(6f, 14f);

        // Cached computed data
        [SerializeField, Tooltip("Computed length (Entry->Exit). Read-only.")]
        private float length = 0f; // kept serialized for quick inspector view
        private Vector3 forward = Vector3.forward;
        private float yawDelta = 0f; // degrees relative to entry forward vs segment root forward after alignment

    // Runtime / generated
    private readonly List<Transform> runtimeLayerSockets = new List<Transform>();

        public Transform EntryAnchor => entryAnchor;
        public Transform ExitAnchor => exitAnchor;
        public Transform FaceSocket => faceSocket != null ? faceSocket : exitAnchor; // fallback
        public float Length => length;
        public Vector3 Forward => forward;
        public float YawDelta => yawDelta;

        private void OnValidate()
        {
            RecomputeGeometry();
            // Marcamos suciedad de sockets pero NO regeneramos aquí (evita spam y errores de DestroyImmediate en OnValidate)
            if (autoGenerateLayerSockets)
            {
                socketsDirty = true; // visible solo conceptualmente; se regenera por botón o en runtime
            }
        }

        private void Awake()
        {
            // Ya NO generamos aquí porque el segmento todavía no ha sido alineado por el manager.
            // La generación se hará tras AlignEntryTo (o bajo demanda) para usar la longitud definitiva.
        }

        private void RecomputeGeometry()
        {
            if (entryAnchor == null || exitAnchor == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[TunnelSegmentMeta] Missing anchors on '{name}'");
                #endif
                return;
            }
            length = Vector3.Distance(entryAnchor.position, exitAnchor.position);
            Vector3 dir = (exitAnchor.position - entryAnchor.position);
            forward = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;
            Vector3 projected = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (projected.sqrMagnitude > 0.0001f)
            {
                projected.Normalize();
                Vector3 refForward = entryAnchor.forward;
                refForward = Vector3.ProjectOnPlane(refForward, Vector3.up);
                if (refForward.sqrMagnitude < 0.0001f) refForward = Vector3.forward;
                refForward.Normalize();
                yawDelta = Vector3.SignedAngle(refForward, projected, Vector3.up);
            }
            else yawDelta = 0f;

            if (length <= 0f)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[TunnelSegmentMeta] Length is zero on '{name}'. Check anchor placement.");
                #endif
            }
            else if (length < suggestedLengthRange.x || length > suggestedLengthRange.y)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[TunnelSegmentMeta] Length {length:F2} outside suggested range {suggestedLengthRange.x}-{suggestedLengthRange.y} on '{name}' (not fatal).");
                #endif
            }
        }

        /// <summary>
        /// Aligns this segment so that its entry anchor matches the target anchor's position/rotation.
        /// Does not rely on names; manager supplies the target explicitly.
        /// </summary>
        public void AlignEntryTo(Transform targetAnchor)
        {
            if (entryAnchor == null || targetAnchor == null) return;

            // Rotate first: compute delta from current entry rotation to target rotation
            Quaternion deltaRot = targetAnchor.rotation * Quaternion.Inverse(entryAnchor.rotation);
            transform.rotation = deltaRot * transform.rotation;
            // Then translate so entryAnchor coincides with target
            Vector3 offset = targetAnchor.position - entryAnchor.position;
            transform.position += offset;
            // Recompute afterwards for accuracy in case manager inspects
            RecomputeGeometry();
            if (autoGenerateLayerSockets && runtimeLayerSockets.Count == 0)
            {
                GenerateLayerSockets(false); // runtime generation post-alineación
            }
        }

        #region Layer Sockets Generation
        private bool socketsDirty = false; // sólo indicador en editor

        [ContextMenu("Regenerate Layer Sockets")] // Botón rápido en Inspector
        private void RegenerateLayerSocketsContext() { RegenerateLayerSocketsEditor(); }

        public void RegenerateLayerSocketsEditor()
        {
            if (!autoGenerateLayerSockets)
            {
                #if UNITY_EDITOR
                Debug.Log($"[TunnelSegmentMeta] AutoGenerate desactivado en '{name}'. Nada que regenerar.");
                #endif
                return;
            }
            GenerateLayerSockets(true);
            socketsDirty = false;
        }

    private void GenerateLayerSockets(bool editorContext)
        {
            runtimeLayerSockets.Clear();
            if (entryAnchor == null || exitAnchor == null) return;

            // Prefab asset guard (evita errors destruyendo assets dentro del Project)
            #if UNITY_EDITOR
            if (editorContext && PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                // No modificamos el asset directo; el usuario puede usar modo escena o instanciarlo para previsualizar.
                return;
            }
            #endif

            // Buscar/crear contenedor (solo si vamos a materializar transforms)
            Transform socketsParent = transform.Find("_LayerSockets");
            if (socketsParent == null)
            {
                GameObject p = new GameObject("_LayerSockets");
                p.transform.SetParent(transform, false);
                socketsParent = p.transform;
            }
            else
            {
                // Limpiar hijos previos de forma segura
                for (int i = socketsParent.childCount - 1; i >= 0; i--)
                {
                    var c = socketsParent.GetChild(i);
                    #if UNITY_EDITOR
                    if (editorContext && !Application.isPlaying) DestroyImmediate(c.gameObject);
                    else Destroy(c.gameObject);
                    #else
                    Destroy(c.gameObject);
                    #endif
                }
            }

            // Aseguramos geometría recalculada (por si se llama manualmente sin alineación previa)
            RecomputeGeometry();
            float usable = length - firstLayerOffset - endClearance;
            if (usable <= 0.1f || desiredLayerSpacing <= 0.0001f)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[TunnelSegmentMeta] No sockets (usable={usable:F2}, spacing={desiredLayerSpacing:F2}) en '{name}'. Comprueba anchors y offsets.");
                #endif
                return;
            }
            int count = Mathf.FloorToInt(usable / desiredLayerSpacing) + 1;
            if (count < 1) return;
            float spacing = (adaptiveFit && count > 1) ? usable / (count - 1) : desiredLayerSpacing;
            Vector3 basePos = entryAnchor.position;
            Vector3 dir = forward;
            for (int i = 0; i < count; i++)
            {
                float dist = firstLayerOffset + i * spacing;
                Vector3 pos = basePos + dir * dist;
                float distToExit = Vector3.Dot((exitAnchor.position - pos), dir);
                if (distToExit < endClearance - 0.01f) break;
                GameObject g = new GameObject($"LayerSocket_{i}");
                g.transform.position = pos;
                g.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                g.transform.SetParent(socketsParent, true);
                runtimeLayerSockets.Add(g.transform);
            }
        }

        public int GetLayerSocketCount()
        {
            if (!autoGenerateLayerSockets && manualLayerSockets.Count > 0)
                return manualLayerSockets.Count;
            return runtimeLayerSockets.Count;
        }

        public Transform GetLayerSocket(int index)
        {
            if (!autoGenerateLayerSockets && manualLayerSockets.Count > 0)
            {
                if (index < 0 || index >= manualLayerSockets.Count) return null;
                return manualLayerSockets[index];
            }
            if (index < 0 || index >= runtimeLayerSockets.Count) return null;
            return runtimeLayerSockets[index];
        }
        #endregion

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            // Draw sockets too
            Gizmos.color = socketsGizmoColor;
            int c = GetLayerSocketCount();
            for (int i = 0; i < c; i++)
            {
                var t = GetLayerSocket(i);
                if (t == null) continue;
                Gizmos.DrawSphere(t.position, 0.15f);
                Gizmos.DrawRay(t.position, t.forward * 0.5f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos || entryAnchor == null || exitAnchor == null) return;
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(entryAnchor.position, exitAnchor.position);
            // Arrow at exit
            Vector3 mid = Vector3.Lerp(entryAnchor.position, exitAnchor.position, 0.5f);
            Gizmos.DrawSphere(entryAnchor.position, 0.1f);
            Gizmos.DrawSphere(exitAnchor.position, 0.1f);
            Gizmos.DrawRay(mid, forward * 0.8f);
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Guarda metadatos de un segmento de túnel (anchors, longitud, yaw) y permite alineación exacta.
// RelatedScripts: TunnelManager, AdvanceTrigger (creado dinámicamente)
// UsesSO: No
// ReceivesFrom: TunnelManager (para alineación)
// SendsTo: (futuro) Cluster/face spawning mediante FaceSocket
