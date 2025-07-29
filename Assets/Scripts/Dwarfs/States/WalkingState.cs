using UnityEngine;

namespace Dwarfs.States
{
    public class WalkingState : DwarfBaseState
    {
        private Vector3 _destination;

        public WalkingState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

        public void SetDestination(Vector3 destination)
        {
            _destination = destination;
        }

        public override void EnterState()
        {
            Debug.Log($"{_data.name} is now Walking to {_destination}.");
            _movement.OnArrival += HandleArrival;
            _movement.GoToTarget(_destination);
        }

        public override void UpdateState()
        {
            // Movement is handled by the coroutine. The brain will interrupt if needed.
        }

        public override void ExitState()
        {
            _movement.OnArrival -= HandleArrival;
            // Ensure movement is stopped if the state is exited prematurely (e.g., interrupted by the brain).
            _movement.StopMoving(); 
        }

        private void HandleArrival()
        {
            // Simply notify that we've arrived. The brain will decide what to do next in its Update.
            Debug.Log($"{_data.name} has arrived at destination.");
        }
    }
}

// ScriptRole: Manages the dwarf while it's moving towards a destination.
// HandlesEvents: DwarfMovement.OnArrival 