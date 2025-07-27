using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for MaterialSO to make the GUID field read-only.
/// </summary>
[CustomEditor(typeof(MaterialSO))]
public class MaterialSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MaterialSO materialSO = (MaterialSO)target;

        // Draw the GUID field as a read-only label
        EditorGUILayout.LabelField("GUID", materialSO.Guid);

        // Draw the rest of the default inspector
        DrawDefaultInspector();
    }
} 