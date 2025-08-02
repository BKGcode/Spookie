using UnityEngine;

namespace Core.Shared
{
    /// <summary>
    /// Indicates that a serialized field is required and should not be null.
    /// Used in the Unity Inspector to mark mandatory fields.
    /// </summary>
    public class RequiredAttribute : PropertyAttribute
    {
        public string Message { get; private set; }

        public RequiredAttribute(string message = "This field is required")
        {
            Message = message;
        }
    }
}

// ScriptRole: Provides a custom attribute for marking required fields in Unity Inspector
// Dependencies: None
// Features:
//   - Custom validation message
//   - Unity Inspector integration
// Usage: [Required] or [Required("Custom message")]