using UnityEngine;

namespace Core.Shared
{
    public class PlayerDirective
    {
        public ITargetable Target { get; }

        public PlayerDirective(ITargetable target)
        {
            Target = target;
            Debug.Log($"New player directive created for target: {target.name}");
        }

        public string Description => Target != null ? $"Interact with {Target.name}" : "None";

        public override string ToString()
        {
            return Description;
        }
    }
}

// ScriptRole: Represents a player command to interact with a target
// Dependencies: ITargetable
// NeedsSetup: None - Used as a data structure