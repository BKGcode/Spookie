using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FuseBox))]
public class FuseBoxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        FuseBox fuseBox = (FuseBox)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("fuseboxType"));


        if (fuseBox.fuseboxType == FuseBox.FuseboxType.FuseboxDoor)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FuseboxAnimator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("openAnimation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("closeAnimation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FuseboxDoorOpen"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Open"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Close"));
        }
        else if (fuseBox.fuseboxType == FuseBox.FuseboxType.FuseboxHandle)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FuseboxLeverAnimator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pullupAnimation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pulldownAnimation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FuseboxLeverPulled"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Electronic Devices", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Switchers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("brokenSwitchers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("television"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Lamps"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PullDown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PullUp"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("inspectController"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemName"));

        serializedObject.ApplyModifiedProperties();
    }
}
