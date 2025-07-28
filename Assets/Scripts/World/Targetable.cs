using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class Targetable : MonoBehaviour
    {
        // Static list to keep track of all available targets
        private static readonly List<Targetable> AvailableTargets = new List<Targetable>();

        public bool IsOccupied { get; private set; }
        public event Action<bool> OnOccupancyChanged;

        private void OnEnable()
        {
            // Register this target as available when it's enabled
            if (!IsOccupied)
            {
                AvailableTargets.Add(this);
            }
            Debug.Log($"{name} is now available as a target.");
        }

        private void OnDisable()
        {
            // Unregister this target when it's disabled
            AvailableTargets.Remove(this);
            Debug.Log($"{name} is no longer available as a target.");
        }

        /// <summary>
        /// Sets the occupancy state of this target and notifies listeners.
        /// </summary>
        /// <param name="isOccupied">The new occupancy state.</param>
        public void SetOccupancy(bool isOccupied)
        {
            if (IsOccupied == isOccupied) return;

            IsOccupied = isOccupied;
            OnOccupancyChanged?.Invoke(IsOccupied);

            if (IsOccupied)
            {
                AvailableTargets.Remove(this);
                Debug.Log($"{name} is now OCCUPIED.");
            }
            else
            {
                AvailableTargets.Add(this);
                Debug.Log($"{name} is now FREE.");
            }
        }

        /// <summary>
        /// Finds the closest available target to a given position.
        /// </summary>
        /// <param name="position">The position to find the closest target to.</param>
        /// <returns>The transform of the closest target, or null if none are available.</returns>
        public static Transform FindClosest(Vector3 position)
        {
            return FindClosest(position, null);
        }

        public static Transform FindClosest(Vector3 position, HashSet<Targetable> blacklist)
        {
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var target in AvailableTargets)
            {
                if (blacklist != null && blacklist.Contains(target))
                {
                    continue; // Skip blacklisted targets
                }

                Vector3 directionToTarget = target.transform.position - position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = target.transform;
                }
            }

            return bestTarget;
        }
    }
}

// ScriptRole: Marks an object as a potential target for AI, managing its occupied/free state.
// TriggersEvents: OnOccupancyChanged(bool).
// NeedsSetup: Attach to any GameObject that should be interactable (e.g., rocks, beds). 