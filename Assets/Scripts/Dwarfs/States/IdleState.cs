using UnityEngine;

namespace Dwarfs.States
{
    public class IdleState : DwarfBaseState
    {
        public IdleState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

        public override void EnterState()
        {
            Debug.Log($"{_data.name} is now Idle, awaiting command from the brain.");
            // Stop any previous movement just in case.
            _movement.StopMoving();
        }

        public override void UpdateState()
        {
            // The brain is now responsible for all decisions. This state does nothing.
        }

        public override void ExitState()
        {
            // Nothing to clean up.
        }
    }
}
// ScriptRole: A passive state where the dwarf waits for the brain to decide the next action. 