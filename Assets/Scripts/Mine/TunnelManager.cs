using System.Collections.Generic;
using UnityEngine;

namespace Game.Mine
{
    /// <summary>
    /// Runtime controller for spawning and recycling tunnel segments with variable length and curvature.
    /// Versión ajustada: ya no depende del jugador ni de triggers; genera una cadena hacia delante
    /// partiendo de la posición/rotación del propio TunnelManager. Mantiene un número objetivo de
    /// segmentos vivos (ventana) y permite spawns adicionales manuales o automáticos.
    /// </summary>
    [AddComponentMenu("Spookie/Mine/Tunnel Manager")]
    public class TunnelManager : MonoBehaviour
    {
    [Header("Segment Prefabs (assign)")]
        [Tooltip("Variantes de segmentos disponibles. Se elige una al generar el siguiente.")]
        [SerializeField] private List<TunnelSegmentMeta> segmentVariants = new();
        
    [Header("Spawn Settings (Forward Chain)")]
    [Tooltip("Número inicial de segmentos a generar al arrancar (incluye el primero).")]
    [SerializeField] private int initialSegments = 3;
    [Tooltip("Número máximo de segmentos vivos (ventana). Se reciclan los más antiguos una vez se excede.")]
    [SerializeField] private int maxSegmentsInMemory = 8;
    [Tooltip("Si está activo, el manager mantendrá siempre al menos 'targetActiveSegments' generados (útil para entornos continuos).")]
    [SerializeField] private bool autoMaintainTarget = true;
    [Tooltip("Cantidad objetivo de segmentos vivos que se intenta mantener (<= maxSegmentsInMemory).")]
    [SerializeField] private int targetActiveSegments = 5;
    [Tooltip("Forzar reintentos si una variante excede el límite de yaw acumulada.")]
    [SerializeField] private int curvaturePickSafetyIterations = 25;

    [Header("Curvature Limits (Yaw Only)")]
        [Tooltip("Máximo acumulado (absoluto) de yaw permitido (para evitar giros extremos). 0 = sin límite.")]
        [SerializeField] private float maxAbsoluteAccumulatedYaw = 0f;

    [Header("Randomization")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int fixedSeed = 12345;

    [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool logSpawnDecisions = false;

        private readonly List<TunnelSegmentMeta> activeSegments = new();
        private System.Random rng;
        private float accumulatedYaw = 0f;

        private void Awake()
        {
            rng = useRandomSeed ? new System.Random() : new System.Random(fixedSeed);
        }

        private void Start()
        {
            if (segmentVariants == null || segmentVariants.Count == 0)
            {
                Debug.LogError("[TunnelManager] No segment variants assigned.");
                enabled = false;
                return;
            }
            SpawnInitial();
        }

        private void Update()
        {
            if (autoMaintainTarget)
            {
                EnsureTargetCount();
            }
        }

        private void SpawnInitial()
        {
            int count = Mathf.Max(1, initialSegments);
            for (int i = 0; i < count; i++)
            {
                InternalSpawnNext(i == 0);
                if (activeSegments.Count >= maxSegmentsInMemory) break; // safety
            }
            EnsureTargetCount();
        }

        private void EnsureTargetCount()
        {
            int desired = Mathf.Clamp(targetActiveSegments, 1, maxSegmentsInMemory);
            while (activeSegments.Count < desired)
            {
                InternalSpawnNext(activeSegments.Count == 0);
                if (activeSegments.Count >= maxSegmentsInMemory) break;
            }
        }

        /// <summary>
        /// Public manual spawn (e.g., desde editor o tras evento de minado) respetando límites.
        /// </summary>
        public void SpawnNext()
        {
            InternalSpawnNext(activeSegments.Count == 0);
            EnsureTargetCount(); // optional top-up if below target
        }

        private void InternalSpawnNext(bool first)
        {
            var variant = PickVariant();
            if (variant == null) return;
            var inst = Instantiate(variant, transform);
            if (first)
            {
                // Alinear primer segmento a la posición/rotación del manager (entryAnchor -> manager transform)
                inst.AlignEntryTo(transform);
                accumulatedYaw = 0f;
                Log($"Spawned initial segment '{inst.name}' len={inst.Length:F2}");
            }
            else
            {
                var last = GetLast();
                if (last != null)
                {
                    inst.AlignEntryTo(last.ExitAnchor);
                    accumulatedYaw += inst.YawDelta;
                    Log($"Spawn next '{inst.name}' len={inst.Length:F2} yawΔ={inst.YawDelta:F1} accYaw={accumulatedYaw:F1}");
                }
            }
            activeSegments.Add(inst);
            CullOldSegments();
        }

        private TunnelSegmentMeta PickVariant()
        {
            if (segmentVariants.Count == 0) return null;
            // Simple random pick with optional curvature constraint
            int safety = 0;
            int maxIter = Mathf.Max(1, curvaturePickSafetyIterations);
            while (safety < maxIter)
            {
                int idx = rng.Next(0, segmentVariants.Count);
                var cand = segmentVariants[idx];
                float prospective = maxAbsoluteAccumulatedYaw <= 0f ? 0f : Mathf.Abs(accumulatedYaw + cand.YawDelta);
                if (maxAbsoluteAccumulatedYaw <= 0f || prospective <= maxAbsoluteAccumulatedYaw)
                {
                    if (logSpawnDecisions) Log($"Pick variant idx={idx} yawΔ={cand.YawDelta:F1} prospectiveAcc={accumulatedYaw + cand.YawDelta:F1}");
                    return cand;
                }
                safety++;
            }
            // Fallback: ignore limit after safety attempts
            var fallback = segmentVariants[0];
            if (logSpawnDecisions) Log("Fallback variant due to curvature limit.");
            return fallback;
        }

        private void CullOldSegments()
        {
            while (activeSegments.Count > Mathf.Max(1, maxSegmentsInMemory))
            {
                var seg = activeSegments[0];
                activeSegments.RemoveAt(0);
                if (seg != null)
                {
                    Destroy(seg.gameObject);
                }
            }
        }

        private TunnelSegmentMeta GetLast() => activeSegments.Count > 0 ? activeSegments[^1] : null;
        private TunnelSegmentMeta GetSecondLast() => activeSegments.Count > 1 ? activeSegments[^2] : null;

    // NOTE: Trigger system eliminado para esta versión basada solo en generación hacia adelante.

        private void Log(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.Log($"[TunnelManager] {msg}");
            #endif
        }

        // Debug utility to force spawn next segment from inspector context menu
        [ContextMenu("Force Spawn Next (Debug)")]
        private void ForceSpawnNextContextMenu()
        {
            SpawnNext();
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Gestiona el spawn incremental de segmentos de túnel (ventana, curvatura básica) basándose en la posición del manager.
// RelatedScripts: TunnelSegmentMeta, (futuro) Cluster/Mining.
// UsesSO: No en esta fase.
// ReceivesFrom: (futuro) eventos de capa limpia o UI para SpawnNext.
// SendsTo: Instanciación de prefabs de segmentos.
