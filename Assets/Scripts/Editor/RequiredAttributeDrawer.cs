using UnityEngine;
using UnityEditor;
using Core.Shared;

namespace Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var requiredAttribute = (RequiredAttribute)attribute;
            bool isNull = false;

            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    isNull = property.objectReferenceValue == null;
                    break;
                case SerializedPropertyType.String:
                    isNull = string.IsNullOrWhiteSpace(property.stringValue);
                    break;
                // Add more types as needed
            }

            if (isNull)
            {
                var color = GUI.color;
                GUI.color = new Color(1, 0.4f, 0.4f); // Light red
                EditorGUI.PropertyField(position, property, label);
                GUI.color = color;

                var messageRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 
                    position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(messageRect, requiredAttribute.Message, MessageType.Error);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool isNull = false;
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    isNull = property.objectReferenceValue == null;
                    break;
                case SerializedPropertyType.String:
                    isNull = string.IsNullOrWhiteSpace(property.stringValue);
                    break;
            }

            return isNull ? 
                EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing : 
                EditorGUIUtility.singleLineHeight;
        }
    }
}

// ScriptRole: Custom property drawer for RequiredAttribute that shows validation in Unity Inspector
// Dependencies: RequiredAttribute
// Features:
//   - Visual feedback for null/empty required fields
//   - Custom error message display
//   - Support for Object references and strings