using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World
{
    [RequireComponent(typeof(Targetable))]
    public class Bed : MonoBehaviour
    {
        private static readonly List<Bed> allBeds = new List<Bed>();

        public bool IsOccupied { get; private set; }
        private Targetable _targetable;

        private void Awake()
        {
            _targetable = GetComponent<Targetable>();
        }

        private void OnEnable()
        {
            if (!allBeds.Contains(this))
            {
                allBeds.Add(this);
            }
        }

        private void OnDisable()
        {
            allBeds.Remove(this);
        }

        public void SetOccupancy(bool isOccupied)
        {
            IsOccupied = isOccupied;
            _targetable.SetOccupancy(isOccupied); // Also update the underlying targetable
            Debug.Log($"Bed {name} is now {(isOccupied ? "Occupied" : "Free")}.");
        }

        public static Bed ReserveClosestBed(Vector3 position)
        {
            allBeds.RemoveAll(item => item == null);

            Bed closestBed = allBeds
                .Where(b => !b.IsOccupied)
                .OrderBy(b => Vector3.SqrMagnitude(b.transform.position - position))
                .FirstOrDefault();

            if (closestBed != null)
            {
                closestBed.SetOccupancy(true);
            }

            return closestBed;
        }
    }
}

// ScriptRole: Identifies a GameObject as a bed and manages its occupancy.
// Dependencies: Targetable
// NeedsSetup: Attach to a 'Bed' prefab alongside a 'Targetable' component. 