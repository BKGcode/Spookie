using UnityEngine;
using System;

namespace Game.Mine
{
    /// <summary>
    /// Nodo destructible (una roca). Al recibir daño y llegar a 0 HP, se destruye y notifica.
    /// No gestiona drops físicos (KISS); suma de recursos se hará vía evento.
    /// </summary>
    [AddComponentMenu("Spookie/Mine/Rock Node")]
    public class RockNode : MonoBehaviour
    {
        [Header("Stats")]
        [Tooltip("Vida máxima de la roca (golpes aprox. = MaxHP / DañoArma.")]
        [Min(1)] [SerializeField] private int maxHP = 10;
        [Tooltip("Tipo de recurso que otorga al destruirla.")]
        [SerializeField] private ResourceType resourceType = ResourceType.Stone;
        [Tooltip("Rango de cantidad de recurso obtenido al destruir (min inclusive, max inclusive).")]
        [Min(0)] [SerializeField] private int amountMin = 1;
        [Min(0)] [SerializeField] private int amountMax = 2;

        [Header("Optional Visual/SFX Refs")]
        [Tooltip("Objeto visual a desactivar al morir si no quieres destruir la raíz.")]
        [SerializeField] private GameObject visualsRoot;
        [Tooltip("Efecto (prefab) instanciado al destruir (opcional). Mantener simple; se podría pool en futuro.")]
        [SerializeField] private GameObject destroyVFXPrefab;

        [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        // Runtime
        private int currentHP;
        private bool destroyed;

        // Events
        public event Action<RockNode> OnRockDamaged; // before death
        public event Action<RockNode, int, ResourceType> OnRockDestroyed; // amount, resourceType

        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public bool IsDestroyed => destroyed;
        public ResourceType Type => resourceType;

        private void Awake()
        {
            currentHP = Mathf.Max(1, maxHP);
        }

        private void OnValidate()
        {
            if (amountMax < amountMin) amountMax = amountMin;
            maxHP = Mathf.Max(1, maxHP);
        }

        /// <summary>
        /// Aplica daño directo (no negativo). Devuelve HP restante tras aplicar.
        /// </summary>
        public int ApplyDamage(int damage)
        {
            if (destroyed) return 0;
            if (damage <= 0) return currentHP;
            currentHP -= damage;
            if (currentHP > 0)
            {
                OnRockDamaged?.Invoke(this);
                if (showDebugLogs) Debug.Log($"[RockNode] Damaged '{name}' HP={currentHP}/{maxHP}");
                return currentHP;
            }
            currentHP = 0;
            HandleDestroyed();
            return 0;
        }

        private void HandleDestroyed()
        {
            if (destroyed) return;
            destroyed = true;
            int amount = UnityEngine.Random.Range(amountMin, amountMax + 1);
            OnRockDestroyed?.Invoke(this, amount, resourceType);
            if (showDebugLogs) Debug.Log($"[RockNode] Destroyed '{name}' -> {amount} {resourceType}");

            if (destroyVFXPrefab != null)
            {
                try { Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity); } catch { }
            }
            if (visualsRoot != null) visualsRoot.SetActive(false);
            else gameObject.SetActive(false); // KISS: desactivar en lugar de Destroy para posible pooling futuro
        }

        // Utilidad para restaurar (p.ej. al cargar partida o reutilizar en pooling)
        public void ResetNode(int? overrideMaxHP = null)
        {
            maxHP = overrideMaxHP.HasValue ? Mathf.Max(1, overrideMaxHP.Value) : maxHP;
            currentHP = maxHP;
            destroyed = false;
            if (visualsRoot != null) visualsRoot.SetActive(true); else gameObject.SetActive(true);
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Representa una roca destructible con HP y tipo de recurso.
// RelatedScripts: MiningCluster, (futuro) ResourceInventory.
// UsesSO: No (posible futuro ResourceConfigSO).
// ReceivesFrom: Herramienta/arma (raycast) -> ApplyDamage.
// SendsTo: MiningCluster (eventos), sistema de recursos (listener a OnRockDestroyed).
