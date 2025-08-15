#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Messages;
using Game.Interaction;
using Game.EditorTools.Messages; // MessageDBSOEditor.SelectIdOnOpen
using MsgType = Game.Messages.MessageType;

namespace Game.EditorTools.Messages
{
    /// <summary>
    /// KISS browser to list MessageDB entries, search/filter, see usage in open scenes,
    /// and assign an entry id to selected DBTextInteractable objects (or add the component).
    /// </summary>
    public class MessageBrowserWindow : EditorWindow
    {
        private const string Title = "Messages Browser & Assign";
        private const float RowHeight = 22f;

    [MenuItem("Tools/Spookie/Localization/Messages Browser & Assign", priority = 2)]
        public static void Open()
        {
            var w = GetWindow<MessageBrowserWindow>(false, Title, true);
            w.minSize = new Vector2(680, 420);
            w.Show();
        }

        private MessageDBSO _msgDb;
        private List<MessageDBSO.Entry> _entries = new List<MessageDBSO.Entry>();
        private Vector2 _scroll;
        private string _search = "";
        private bool _showLower = true;
        private bool _showOniric = true;
        private Dictionary<string, int> _usageCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        private HashSet<string> _selectedIds = new HashSet<string>(StringComparer.Ordinal);
    private bool _filterUnusedOnly = false;
    private bool _filterMissingVoOnly = false;

        private void OnEnable()
        {
            TryAutoWireDb();
            RebuildEntryCache();
        }

        private void OnFocus()
        {
            // Refresh usage on focus to stay up-to-date
            RefreshUsageInOpenScenes();
            Repaint();
        }

        private void OnGUI()
        {
            DrawHeader();
            if (_msgDb == null)
            {
                EditorGUILayout.HelpBox("Asigna un MessageDB para listar entradas.", UnityEditor.MessageType.Info);
                return;
            }

            DrawToolbar();
            DrawList();
            DrawFooterActions();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Database", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                var newDb = (MessageDBSO)EditorGUILayout.ObjectField("MessageDB", _msgDb, typeof(MessageDBSO), false);
                if (newDb != _msgDb)
                {
                    _msgDb = newDb;
                    RebuildEntryCache();
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Autodetect", GUILayout.Width(120)))
                {
                    TryAutoWireDb();
                    RebuildEntryCache();
                }
                if (GUILayout.Button("Open DB", GUILayout.Width(100)))
                {
                    if (_msgDb) { Selection.activeObject = _msgDb; EditorGUIUtility.PingObject(_msgDb); }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            // Search
            var newSearch = EditorGUILayout.TextField(GUIContent.none, _search, "SearchTextField");
            if (newSearch != _search) { _search = newSearch; }
            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) { _search = string.Empty; GUI.FocusControl(null); }

            GUILayout.Space(8);
            _showLower = GUILayout.Toggle(_showLower, "Lower", EditorStyles.miniButton, GUILayout.Width(60));
            _showOniric = GUILayout.Toggle(_showOniric, "Oníric", EditorStyles.miniButton, GUILayout.Width(60));

            GUILayout.Space(8);
            _filterUnusedOnly = GUILayout.Toggle(_filterUnusedOnly, "Unused=0", EditorStyles.miniButton, GUILayout.Width(80));
            _filterMissingVoOnly = GUILayout.Toggle(_filterMissingVoOnly, "Missing VO", EditorStyles.miniButton, GUILayout.Width(90));

            GUILayout.Space(10);
            if (GUILayout.Button("Scan Usage (Open Scenes)", GUILayout.Width(190)))
            {
                RefreshUsageInOpenScenes();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
        }

        private void DrawList()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                // Header row
                DrawRowHeader();

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                var filtered = FilteredEntries();
                for (int i = 0; i < filtered.Count; i++)
                {
                    DrawRow(filtered[i], i);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawRowHeader()
        {
            var r = GUILayoutUtility.GetRect(10, RowHeight, GUILayout.ExpandWidth(true));
            var rect = new Rect(r.x, r.y, r.width, r.height);
            DrawRowBackground(rect, true);
            float x = rect.x + 6f;
            Label(ref x, rect, 24f, "Sel");
            Label(ref x, rect, 160f, "Id");
            Label(ref x, rect, 56f, "Type");
            Label(ref x, rect, 160f, "Key");
            Label(ref x, rect, 48f, "VO");
            Label(ref x, rect, 54f, "OneShot");
            Label(ref x, rect, 50f, "Used");
            // actions space
        }

        private void DrawRow(MessageDBSO.Entry e, int index)
        {
            var r = GUILayoutUtility.GetRect(10, RowHeight, GUILayout.ExpandWidth(true));
            var rect = new Rect(r.x, r.y, r.width, r.height);
            DrawRowBackground(rect, false, index);

            float x = rect.x + 4f;
            // select toggle
            bool isSel = _selectedIds.Contains(e.id);
            bool newSel = GUI.Toggle(new Rect(x, rect.y + 2, 20, rect.height - 4), isSel, GUIContent.none);
            if (newSel != isSel)
            {
                if (newSel) _selectedIds.Add(e.id); else _selectedIds.Remove(e.id);
            }
            x += 24f;

            // id
            GUI.Label(new Rect(x, rect.y, 160f, rect.height), e.id);
            x += 160f;

            // type
            GUI.Label(new Rect(x, rect.y, 56f, rect.height), e.type.ToString());
            x += 56f;

            // key (trim)
            string key = string.IsNullOrEmpty(e.key) ? "" : e.key;
            GUI.Label(new Rect(x, rect.y, 160f, rect.height), key);
            x += 160f;

            // VO indicator
            bool hasVO = HasVO(e);
            GUI.Label(new Rect(x, rect.y, 48f, rect.height), hasVO ? "yes" : "-");
            x += 48f;

            // oneShot
            GUI.Label(new Rect(x, rect.y, 54f, rect.height), e.oneShot ? "yes" : "no");
            x += 54f;

            // used count
            int used = 0; _usageCounts.TryGetValue(e.id ?? string.Empty, out used);
            GUI.Label(new Rect(x, rect.y, 50f, rect.height), used.ToString());
            x += 50f;

            // actions
            var btnW = 84f;
            if (GUI.Button(new Rect(rect.xMax - btnW * 3 - 6, rect.y + 2, btnW, rect.height - 4), "Select DB"))
            {
                MessageDBSOEditor.SelectIdOnOpen(e.id);
                Selection.activeObject = _msgDb; EditorGUIUtility.PingObject(_msgDb);
            }
            if (GUI.Button(new Rect(rect.xMax - btnW * 2 - 4, rect.y + 2, btnW, rect.height - 4), "Assign → Sel"))
            {
                AssignIdToSelected(e.id);
            }
            if (GUI.Button(new Rect(rect.xMax - btnW - 2, rect.y + 2, btnW, rect.height - 4), "Create GO"))
            {
                CreateInteractablesOnSelection(e.id);
            }
        }

        private void DrawFooterActions()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_selectedIds.Count == 0))
            {
                if (GUILayout.Button($"Assign {_selectedIds.Count} id(s) to Selected Objects"))
                {
                    AssignMultipleToSelection();
                }
            }
            if (GUILayout.Button("Select All"))
            {
                _selectedIds.Clear();
                foreach (var e in _entries) _selectedIds.Add(e.id);
            }
            if (GUILayout.Button("Clear Sel"))
            {
                _selectedIds.Clear();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void AssignIdToSelected(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            var objs = Selection.gameObjects;
            if (objs == null || objs.Length == 0)
            {
                EditorUtility.DisplayDialog(Title, "Selecciona GameObjects en la jerarquía.", "OK");
                return;
            }
            Undo.IncrementCurrentGroup();
            int changed = 0;
            foreach (var go in objs)
            {
                var dbi = go.GetComponent<DBTextInteractable>();
                if (dbi == null) continue;
                Undo.RecordObject(dbi, "Assign MessageId");
                dbi.MessageId = id;
                EditorUtility.SetDirty(dbi);
                changed++;
            }
            if (changed == 0)
            {
                EditorUtility.DisplayDialog(Title, "Los objetos seleccionados no tienen DBTextInteractable.", "OK");
            }
            else
            {
                EditorGUIUtility.systemCopyBuffer = id; // conveniencia para pegar
            }
        }

        private void CreateInteractablesOnSelection(string id)
        {
            var objs = Selection.gameObjects;
            if (objs == null || objs.Length == 0)
            {
                EditorUtility.DisplayDialog(Title, "Selecciona GameObjects en la jerarquía.", "OK");
                return;
            }
            int created = 0, assigned = 0, collidersAdded = 0, layersChanged = 0;
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            bool missingInteractableLayer = interactableLayer == -1;
            foreach (var go in objs)
            {
                var dbi = go.GetComponent<DBTextInteractable>();
                if (dbi == null)
                {
                    Undo.AddComponent<DBTextInteractable>(go);
                    dbi = go.GetComponent<DBTextInteractable>();
                    created++;
                }
                if (dbi != null && !string.IsNullOrWhiteSpace(id))
                {
                    Undo.RecordObject(dbi, "Assign MessageId");
                    dbi.MessageId = id;
                    EditorUtility.SetDirty(dbi);
                    assigned++;
                }

                // Ensure it has a Collider for raycast interactions (default fire mode = Interaction)
                var col = go.GetComponent<Collider>();
                if (col == null)
                {
                    Undo.AddComponent<BoxCollider>(go);
                    var bc = go.GetComponent<BoxCollider>();
                    if (bc != null)
                    {
                        bc.isTrigger = false; // default to raycast interaction
                        collidersAdded++;
                    }
                }

                // Ensure layer is set to "Interactable" on the GameObject (not recursive, KISS)
                if (!missingInteractableLayer)
                {
                    if (go.layer != interactableLayer)
                    {
                        Undo.RecordObject(go, "Set Layer to Interactable");
                        go.layer = interactableLayer;
                        EditorUtility.SetDirty(go);
                        layersChanged++;
                    }
                }
            }
            // Warn if there's no orchestrator in any open scene
            bool hasOrchestrator = false;
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.GetComponentInChildren<Game.Messages.MessageOrchestrator>(true) != null)
                    {
                        hasOrchestrator = true;
                        break;
                    }
                }
                if (hasOrchestrator) break;
            }

            string msg = $"Interactuables creados: {created}\nAsignaciones realizadas: {assigned}";
            if (collidersAdded > 0) msg += $"\nColliders añadidos: {collidersAdded}";
            if (layersChanged > 0) msg += $"\nObjetos pasados a capa 'Interactable': {layersChanged}";
            if (!hasOrchestrator) msg += "\n\nAviso: No se encontró MessageOrchestrator en las escenas abiertas. Añádelo para que los mensajes se muestren.";
            if (missingInteractableLayer) msg += "\n\nAviso: No existe la capa 'Interactable'. Crea la capa en Project Settings > Tags and Layers para que el raycast la filtre correctamente.";
            EditorUtility.DisplayDialog(Title, msg, "OK");
        }

        private void AssignMultipleToSelection()
        {
            var ids = _selectedIds.ToList();
            if (ids.Count == 0) return;
            var objs = Selection.gameObjects;
            if (objs == null || objs.Length == 0) { EditorUtility.DisplayDialog(Title, "Selecciona GameObjects.", "OK"); return; }

            // Simple mode: assign the first selected id to all selected objects.
            string id = ids[0];
            AssignIdToSelected(id);
        }

        private List<MessageDBSO.Entry> FilteredEntries()
        {
            IEnumerable<MessageDBSO.Entry> q = _entries;
            if (!_showLower) q = q.Where(e => e.type != MsgType.Lower);
            if (!_showOniric) q = q.Where(e => e.type != MsgType.Oniric);
            if (!string.IsNullOrWhiteSpace(_search))
            {
                var s = _search.Trim();
                q = q.Where(e => (e.id != null && e.id.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0)
                              || (e.key != null && e.key.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0));
            }
            if (_filterUnusedOnly)
            {
                q = q.Where(e =>
                {
                    int used;
                    if (!_usageCounts.TryGetValue(e.id ?? string.Empty, out used)) used = 0;
                    return used == 0;
                });
            }
            if (_filterMissingVoOnly)
            {
                q = q.Where(e => !HasVO(e));
            }
            return q.ToList();
        }

        private static bool HasVO(MessageDBSO.Entry e)
        {
            if (e == null) return false;
            if (e.audio != null) return true;
            if (e.localizedAudio == null) return false;
            for (int i = 0; i < e.localizedAudio.Count; i++)
            {
                var la = e.localizedAudio[i];
                if (la != null && la.clip != null) return true;
            }
            return false;
        }

        private void RefreshUsageInOpenScenes()
        {
            _usageCounts.Clear();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var list = root.GetComponentsInChildren<DBTextInteractable>(true);
                    foreach (var dbi in list)
                    {
                        string id = dbi != null ? dbi.MessageId : null;
                        if (string.IsNullOrWhiteSpace(id)) continue;
                        if (_usageCounts.TryGetValue(id, out var c)) _usageCounts[id] = c + 1; else _usageCounts[id] = 1;
                    }
                }
            }
        }

        private void TryAutoWireDb()
        {
            if (_msgDb) return;
            var guids = AssetDatabase.FindAssets("t:Game.Messages.MessageDBSO t:MessageDBSO");
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _msgDb = AssetDatabase.LoadAssetAtPath<MessageDBSO>(path);
            }
        }

        private void RebuildEntryCache()
        {
            _entries.Clear();
            if (_msgDb == null) return;
            var field = typeof(MessageDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = field?.GetValue(_msgDb) as System.Collections.IList;
            if (list == null) return;
            foreach (var obj in list)
            {
                var e = obj as MessageDBSO.Entry;
                if (e != null) _entries.Add(e);
            }
        }

        private static void DrawRowBackground(Rect rect, bool header, int index = 0)
        {
            var bg = header ? new Color(0.18f, 0.18f, 0.18f, 1f) : (index % 2 == 0 ? new Color(0, 0, 0, 0.06f) : new Color(0, 0, 0, 0.0f));
            EditorGUI.DrawRect(rect, bg);
        }

        private static void Label(ref float x, Rect row, float width, string text)
        {
            GUI.Label(new Rect(x, row.y, width, row.height), text, EditorStyles.miniBoldLabel);
            x += width;
        }
    }
}
#endif
