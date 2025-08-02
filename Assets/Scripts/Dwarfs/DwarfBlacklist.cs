using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Core.Shared;
using World;

namespace Dwarfs
{
    /// <summary>
    /// Gestiona el sistema de blacklisting de objetivos para los enanos.
    /// </summary>
    public class DwarfBlacklist
    {
        private readonly Dictionary<Targetable, BlacklistEntry> _blacklistedTargets = new Dictionary<Targetable, BlacklistEntry>();
        private readonly Dictionary<Targetable, int> _failedPathAttempts = new Dictionary<Targetable, int>();
        private readonly float _blacklistCooldown;
        private readonly int _maxPathAttempts;
        private readonly string _dwarfName;

        private class BlacklistEntry
        {
            public float ExpirationTime { get; set; }
            public string Reason { get; set; }
            public int FailCount { get; set; }

            public BlacklistEntry(float expirationTime, string reason, int failCount = 1)
            {
                ExpirationTime = expirationTime;
                Reason = reason;
                FailCount = failCount;
            }
        }

        public DwarfBlacklist(float blacklistCooldown, int maxPathAttempts, string dwarfName)
        {
            _blacklistCooldown = blacklistCooldown;
            _maxPathAttempts = maxPathAttempts;
            _dwarfName = dwarfName;
        }

        /// <summary>
        /// Registra un intento fallido de pathfinding para un objetivo.
        /// </summary>
        public void RegisterPathfindingFailure(Targetable target)
        {
            if (target == null) return;

            if (!_failedPathAttempts.ContainsKey(target))
                _failedPathAttempts[target] = 0;
            
            _failedPathAttempts[target]++;
            Debug.LogWarning($"{_dwarfName} failed to find path to {target.name}. Attempt {_failedPathAttempts[target]}/{_maxPathAttempts}.");

            if (_failedPathAttempts[target] >= _maxPathAttempts)
            {
                BlacklistTarget(target, $"Failed to reach after {_maxPathAttempts} attempts", _failedPathAttempts[target]);
                _failedPathAttempts.Remove(target);
            }
        }

        /// <summary>
        /// Añade un objetivo a la lista negra.
        /// </summary>
        public void BlacklistTarget(Targetable target, string reason, int failCount = 1)
        {
            if (target == null) return;

            float expirationTime = Time.time + (_blacklistCooldown * failCount);
            _blacklistedTargets[target] = new BlacklistEntry(expirationTime, reason, failCount);
            
            Debug.LogWarning($"{_dwarfName} blacklisted {target.name} until {expirationTime:F1}s. Reason: {reason}");
        }

        /// <summary>
        /// Obtiene todos los objetivos en la lista negra.
        /// </summary>
        public HashSet<Targetable> GetBlacklistedTargets()
        {
            PruneExpiredEntries();
            return new HashSet<Targetable>(_blacklistedTargets.Keys);
        }

        /// <summary>
        /// Verifica si un objetivo está en la lista negra.
        /// </summary>
        public bool IsBlacklisted(Targetable target, out string reason)
        {
            reason = string.Empty;
            if (target == null) return false;

            PruneExpiredEntries();
            if (_blacklistedTargets.TryGetValue(target, out var entry))
            {
                reason = entry.Reason;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Limpia todas las entradas de la lista negra.
        /// </summary>
        public void Clear()
        {
            _blacklistedTargets.Clear();
            _failedPathAttempts.Clear();
            Debug.Log($"{_dwarfName} cleared blacklist.");
        }

        /// <summary>
        /// Elimina un objetivo específico de la lista negra.
        /// </summary>
        public void RemoveTarget(Targetable target)
        {
            if (target == null) return;

            if (_blacklistedTargets.Remove(target))
            {
                Debug.Log($"{_dwarfName} removed {target.name} from blacklist.");
            }
            _failedPathAttempts.Remove(target);
        }

        private void PruneExpiredEntries()
        {
            var expiredTargets = _blacklistedTargets
                .Where(kvp => Time.time > kvp.Value.ExpirationTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var target in expiredTargets)
            {
                Debug.Log($"{_dwarfName} removed expired blacklist entry for {target.name}.");
                _blacklistedTargets.Remove(target);
            }
        }
    }
}

// ScriptRole: Gestiona el sistema de blacklisting de objetivos para los enanos
// Dependencies: None
// RelatedScripts: DwarfBrain, Targetable
// NeedsSetup: None - Used internally by DwarfBrain