using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Shared;

namespace World
{
    public class Targetable : MonoBehaviour, ITargetable
    {
        // Static events to notify when any target changes state
        public static event Action<Targetable, GameObject> OnTargetOccupied;
        public static event Action<Targetable> OnTargetFreed;
        public static event Action<Targetable, GameObject, GameObject> OnTargetTransferred; // oldOccupier, newOccupier

        // Static list to keep track of all available targets
        private static readonly List<Targetable> allTargetables = new List<Targetable>();

        [Header("Targeting Settings")]
        [Tooltip("Is this target a valid work objective for autonomous dwarfs?")]
        [SerializeField] private bool isWorkable = true;
        [Tooltip("When finding the closest target, how many of the top candidates should be considered for a random pick?")]
        [SerializeField, Range(1, 10)] private int randomCandidateCount = 3;

        public bool IsOccupied { get; private set; }
        public GameObject OccupiedBy { get; private set; }
        public event Action<bool> OnOccupancyChanged;

        private void OnEnable()
        {
            // Register this target as available when it's enabled
            if (!IsOccupied)
            {
                allTargetables.Add(this);
            }
        }

        private void OnDisable()
        {
            // Unregister this target when it's disabled
            allTargetables.Remove(this);
        }

        /// <summary>
        /// Sets the occupancy state of this target and notifies listeners.
        /// </summary>
        /// <param name="isOccupied">The new occupancy state.</param>
        /// <param name="occupier">The GameObject that is occupying this target. Can be null if freeing it.</param>
        public void SetOccupancy(bool isOccupied, GameObject occupier = null)
        {
            if (IsOccupied == isOccupied && OccupiedBy == occupier) return;

            GameObject previousOccupier = OccupiedBy;
            bool wasOccupied = IsOccupied;

            IsOccupied = isOccupied;
            OccupiedBy = isOccupied ? occupier : null;
            OnOccupancyChanged?.Invoke(IsOccupied);

            // Caso 1: El objetivo estaba libre y ahora está ocupado
            if (!wasOccupied && IsOccupied)
            {
                allTargetables.Remove(this);
                OnTargetOccupied?.Invoke(this, occupier);
                Debug.Log($"{name} is now OCCUPIED by {occupier?.name}.");
            }
            // Caso 2: El objetivo estaba ocupado y ahora está libre
            else if (wasOccupied && !IsOccupied)
            {
                allTargetables.Add(this);
                OnTargetFreed?.Invoke(this);
                Debug.Log($"{name} is now FREE (was occupied by {previousOccupier?.name}).");
            }
            // Caso 3: El objetivo cambia de ocupante
            else if (wasOccupied && IsOccupied && previousOccupier != occupier)
            {
                OnTargetTransferred?.Invoke(this, previousOccupier, occupier);
                Debug.Log($"{name} transferred from {previousOccupier?.name} to {occupier?.name}.");
            }
        }

        public bool IsOccupiedBy(GameObject potentialOccupier)
        {
            return IsOccupied && OccupiedBy == potentialOccupier;
        }

        /// <summary>
        /// Finds the closest available and workable target to a given position.
        /// </summary>
        /// <param name="position">The position to find the closest target to.</param>
        /// <returns>The closest workable Targetable component, or null if none are available.</returns>
        public static Targetable FindClosest(Vector3 position, HashSet<Targetable> blacklist)
        {
            List<(Targetable target, float distanceSqr)> candidates = new List<(Targetable, float)>();

            allTargetables.RemoveAll(item => item == null); // Clean up destroyed targets

            foreach (var target in allTargetables)
            {
                if (blacklist != null && blacklist.Contains(target) || !target.isWorkable)
                {
                    continue; // Skip blacklisted or non-workable targets
                }

                Vector3 directionToTarget = target.transform.position - position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                candidates.Add((target, dSqrToTarget));
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            // Sort candidates by distance
            candidates.Sort((a, b) => a.distanceSqr.CompareTo(b.distanceSqr));

            // Determine the range of candidates to choose from
            int range = Mathf.Min(candidates[0].target.randomCandidateCount, candidates.Count);
            
            // Pick a random target from the top candidates
            int randomIndex = UnityEngine.Random.Range(0, range);
            
            return candidates[randomIndex].target;
        }
    }
}

// ScriptRole: Marks an object as a potential target for AI, managing its occupied/free state and workability.
// TriggersEvents: OnOccupancyChanged(bool), OnTargetOccupied(Targetable, GameObject)
// NeedsSetup: Attach to any GameObject that should be interactable. Configure 'Is Workable' and 'Random Candidate Count'.
