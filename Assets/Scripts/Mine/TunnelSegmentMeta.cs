using UnityEngine;

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

        public Transform EntryAnchor => entryAnchor;
        public Transform ExitAnchor => exitAnchor;
        public Transform FaceSocket => faceSocket != null ? faceSocket : exitAnchor; // fallback
        public float Length => length;
        public Vector3 Forward => forward;
        public float YawDelta => yawDelta;

        private void OnValidate()
        {
            if (entryAnchor == null || exitAnchor == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[TunnelSegmentMeta] Missing anchors on '{name}'");
                #endif
                return;
            }

            // Compute basic values
            length = Vector3.Distance(entryAnchor.position, exitAnchor.position);
            Vector3 dir = (exitAnchor.position - entryAnchor.position);
            forward = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;

            // Yaw (project onto world up plane)
            Vector3 projected = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (projected.sqrMagnitude > 0.0001f)
            {
                projected.Normalize();
                // Angle from world forward (or we could use entryAnchor.forward). Choose entryAnchor for local continuity.
                Vector3 refForward = entryAnchor.forward;
                refForward = Vector3.ProjectOnPlane(refForward, Vector3.up);
                if (refForward.sqrMagnitude < 0.0001f) refForward = Vector3.forward;
                refForward.Normalize();
                yawDelta = Vector3.SignedAngle(refForward, projected, Vector3.up);
            }
            else
            {
                yawDelta = 0f;
            }

            // Soft warnings
            if (length <= 0f)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[TunnelSegmentMeta] Length is zero on '{name}'. Check anchor placement.");
                #endif
            }
            if (length < suggestedLengthRange.x || length > suggestedLengthRange.y)
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
            OnValidate();
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
