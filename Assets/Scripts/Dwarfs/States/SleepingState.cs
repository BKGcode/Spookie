using UnityEngine;

namespace Dwarfs.States
{
    public class SleepingState : DwarfBaseState
    {
        public SleepingState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

        public override void EnterState()
        {
            Debug.Log($"{_data.name} is now Sleeping in bed.");
            _movement.StopMoving();
        }

        public override void UpdateState()
        {
            // The dwarf is asleep, does nothing. The brain will transition it out when it's daytime.
        }

        public override void ExitState()
        {
            Debug.Log($"{_data.name} is waking up.");
            // Free up the bed when waking up
            if (_brain.ReservedBed != null)
            {
                _brain.ReservedBed.SetOccupancy(false);
                _brain.ReservedBed = null;
            }
        }
    }
}
// ScriptRole: Manages the dwarf's behavior when it is sleeping, primarily waiting for the day to start.
// HandlesEvents: TimeManager.OnDayStart 