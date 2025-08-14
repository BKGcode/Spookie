#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Game.Messages;
using MsgType = Game.Messages.MessageType;
using Game.Interaction; // DBTextInteractable for Assign->Selected

namespace Game.EditorTools.Messages
{
    [CustomEditor(typeof(MessageDBSO))]
    public class MessageDBSOEditor : Editor
    {
        // Allow other editor tools to request selecting a specific entry by id when this inspector opens
        private static string _pendingSelectId;
        public static void SelectIdOnOpen(string id)
        {
            _pendingSelectId = id;
        }

        private SerializedProperty _entriesProp;
        private ReorderableList _list;

        private void OnEnable()
        {
            _entriesProp = serializedObject.FindProperty("entries");
            _list = new ReorderableList(serializedObject, _entriesProp, true, true, true, true);

            _list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Messages");
            };

            _list.elementHeight = EditorGUIUtility.singleLineHeight + 6f;
            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var elem = _entriesProp.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;

                var idProp = elem.FindPropertyRelative("id");
                var typeProp = elem.FindPropertyRelative("type");
                var keyProp = elem.FindPropertyRelative("key");

                float w = rect.width;
                float col = w * 0.32f;
                var idRect = new Rect(rect.x, rect.y, col, rect.height);
                var typeRect = new Rect(rect.x + col + 4, rect.y, 80, rect.height);
                var keyRect = new Rect(typeRect.x + typeRect.width + 4, rect.y, w - (typeRect.x + typeRect.width + 4) - 120f, rect.height);
                var btnRect = new Rect(rect.x + w - 116f, rect.y, 56f, rect.height);
                var guidRect = new Rect(rect.x + w - 56f, rect.y, 56f, rect.height);

                idProp.stringValue = EditorGUI.TextField(idRect, idProp.stringValue);
                EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);
                keyProp.stringValue = EditorGUI.TextField(keyRect, keyProp.stringValue);

                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(idProp.stringValue)))
                {
                    if (GUI.Button(btnRect, "Copy"))
                    {
                        EditorGUIUtility.systemCopyBuffer = idProp.stringValue;
                    }
                }
        if (GUI.Button(guidRect, "GUID"))
                {
                    idProp.stringValue = ("id_" + System.Guid.NewGuid().ToString("N").Substring(0, 8)).ToLowerInvariant();
                    // Autogenerar key si está vacía
                    if (string.IsNullOrWhiteSpace(keyProp.stringValue))
                    {
            var mt = (MsgType)typeProp.enumValueIndex;
                        keyProp.stringValue = $"msg.{(mt == MsgType.Lower ? "lower" : "oniric")}.{idProp.stringValue}";
                    }
                }
            };

            _list.onAddCallback = l =>
            {
                int i = _entriesProp.arraySize;
                _entriesProp.arraySize++;
                var elem = _entriesProp.GetArrayElementAtIndex(i);
                // Init defaults without cloning previous
                elem.FindPropertyRelative("id").stringValue = string.Empty;
                elem.FindPropertyRelative("type").enumValueIndex = (int)MsgType.Lower;
                elem.FindPropertyRelative("key").stringValue = string.Empty;
                elem.FindPropertyRelative("autoCloseSeconds").floatValue = -1f;
                elem.FindPropertyRelative("typewriterCps").floatValue = -1f;
                elem.FindPropertyRelative("oneShot").boolValue = false;
                elem.FindPropertyRelative("persistentIdOverride").stringValue = string.Empty;
                elem.FindPropertyRelative("audio").objectReferenceValue = null;
                var locList = elem.FindPropertyRelative("localizedAudio");
                if (locList != null) locList.arraySize = 0;
                _list.index = i;
            };

            // If someone asked to select a particular id, try to find and select it now
            if (!string.IsNullOrEmpty(_pendingSelectId) && _entriesProp != null)
            {
                int found = -1;
                for (int i = 0; i < _entriesProp.arraySize; i++)
                {
                    var elem = _entriesProp.GetArrayElementAtIndex(i);
                    if (elem.FindPropertyRelative("id").stringValue == _pendingSelectId)
                    {
                        found = i;
                        break;
                    }
                }
                if (found >= 0)
                {
                    _list.index = found;
                    // Clear pending once applied
                    _pendingSelectId = null;
                    // Ensure repaint so selection is visible
                    Repaint();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Toolbar
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add New", GUILayout.Height(22)))
            {
                _list.onAddCallback?.Invoke(_list);
            }
            using (new EditorGUI.DisabledScope(_list.index < 0 || _list.index >= _entriesProp.arraySize))
            {
                if (GUILayout.Button("Assign->Selected", GUILayout.Width(130), GUILayout.Height(22)))
                {
                    AssignSelectedIdToInteractable();
                }
                if (GUILayout.Button("Fix Key Auto", GUILayout.Width(110), GUILayout.Height(22)))
                {
                    AutoFillKeyForSelected();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            _list.DoLayoutList();

            // Details panel for selected
            if (_list.index >= 0 && _list.index < _entriesProp.arraySize)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Selected Entry Details", EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    var elem = _entriesProp.GetArrayElementAtIndex(_list.index);
                    EditorGUILayout.PropertyField(elem, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AssignSelectedIdToInteractable()
        {
            var go = Selection.activeGameObject;
            if (go == null) { EditorUtility.DisplayDialog("Assign id", "Selecciona un GameObject con DBTextInteractable.", "OK"); return; }
            var dbi = go.GetComponent<DBTextInteractable>();
            if (dbi == null) { EditorUtility.DisplayDialog("Assign id", "El objeto seleccionado no tiene DBTextInteractable.", "OK"); return; }
            var elem = _entriesProp.GetArrayElementAtIndex(_list.index);
            var id = elem.FindPropertyRelative("id").stringValue;
            if (string.IsNullOrWhiteSpace(id)) { EditorUtility.DisplayDialog("Assign id", "El id está vacío.", "OK"); return; }
            Undo.RecordObject(dbi, "Assign MessageId");
            dbi.MessageId = id;
            EditorUtility.SetDirty(dbi);
            EditorGUIUtility.systemCopyBuffer = id;
        }

        private void AutoFillKeyForSelected()
        {
            var elem = _entriesProp.GetArrayElementAtIndex(_list.index);
            var idProp = elem.FindPropertyRelative("id");
            var keyProp = elem.FindPropertyRelative("key");
            var typeProp = elem.FindPropertyRelative("type");
            if (string.IsNullOrWhiteSpace(idProp.stringValue)) { EditorUtility.DisplayDialog("Fix Key", "El id está vacío.", "OK"); return; }
            var mt = (MsgType)typeProp.enumValueIndex;
            keyProp.stringValue = $"msg.{(mt == MsgType.Lower ? "lower" : "oniric")}.{idProp.stringValue}";
        }
    }
}
#endif
