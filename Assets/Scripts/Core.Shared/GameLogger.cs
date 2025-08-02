using UnityEngine;
using System;

namespace Core.Shared
{
    /// <summary>
    /// Provides centralized logging functionality with consistent formatting and error levels.
    /// </summary>
    public static class GameLogger
    {
        private const string LOG_FORMAT = "[{0}] {1}";
        private static readonly string[] CONTEXT_PREFIXES = { "DWARF", "TIME", "WORLD", "UI", "SYSTEM" };

        public enum LogContext
        {
            Dwarf,
            Time,
            World,
            UI,
            System
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public static void Info(LogContext context, string message, UnityEngine.Object sender = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            UnityEngine.Debug.Log(Format(context, message), sender);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public static void Warn(LogContext context, string message, UnityEngine.Object sender = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            UnityEngine.Debug.LogWarning(Format(context, message), sender);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public static void Error(LogContext context, string message, UnityEngine.Object sender = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            UnityEngine.Debug.LogError(Format(context, message), sender);
        }

        /// <summary>
        /// Logs an exception with full stack trace.
        /// </summary>
        public static void Exception(LogContext context, Exception ex, string message = null, UnityEngine.Object sender = null)
        {
            if (ex == null) return;
            
            string fullMessage = string.IsNullOrEmpty(message)
                ? ex.Message
                : $"{message}: {ex.Message}";
            
            UnityEngine.Debug.LogException(ex, sender);
            Error(context, fullMessage, sender);
        }

        /// <summary>
        /// Logs a debug message only in development builds.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Debug(LogContext context, string message, UnityEngine.Object sender = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            UnityEngine.Debug.Log($"[DEBUG][{CONTEXT_PREFIXES[(int)context]}] {message}", sender);
        }

        private static string Format(LogContext context, string message)
        {
            return string.Format(LOG_FORMAT, CONTEXT_PREFIXES[(int)context], message);
        }
    }
}

// ScriptRole: Provides centralized logging functionality with consistent formatting and context-based categorization.
// Dependencies: None
// Features:
//   - Context-based logging categories
//   - Consistent message formatting
//   - Development-only debug messages
//   - Exception handling with stack traces
// Usage:
//   GameLogger.Info(LogContext.Dwarf, "Dwarf initialized", this);
//   GameLogger.Error(LogContext.System, "Configuration error", this);
//   GameLogger.Exception(LogContext.World, ex, "Failed to load terrain", this);