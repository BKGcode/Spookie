using UnityEngine;

namespace Dwarfs.States
{
    public class LivingState : DwarfBaseState
    {
        private float _livingTimer;
        private const float LivingDuration = 10f; // Time to wait before re-evaluating

        public LivingState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

        public override void EnterState()
        {
            Debug.Log($"{_data.name} is now in Living state. Wandering or resting.");
            _livingTimer = 0f;
        }

        public override void UpdateState()
        {
            _data.ConsumeLivingTime(Time.deltaTime);
            
            _livingTimer += Time.deltaTime;
            if (_livingTimer >= LivingDuration)
            {
                // After some time, go back to idle to check for new day or new opportunities
                _brain.TransitionToState(_brain.IdleState);
            }
        }

        public override void ExitState()
        {
            // Nothing to clean up
        }
    }
}

// ScriptRole: Manages the dwarf's behavior when it has no work to do, causing it to wander or rest temporarily. 