using UnityEngine;

namespace Dwarfs.States
{
    public class WalkingState : DwarfBaseState
    {
        private Vector3 _destination;
        private WalkPurpose _purpose;

        public WalkingState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

        public void SetDestination(Vector3 destination, WalkPurpose purpose)
        {
            _destination = destination;
            _purpose = purpose;
        }

        public override void EnterState()
        {
            Debug.Log($"{_data.name} is now Walking to {_destination} for the purpose of {_purpose}.");
            _movement.OnArrival += HandleArrival;
            _movement.GoToTarget(_destination);
        }

        public override void UpdateState()
        {
            // Consume the correct time budget based on the purpose of the walk
            switch (_purpose)
            {
                case WalkPurpose.Work:
                    _data.ConsumeWorkTime(Time.deltaTime);
                    break;
                case WalkPurpose.Leisure:
                    _data.ConsumeLivingTime(Time.deltaTime);
                    break;
                case WalkPurpose.GoToBed:
                    // No budget is consumed for going to bed, it's a high-priority need.
                    break;
                case WalkPurpose.Wander:
                    _data.ConsumeLivingTime(Time.deltaTime);
                    break;
            }
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
            
            // Si era deambulación, volver a LivingState para continuar deambulando
            if (_purpose == WalkPurpose.Wander)
            {
                Debug.Log($"{_data.name} finished wandering, returning to LivingState");
                _brain.TransitionToState(_brain.LivingState);
            }
        }
    }
}

// ScriptRole: Manages the dwarf while it's moving towards a destination.
// HandlesEvents: DwarfMovement.OnArrival 