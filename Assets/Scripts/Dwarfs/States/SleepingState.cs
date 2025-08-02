using UnityEngine;

namespace Dwarfs.States
{
    public class SleepingState : DwarfBaseState
    {
        public SleepingState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

                public override void EnterState()
        {
            if (_brain.TargetBed != null && !_brain.TargetBed.IsOccupied)
            {
                Debug.Log($"{_data.name} is now Sleeping in bed {_brain.TargetBed.name}.");
                _movement.StopMoving();
                _brain.TargetBed.Occupy(_brain.gameObject);
            }
            else
            {
                // If the bed is somehow occupied when we arrive, go back to idle to re-evaluate.
                Debug.LogWarning($"{_data.name} arrived at bed {_brain.TargetBed?.name}, but it was already taken. Finding another.");
                _brain.TargetBed = null;
                _brain.TransitionToState(_brain.IdleState);
            }
        }

        public override void UpdateState()
        {
            // The dwarf is asleep. The brain will transition it out when it's daytime.
        }

        public override void ExitState()
        {
            Debug.Log($"{_data.name} is waking up.");
            // Free up the bed when waking up.
            if (_brain.TargetBed != null)
            {
                _brain.TargetBed.Vacate();
                _brain.TargetBed = null;
            }
        }
    }
}

// ScriptRole: Manages the dwarf's sleeping behavior, occupying and vacating a bed.
// Dependencies: DwarfBrain
// NeedsSetup: None 