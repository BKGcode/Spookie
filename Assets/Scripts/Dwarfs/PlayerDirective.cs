using World;

namespace Dwarfs
{
    /// <summary>
    /// A data container for player-issued commands.
    /// </summary>
    public class PlayerDirective
    {
        public Targetable Target { get; }

        public PlayerDirective(Targetable target)
        {
            Target = target;
        }
    }
}
// ScriptRole: A simple data structure to encapsulate a player's command to a dwarf. 