#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Interaction;
using Game.Messages;
using Game.EditorTools.Messages; // MessageDBSOEditor.SelectIdOnOpen
using MsgType = Game.Messages.MessageType;
using System.Collections.Generic;

namespace Game.EditorTools.Interaction
{
    [CustomEditor(typeof(DBTextInteractable))]
    public class DBTextInteractableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var dbi = (DBTextInteractable)target;
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(dbi.MessageId)))
            {
                if (GUILayout.Button("Abrir MessageDB y seleccionar Id", GUILayout.Height(22)))
                {
                    OpenDBAndSelectId(dbi.MessageId);
                }
            }
            if (GUILayout.Button("Crear entrada en MessageDB", GUILayout.Height(22)))
            {
                CreateEntryInDBFor(dbi);
            }
            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("GameObject/Messages/Crear entrada en MessageDB desde seleccionado", false, 0)]
        private static void CreateEntryFromMenu()
        {
            var go = Selection.activeGameObject;
            if (!go) { EditorUtility.DisplayDialog("MessageDB", "Selecciona un GameObject.", "OK"); return; }
            var dbi = go.GetComponent<DBTextInteractable>();
            if (!dbi) { EditorUtility.DisplayDialog("MessageDB", "El objeto seleccionado no tiene DBTextInteractable.", "OK"); return; }
            CreateEntryInDBFor(dbi);
        }

        private static MessageDBSO FindMessageDB()
        {
            // Prefer a single asset in project named MessageDBSO or of type MessageDBSO
            var guids = AssetDatabase.FindAssets("t:MessageDBSO");
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<MessageDBSO>(path);
            }
            EditorUtility.DisplayDialog("MessageDB", "No se encontró un asset de tipo MessageDBSO.", "OK");
            return null;
        }

        private static void OpenDBAndSelectId(string id)
        {
            var db = FindMessageDB();
            if (!db) return;
            // Ask the custom inspector to select id on open
            MessageDBSOEditor.SelectIdOnOpen(id);
            Selection.activeObject = db;
            EditorGUIUtility.PingObject(db);
        }

        private static void CreateEntryInDBFor(DBTextInteractable dbi)
        {
            var db = FindMessageDB();
            if (!db) return;

            Undo.RecordObject(db, "Crear entrada MessageDB");
            // Access private entries list via reflection
            var list = GetEntryList(db);
            if (list == null) { EditorUtility.DisplayDialog("MessageDB", "No se pudo acceder a la lista de entradas.", "OK"); return; }

            var entry = new MessageDBSO.Entry
            {
                id = string.IsNullOrWhiteSpace(dbi.MessageId) ? ("id_" + System.Guid.NewGuid().ToString("N").Substring(0, 8)).ToLowerInvariant() : dbi.MessageId,
                type = MsgType.Lower,
                key = string.Empty,
                autoCloseSeconds = -1f,
                typewriterCps = -1f,
                oneShot = false,
                persistentIdOverride = string.Empty,
                audio = null,
                localizedAudio = new List<MessageDBSO.LocalizedAudio>()
            };
            // Auto key
            if (string.IsNullOrWhiteSpace(entry.key))
                entry.key = $"msg.lower.{entry.id}";

            list.Add(entry);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();

            // Assign id back to the interactable if it was empty
            if (string.IsNullOrWhiteSpace(dbi.MessageId))
            {
                Undo.RecordObject(dbi, "Asignar MessageId");
                dbi.MessageId = entry.id;
                EditorUtility.SetDirty(dbi);
            }

            // Open and select
            OpenDBAndSelectId(entry.id);
        }

        private static List<MessageDBSO.Entry> GetEntryList(MessageDBSO db)
        {
            if (!db) return null;
            var field = typeof(MessageDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(db) as List<MessageDBSO.Entry>;
        }
    }
}
#endif
