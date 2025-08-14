using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DoorManager))]
public class DoorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DoorManager cabinetDoor = (DoorManager)target;

        cabinetDoor.doorType = (DoorManager.DoorType)EditorGUILayout.EnumPopup("Door Type", cabinetDoor.doorType);

        cabinetDoor.CabinetClose = (AudioSource)EditorGUILayout.ObjectField("Cabinet Close", cabinetDoor.CabinetClose, typeof(AudioSource), true);
        cabinetDoor.CabinetOpen = (AudioSource)EditorGUILayout.ObjectField("Cabinet Open", cabinetDoor.CabinetOpen, typeof(AudioSource), true);
        cabinetDoor.doorOpened = EditorGUILayout.Toggle("Door Opened", cabinetDoor.doorOpened);

        if (cabinetDoor.doorType == DoorManager.DoorType.KeyDoor)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Door Key Settings", EditorStyles.boldLabel);
            cabinetDoor.isLocked = EditorGUILayout.Toggle("Is Locked", cabinetDoor.isLocked);
            cabinetDoor.hasKey = EditorGUILayout.Toggle("Has Key", cabinetDoor.hasKey);
            cabinetDoor.keyTag = EditorGUILayout.TextField("Key Tag", cabinetDoor.keyTag);
            cabinetDoor.LockDoorOpen = (AudioSource)EditorGUILayout.ObjectField("Lock Door Open Sound", cabinetDoor.LockDoorOpen, typeof(AudioSource), true);
            cabinetDoor.LockedDoor = (AudioSource)EditorGUILayout.ObjectField("Locked Door", cabinetDoor.LockedDoor, typeof(AudioSource), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Error Canvas Settings", EditorStyles.boldLabel);
            cabinetDoor.ErrorCanvas = (GameObject)EditorGUILayout.ObjectField("Error Canvas", cabinetDoor.ErrorCanvas, typeof(GameObject), true);
            cabinetDoor.text = (Animator)EditorGUILayout.ObjectField("Error Text Animator", cabinetDoor.text, typeof(Animator), true);
            cabinetDoor.ErrorTextAnimationName = EditorGUILayout.TextField("Error Text Animation Name", cabinetDoor.ErrorTextAnimationName);
        }

        if (cabinetDoor.doorType == DoorManager.DoorType.KeypadDoor)
        {
            cabinetDoor.hasKey = EditorGUILayout.Toggle("Is Keypad Unlocked", cabinetDoor.isKeypadUnlocked);
        }
        if (cabinetDoor.doorType == DoorManager.DoorType.KeyCardDoor)
        {
            cabinetDoor.hasKey = EditorGUILayout.Toggle("Is Keycard Unlocked", cabinetDoor.isKeyCardUnlocked);
        }

        if (cabinetDoor.doorType == DoorManager.DoorType.Drawer)
        {
            cabinetDoor.targetPosition = EditorGUILayout.Vector3Field("Target Position", cabinetDoor.targetPosition);
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Door Rotation Settings", EditorStyles.boldLabel);
            cabinetDoor.targetAngleX = EditorGUILayout.FloatField("Target Angle X", cabinetDoor.targetAngleX);
            cabinetDoor.targetAngleY = EditorGUILayout.FloatField("Target Angle Y", cabinetDoor.targetAngleY);
            cabinetDoor.targetAngleZ = EditorGUILayout.FloatField("Target Angle Z", cabinetDoor.targetAngleZ);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nav Mesh Obstacles");
            SerializedProperty navMeshObstacles = serializedObject.FindProperty("navMeshObstacle");
            EditorGUILayout.PropertyField(navMeshObstacles, true);
        }

        cabinetDoor.openSpeed = EditorGUILayout.FloatField("Open Speed", cabinetDoor.openSpeed);
        cabinetDoor.closeSpeed = EditorGUILayout.FloatField("Close Speed", cabinetDoor.closeSpeed);

        cabinetDoor.inspectController = (InspectController)EditorGUILayout.ObjectField("Inspect Controller", cabinetDoor.inspectController, typeof(InspectController), true);
        cabinetDoor.ItemName = EditorGUILayout.TextField("Item Name", cabinetDoor.ItemName);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cabinetDoor);
        }
    }
}
