using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Mine
{
    /// <summary>
    /// Agrupa múltiples RockNode y notifica cuando todos han sido destruidos (gating de capa).
    /// Puede auto-recoger nodos hijos si no se listan manualmente. KISS: no pooling interno.
    /// </summary>
    [AddComponentMenu("Spookie/Mine/Mining Cluster")]
    public class MiningCluster : MonoBehaviour
    {
        [Header("Nodes (Inspector)")]
        [Tooltip("Lista de RockNode del cluster. Si está vacía, se rellenará automáticamente en Awake con hijos.")]
        [SerializeField] private List<RockNode> nodes = new();
        [Tooltip("Si true, en Awake buscará RockNode en hijos solamente si la lista está vacía.")]
        [SerializeField] private bool autoCollectIfEmpty = true;

        [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        private int aliveCount;
        private bool cleared;

        // Eventos
        public event Action<MiningCluster> OnClusterCleared; // cuando todas las rocas se destruyen
        public event Action<RockNode, int, ResourceType> OnRockDestroyedForward; // re-emite cada roca destruida

        public bool IsCleared => cleared;
        public IReadOnlyList<RockNode> Nodes => nodes;
        public int AliveCount => aliveCount;

        private void Awake()
        {
            if ((nodes == null || nodes.Count == 0) && autoCollectIfEmpty)
            {
                nodes = new List<RockNode>(GetComponentsInChildren<RockNode>(includeInactive: true));
            }
            aliveCount = 0;
            foreach (var n in nodes)
            {
                if (n == null) continue;
                if (!n.IsDestroyed) aliveCount++;
                n.OnRockDestroyed += HandleRockDestroyed;
            }
            Log($"Cluster init: {aliveCount} rocks");
            cleared = aliveCount == 0;
            if (cleared) OnClusterCleared?.Invoke(this);
        }

        private void OnDestroy()
        {
            // Limpieza de suscripciones
            foreach (var n in nodes)
            {
                if (n != null) n.OnRockDestroyed -= HandleRockDestroyed;
            }
        }

        private void HandleRockDestroyed(RockNode node, int amount, ResourceType type)
        {
            if (amount < 0) amount = 0;
            OnRockDestroyedForward?.Invoke(node, amount, type);
            aliveCount = Mathf.Max(0, aliveCount - 1);
            Log($"Rock destroyed, remaining={aliveCount}");
            if (!cleared && aliveCount == 0)
            {
                cleared = true;
                Log("Cluster CLEARED.");
                OnClusterCleared?.Invoke(this);
            }
        }

        public void ForceMarkCleared()
        {
            if (cleared) return;
            cleared = true;
            aliveCount = 0;
            OnClusterCleared?.Invoke(this);
        }

        private void Log(string msg)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.Log($"[MiningCluster] {msg}");
            #endif
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Gestiona un conjunto de RockNode y avisa cuando todos se han destruido.
// RelatedScripts: RockNode, (futuro) TunnelManager (para avanzar), ResourceInventory.
// UsesSO: No.
// ReceivesFrom: RockNode (eventos OnRockDestroyed).
// SendsTo: TunnelManager (futuro gating), sistema de recursos (re-emisión de destrucciones).
