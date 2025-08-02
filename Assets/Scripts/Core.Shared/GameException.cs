using UnityEngine;

namespace Core.Shared
{
    /// <summary>
    /// Base class for game-specific exceptions with enhanced logging and context.
    /// </summary>
    public class GameException : System.Exception
    {
        public GameLogger.LogContext Context { get; }
        public Object Sender { get; }

        public GameException(string message, GameLogger.LogContext context = GameLogger.LogContext.System, Object sender = null) 
            : base(message)
        {
            Context = context;
            Sender = sender;
            LogException();
        }

        public GameException(string message, System.Exception innerException, GameLogger.LogContext context = GameLogger.LogContext.System, Object sender = null) 
            : base(message, innerException)
        {
            Context = context;
            Sender = sender;
            LogException();
        }

        private void LogException()
        {
            GameLogger.Exception(Context, this, Message, Sender);
        }
    }

    /// <summary>
    /// Exception thrown when a required game resource is missing or invalid.
    /// </summary>
    public class ResourceException : GameException
    {
        public ResourceException(string message, Object sender = null) 
            : base(message, GameLogger.LogContext.System, sender)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a game state validation fails.
    /// </summary>
    public class ValidationException : GameException
    {
        public ValidationException(string message, GameLogger.LogContext context, Object sender = null) 
            : base(message, context, sender)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an operation fails due to incorrect timing or sequence.
    /// </summary>
    public class StateException : GameException
    {
        public StateException(string message, GameLogger.LogContext context, Object sender = null) 
            : base(message, context, sender)
        {
        }
    }
}

// ScriptRole: Provides base exception types for game-specific error handling with enhanced logging.
// Dependencies: GameLogger
// Features:
//   - Context-aware exceptions
//   - Automatic logging on creation
//   - Specialized exception types for common scenarios
// Usage:
//   throw new ValidationException("Invalid dwarf name", LogContext.Dwarf, this);
//   throw new ResourceException("Missing required sprite", this);
//   throw new StateException("Cannot sleep during day", LogContext.Time, this);