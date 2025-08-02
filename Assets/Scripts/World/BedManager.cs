using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World
{
    public class BedManager : MonoBehaviour
    {
        public static BedManager Instance { get; private set; }

        private readonly List<Bed> _beds = new List<Bed>();
        private readonly HashSet<Bed> _reservedBeds = new HashSet<Bed>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Another instance of BedManager already exists. Destroying this one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("BedManager initialized.");
        }

        public void RegisterBed(Bed bed)
        {
            if (!_beds.Contains(bed))
            {
                _beds.Add(bed);
                bed.OnFreed += HandleBedFreed;
                Debug.Log($"Bed {bed.name} registered. Total beds: {_beds.Count}");
            }
        }

        public void UnregisterBed(Bed bed)
        {
            if (_beds.Contains(bed))
            {
                _beds.Remove(bed);
                _reservedBeds.Remove(bed);
                bed.OnFreed -= HandleBedFreed;
                Debug.Log($"Bed {bed.name} unregistered. Total beds: {_beds.Count}");
            }
        }

        public Bed RequestBed(GameObject dwarf)
        {
            Bed availableBed = _beds.FirstOrDefault(b => !b.IsOccupied && !_reservedBeds.Contains(b));
            
            if (availableBed != null)
            {
                _reservedBeds.Add(availableBed);
                Debug.Log($"Bed {availableBed.name} reserved and assigned to {dwarf.name}.");
                return availableBed;
            }

            Debug.LogWarning($"No available beds for {dwarf.name}.");
            return null;
        }

        private void HandleBedFreed(Bed bed)
        {
            if (_reservedBeds.Contains(bed))
            {
                _reservedBeds.Remove(bed);
                Debug.Log($"Bed {bed.name} is no longer reserved.");
            }
        }
    }
}

// ScriptRole: Central manager for all beds in the game world, handles reservations.
// Dependencies: None
// HandlesEvents: Bed.OnFreed
// NeedsSetup: Add to a single, persistent GameObject in the scene.
