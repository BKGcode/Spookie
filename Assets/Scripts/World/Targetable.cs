using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class Targetable : MonoBehaviour
    {
        // Static event to notify when any target becomes occupied.
        public static event Action<Targetable> OnTargetOccupied;

        // Static list to keep track of all available targets
        private static readonly List<Targetable> allTargetables = new List<Targetable>();

        public bool IsOccupied { get; private set; }
        public event Action<bool> OnOccupancyChanged;

        private void OnEnable()
        {
            // Register this target as available when it's enabled
            if (!IsOccupied)
            {
                allTargetables.Add(this);
            }
            // Debug.Log($"{name} is now available as a target.");
        }

        private void OnDisable()
        {
            // Unregister this target when it's disabled
            allTargetables.Remove(this);
            // Debug.Log($"{name} is no longer available as a target.");
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
                allTargetables.Remove(this);
                OnTargetOccupied?.Invoke(this); // Notify all listeners that this specific target is now taken
                // Debug.Log($"{name} is now OCCUPIED.");
            }
            else
            {
                allTargetables.Add(this);
                // Debug.Log($"{name} is now FREE.");
            }
        }

        /// <summary>
        /// Finds the closest available target to a given position.
        /// </summary>
        /// <param name="position">The position to find the closest target to.</param>
        /// <returns>The closest Targetable component, or null if none are available.</returns>
        public static Targetable FindClosest(Vector3 position, HashSet<Targetable> blacklist)
        {
            Targetable bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;

            allTargetables.RemoveAll(item => item == null); // Clean up destroyed targets

            foreach (var target in allTargetables)
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
                    bestTarget = target;
                }
            }

            return bestTarget;
        }
    }
}

// ScriptRole: Marks an object as a potential target for AI, managing its occupied/free state.
// TriggersEvents: OnOccupancyChanged(bool), OnTargetOccupied(Targetable)
// NeedsSetup: Attach to any GameObject that should be interactable (e.g., rocks, beds). 