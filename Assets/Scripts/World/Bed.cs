using System;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Targetable))]
    public class Bed : MonoBehaviour
    {
        public event Action<Bed> OnFreed;
        
        public bool IsOccupied { get; private set; }
        public GameObject OccupiedBy { get; private set; }
        private Targetable _targetable;

        private void Awake()
        {
            _targetable = GetComponent<Targetable>();
        }

        private void OnEnable()
        {
            if (BedManager.Instance != null)
            {
                BedManager.Instance.RegisterBed(this);
            }
        }

        private void OnDisable()
        {
            if (BedManager.Instance != null)
            {
                BedManager.Instance.UnregisterBed(this);
            }
        }

        public void Occupy(GameObject occupier)
        {
            if (IsOccupied) return;
            
            IsOccupied = true;
            OccupiedBy = occupier;
            _targetable.SetOccupancy(true, occupier);
            Debug.Log($"Bed {name} is now Occupied by {occupier.name}.");
        }

        public void Vacate()
        {
            if (!IsOccupied) return;

            GameObject previousOccupier = OccupiedBy;
            IsOccupied = false;
            OccupiedBy = null;
            _targetable.SetOccupancy(false);
            
            OnFreed?.Invoke(this);
            
            Debug.Log($"Bed {name} is now Free. Was occupied by {previousOccupier?.name}.");
        }
    }
}

// ScriptRole: Manages the state of a bed and notifies when it becomes free.
// Dependencies: Targetable
// TriggersEvents: OnFreed
// NeedsSetup: Attach to a 'Bed' prefab alongside a 'Targetable' component.
