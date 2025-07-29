using UnityEngine;

namespace Dwarfs.States
{
    public abstract class DwarfBaseState
    {
        protected DwarfBrain _brain;
        protected DwarfData _data;
        protected DwarfMovement _movement;

        public DwarfBaseState(DwarfBrain brain, DwarfData data, DwarfMovement movement)
        {
            _brain = brain;
            _data = data;
            _movement = movement;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }
}

// ScriptRole: Abstract base class for all dwarf states, defining the required methods. 