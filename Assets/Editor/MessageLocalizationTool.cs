#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Game.Messages;
using Game.Localization;
using Game.Interaction; // DBTextInteractable
using MsgType = Game.Messages.MessageType;

namespace Game.EditorTools.Messages
{
    /// <summary>
    /// EditorWindow para gestionar Mensajes (Lower/Oníricos) y Localización con flujo combinado (CSV único),
    /// además de vincular IDs de mensaje a objetos interactuables por escena.
    /// KISS: UI sencilla, sin dependencias externas. CSV amigable con Excel/Sheets (UTF-8 BOM).
    /// </summary>
    public class MessageLocalizationTool : EditorWindow
    {
        private const string Title = "Message & Localization";
        private const string PrefsKeyFolder = "Spookie.MsgLoc.LastFolder";

        // Selecciones actuales
        private MessageDBSO _msgDb;
        private LocalizationDBSO _locDb;

        // Tabs reducidos a KISS
        private enum Tab { ImportExport, SingleEntry }
        private Tab _tab = Tab.ImportExport;

        // Single Entry fields
        private string _newId = string.Empty;
    private MsgType _newType = MsgType.Lower;
    private string _newKey = string.Empty;
    private float _newAutoClose = -1f;
    private float _newCps = -1f;
    private bool _newOneShot = false;
        private string _newPersistentId = string.Empty;
        private AudioClip _newAudio;
        private readonly Dictionary<string, string> _perLangText = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    // Cache opcional de IDs (para autocompletar u otros usos futuros)
    private List<string> _cachedIds = new List<string> { string.Empty };

        [MenuItem("Spookie/Messages & Localization Tool", priority = 1)]
        public static void Open()
        {
            var w = GetWindow<MessageLocalizationTool>(false, Title, true);
            w.minSize = new Vector2(560, 420);
            w.Show();
        }

        private void OnEnable()
        {
            TryAutoWireDatabases();
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(4);
            _tab = (Tab)GUILayout.Toolbar((int)_tab, new[] { "Import/Export", "Single Entry" });
            EditorGUILayout.Space(6);

            switch (_tab)
            {
                case Tab.ImportExport:
                    DrawImportExport();
                    break;
                case Tab.SingleEntry:
                    DrawSingleEntry();
                    break;
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Databases", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                _msgDb = (MessageDBSO)EditorGUILayout.ObjectField("MessageDB", _msgDb, typeof(MessageDBSO), false);
                _locDb = (LocalizationDBSO)EditorGUILayout.ObjectField("LocalizationDB", _locDb, typeof(LocalizationDBSO), false);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Autodetect in Project", GUILayout.Width(180)))
                {
                    TryAutoWireDatabases();
                }
                if (GUILayout.Button("Ping Assets", GUILayout.Width(120)))
                {
                    if (_msgDb) EditorGUIUtility.PingObject(_msgDb);
                    if (_locDb) EditorGUIUtility.PingObject(_locDb);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        // Import/Export (CSV combinado)
        private void DrawImportExport()
        {
            EditorGUILayout.HelpBox("CSV combinado (simplificado): id,type,key,autoClose,typewriterCps,oneShot,persistentId,audioPath,<lang1>,<lang2>,...\nImporta también CSVs antiguos con columna 'repeatable' (se ignora).", UnityEditor.MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Combined CSV...", GUILayout.Height(28)))
            {
                ExportCombinedCsv();
            }
            if (GUILayout.Button("Import Combined CSV...", GUILayout.Height(28)))
            {
                ImportCombinedCsv(dryRun: false);
            }
            if (GUILayout.Button("Dry‑Run Import", GUILayout.Height(28)))
            {
                ImportCombinedCsv(dryRun: true);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Tips", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Usa UTF‑8 con BOM para máxima compatibilidad con Excel.");
            EditorGUILayout.LabelField("• Deja key vacío para autogenerar: msg.<type>.<id>.");
            EditorGUILayout.LabelField("• audioPath opcional: Assets/…/clip.wav (se resuelve por AssetDatabase).\n  Si no existe, se ignora y se mantiene el audio actual.");
        }

        private void ExportCombinedCsv()
        {
            if (!_msgDb || !_locDb)
            {
                EditorUtility.DisplayDialog(Title, "Asigna MessageDB y LocalizationDB.", "OK");
                return;
            }

            string startFolder = EditorPrefs.GetString(PrefsKeyFolder, Application.dataPath);
            string path = EditorUtility.SaveFilePanel("Export Combined CSV", startFolder, "messages_full", "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                EditorPrefs.SetString(PrefsKeyFolder, Path.GetDirectoryName(path));
                var langs = GetLanguages(_locDb);
                var sb = new StringBuilder();
                // Header (sin 'repeatable')
                sb.Append(E("id")).Append(',')
                  .Append(E("type")).Append(',')
                  .Append(E("key")).Append(',')
                  .Append(E("autoClose")).Append(',')
                  .Append(E("typewriterCps")).Append(',')
                  .Append(E("oneShot")).Append(',')
                  .Append(E("persistentId")).Append(',')
                  .Append(E("audioPath"));
                foreach (var l in langs) sb.Append(',').Append(E(l));
                sb.AppendLine();

                foreach (var e in GetEntries(_msgDb))
                {
                                        sb.Append(E(e.id)).Append(',')
                                            .Append(E(e.type.ToString().ToLowerInvariant())).Append(',')
                                            .Append(E(e.key ?? string.Empty)).Append(',')
                                            .Append(E(e.autoCloseSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture))).Append(',')
                                            .Append(E(e.typewriterCps.ToString(System.Globalization.CultureInfo.InvariantCulture))).Append(',')
                                            .Append(E(e.oneShot ? "true" : "false")).Append(',')
                                            .Append(E(string.IsNullOrEmpty(e.persistentIdOverride) ? string.Empty : e.persistentIdOverride)).Append(',')
                                            .Append(E(e.audio ? AssetDatabase.GetAssetPath(e.audio) : string.Empty));

                    foreach (var l in langs)
                    {
                        string text = _locDb.Get(e.key, l, langs.Count > 0 ? langs[0] : null);
                        sb.Append(',').Append(E(text ?? string.Empty));
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
                EditorUtility.RevealInFinder(path);
                Debug.Log($"[{Title}] Exported combined CSV to {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Title}] Export failed: {ex.Message}");
            }
        }

        private void ImportCombinedCsv(bool dryRun)
        {
            if (!_msgDb || !_locDb)
            {
                EditorUtility.DisplayDialog(Title, "Asigna MessageDB y LocalizationDB.", "OK");
                return;
            }

            string startFolder = EditorPrefs.GetString(PrefsKeyFolder, Application.dataPath);
            string path = EditorUtility.OpenFilePanel("Import Combined CSV", startFolder, "csv");
            if (string.IsNullOrEmpty(path)) return;
            EditorPrefs.SetString(PrefsKeyFolder, Path.GetDirectoryName(path));

            try
            {
                string content = File.ReadAllText(path, Encoding.UTF8);
                var lines = SplitLines(content);
                if (lines.Count <= 1)
                {
                    EditorUtility.DisplayDialog(Title, "CSV vacío o sin filas.", "OK");
                    return;
                }

                var header = ParseCsvLine(lines[0]);
                // Mapear columnas por nombre (flexible; 'repeatable' opcional)
                var colIndex = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < header.Count; i++)
                {
                    var name = (header[i] ?? string.Empty).Trim();
                    if (!string.IsNullOrEmpty(name) && !colIndex.ContainsKey(name)) colIndex[name] = i;
                }
                string[] required = { "id", "type", "oneShot" };
                foreach (var req in required)
                {
                    if (!colIndex.ContainsKey(req)) { EditorUtility.DisplayDialog(Title, $"CSV inválido: falta columna '{req}'", "OK"); return; }
                }
                // Idiomas importados = columnas desconocidas después de las conocidas base
                var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "id","type","key","autoClose","typewriterCps","repeatable","oneShot","persistentId","audioPath" };
                var importLangs = new List<string>();
                for (int i = 0; i < header.Count; i++)
                {
                    var h = (header[i] ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(h)) continue;
                    if (!known.Contains(h)) importLangs.Add(h);
                }

                // Merge de idiomas en LocalizationDB
                var langs = new List<string>(GetLanguages(_locDb));
                foreach (var l in importLangs)
                {
                    if (!langs.Any(x => string.Equals(x, l, StringComparison.OrdinalIgnoreCase)))
                        langs.Add(l);
                }

                // Backup opcional
                if (!dryRun)
                {
                    CreateAssetBackup(_msgDb);
                    CreateAssetBackup(_locDb);
                    Undo.RecordObject(_msgDb, "Import Messages CSV");
                    Undo.RecordObject(_locDb, "Import Localization CSV");
                    SetLanguages(_locDb, langs);
                }

                // Índice de entradas por id en MessageDB
                var idToEntry = BuildIdIndex(_msgDb);

                int createdMsgs = 0, updatedMsgs = 0, createdKeys = 0, updatedKeys = 0;

                // Procesar filas
                for (int li = 1; li < lines.Count; li++)
                {
                    if (string.IsNullOrWhiteSpace(lines[li])) continue;
                    var cols = ParseCsvLine(lines[li]);
                    // id
                    string id = Get(cols, colIndex, "id");
                    if (string.IsNullOrEmpty(id)) continue;

                    string typeStr = (Get(cols, colIndex, "type") ?? "lower").Trim().ToLowerInvariant();
                    MsgType type = typeStr.StartsWith("o") ? MsgType.Oniric : MsgType.Lower;
                    string key = (Get(cols, colIndex, "key") ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(key)) key = $"msg.{(type == MsgType.Lower ? "lower" : "oniric")}.{id}";

                    float autoClose = ParseFloat(Get(cols, colIndex, "autoClose"), -1f);
                    float cps = ParseFloat(Get(cols, colIndex, "typewriterCps"), -1f);
                    // repeatable (legacy) opcional, se ignora en runtime; aceptamos pero no es requerido
                    bool oneShot = ParseBool(Get(cols, colIndex, "oneShot"));
                    string persistentId = (Get(cols, colIndex, "persistentId") ?? string.Empty).Trim();
                    if (oneShot && string.IsNullOrEmpty(persistentId)) persistentId = id; // autogen
                    string audioPath = (Get(cols, colIndex, "audioPath") ?? string.Empty).Trim();
                    AudioClip audio = string.IsNullOrEmpty(audioPath) ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);

                    // Mensajes: crear/actualizar
                    var e = idToEntry.TryGetValue(id, out var existing) ? existing : null;
                    bool created = false;
                    if (e == null)
                    {
                        if (!dryRun)
                        {
                            e = new MessageDBSO.Entry
                            {
                                id = id,
                                type = type,
                                key = key,
                                autoCloseSeconds = autoClose,
                                typewriterCps = cps,
                                oneShot = oneShot,
                                persistentIdOverride = string.IsNullOrEmpty(persistentId) ? null : persistentId,
                                audio = audio,
                                localizedAudio = new List<MessageDBSO.LocalizedAudio>()
                            };
                            AddEntry(_msgDb, e);
                            idToEntry[id] = e;
                        }
                        created = true;
                        createdMsgs++;
                    }
                    else
                    {
                        if (!dryRun)
                        {
                            e.type = type;
                            e.key = key;
                            e.autoCloseSeconds = autoClose;
                            e.typewriterCps = cps;
                            e.oneShot = oneShot;
                            e.persistentIdOverride = string.IsNullOrEmpty(persistentId) ? null : persistentId;
                            if (audio != null) e.audio = audio; // no destructivo
                            EditorUtility.SetDirty(_msgDb);
                        }
                        updatedMsgs++;
                    }

                    // Localización: aplicar celdas no vacías
                    for (int i = 0; i < importLangs.Count; i++)
                    {
                        string lang = importLangs[i];
                        // Si la columna existe, usamos su índice; si no, saltamos
                        int ci;
                        if (!colIndex.TryGetValue(lang, out ci)) continue;
                        string text = (ci < cols.Count) ? cols[ci] : null;
                        if (string.IsNullOrEmpty(lang) || string.IsNullOrEmpty(text)) continue;
                        ApplyLocalizationCell(_locDb, key, lang, text, dryRun, ref createdKeys, ref updatedKeys);
                    }
                }

                if (!dryRun)
                {
                    EditorUtility.SetDirty(_msgDb);
                    EditorUtility.SetDirty(_locDb);
                    AssetDatabase.SaveAssets();
                    RebuildIdCache();
                }

                EditorUtility.DisplayDialog(Title,
                    dryRun
                        ? $"Dry‑Run OK\nMsgs: +{createdMsgs} / ~{updatedMsgs}\nLoc cells: +{createdKeys} / ~{updatedKeys}"
                        : $"Import OK\nMsgs: +{createdMsgs} / ~{updatedMsgs}\nLoc cells: +{createdKeys} / ~{updatedKeys}",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Title}] Import failed: {ex.Message}");
            }
        }

        // Single Entry ----------------------------------------------------
        private void DrawSingleEntry()
        {
            if (!_msgDb || !_locDb)
            {
                EditorGUILayout.HelpBox("Asigna MessageDB y LocalizationDB.", UnityEditor.MessageType.Warning);
                return;
            }
            EditorGUILayout.LabelField("Create / Update Single Entry", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _newId = EditorGUILayout.TextField("id", _newId);
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newId)))
            {
                if (GUILayout.Button("Copy", GUILayout.Width(56)))
                {
                    EditorGUIUtility.systemCopyBuffer = _newId;
                }
                if (GUILayout.Button("Assign->Selected", GUILayout.Width(120)))
                {
                    var go = Selection.activeGameObject;
                    if (go != null)
                    {
                        var dbi = go.GetComponent<DBTextInteractable>();
                        if (dbi != null)
                        {
                            Undo.RecordObject(dbi, "Assign MessageId");
                            dbi.MessageId = _newId;
                            EditorUtility.SetDirty(dbi);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(Title, "El objeto seleccionado no tiene DBTextInteractable.", "OK");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(Title, "No hay un GameObject seleccionado en la jerarquía.", "OK");
                    }
                }
            }
            if (GUILayout.Button("GUID", GUILayout.Width(60)))
            {
                _newId = ("id_" + System.Guid.NewGuid().ToString("N").Substring(0, 8)).ToLowerInvariant();
            }
            EditorGUILayout.EndHorizontal();
            _newType = (MsgType)EditorGUILayout.EnumPopup("type", _newType);
            _newKey = EditorGUILayout.TextField("key (auto si vacío)", _newKey);
            _newAutoClose = EditorGUILayout.FloatField("autoClose", _newAutoClose);
            _newCps = EditorGUILayout.FloatField("typewriterCps", _newCps);
            _newOneShot = EditorGUILayout.Toggle("oneShot", _newOneShot);
            using (new EditorGUI.DisabledScope(!_newOneShot))
            {
                var hint = _newOneShot && string.IsNullOrWhiteSpace(_newPersistentId) && !string.IsNullOrWhiteSpace(_newId) ? $"(default: {_newId})" : "";
                _newPersistentId = EditorGUILayout.TextField($"persistentId {hint}", _newPersistentId);
            }
            _newAudio = (AudioClip)EditorGUILayout.ObjectField("audio (default)", _newAudio, typeof(AudioClip), false);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Localization Texts", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                foreach (var lang in GetLanguages(_locDb))
                {
                    if (!_perLangText.ContainsKey(lang)) _perLangText[lang] = string.Empty;
                    _perLangText[lang] = EditorGUILayout.TextField(lang, _perLangText[lang]);
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Entry", GUILayout.Height(26)))
            {
                SaveSingleEntry();
            }
            if (GUILayout.Button("Clear Form", GUILayout.Height(26)))
            {
                ClearSingleForm();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SaveSingleEntry()
        {
            if (string.IsNullOrWhiteSpace(_newId)) { EditorUtility.DisplayDialog(Title, "id requerido", "OK"); return; }
            string key = string.IsNullOrWhiteSpace(_newKey) ? $"msg.{(_newType == MsgType.Lower ? "lower" : "oniric")}.{_newId}" : _newKey.Trim();

            var idIndex = BuildIdIndex(_msgDb);
            if (!idIndex.TryGetValue(_newId, out var e))
            {
                e = new MessageDBSO.Entry { id = _newId, localizedAudio = new List<MessageDBSO.LocalizedAudio>() };
                AddEntry(_msgDb, e);
            }

            Undo.RecordObject(_msgDb, "Save Message Entry");
            e.type = _newType;
            e.key = key;
            e.autoCloseSeconds = _newAutoClose;
            e.typewriterCps = _newCps;
            e.oneShot = _newOneShot;
            // Autogen de persistentId si es oneShot y está vacío
            var pid = _newOneShot && string.IsNullOrWhiteSpace(_newPersistentId) ? _newId : _newPersistentId;
            e.persistentIdOverride = string.IsNullOrWhiteSpace(pid) ? null : pid;
            if (_newAudio) e.audio = _newAudio;
            EditorUtility.SetDirty(_msgDb);

            // Localización
            Undo.RecordObject(_locDb, "Save Localization Texts");
            int cNew = 0, cUpd = 0;
            foreach (var kv in _perLangText)
            {
                var lang = kv.Key; var text = kv.Value;
                if (string.IsNullOrEmpty(text)) continue;
                ApplyLocalizationCell(_locDb, key, lang, text, dryRun: false, ref cNew, ref cUpd);
            }
            EditorUtility.SetDirty(_locDb);
            AssetDatabase.SaveAssets();
            RebuildIdCache();
            // Copy id to clipboard for quick paste into interactables
            EditorGUIUtility.systemCopyBuffer = _newId;
            EditorUtility.DisplayDialog(Title, $"Entry guardado. id='{_newId}' (copiado al portapapeles). Loc +{cNew}/~{cUpd}", "OK");
            // Clear only the id to prevent accidental reuse; keep other fields for convenience
            _newId = string.Empty;
        }

        private void ClearSingleForm()
        {
            _newId = ""; _newType = MsgType.Lower; _newKey = ""; _newAutoClose = -1f; _newCps = -1f;
            _newOneShot = false; _newPersistentId = ""; _newAudio = null; _perLangText.Clear();
        }

        private void TryAutoWireDatabases()
        {
            if (!_msgDb)
            {
                var guids = AssetDatabase.FindAssets("t:Game.Messages.MessageDBSO");
                if (guids != null && guids.Length > 0)
                {
                    _msgDb = AssetDatabase.LoadAssetAtPath<MessageDBSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
            if (!_locDb)
            {
                var guids = AssetDatabase.FindAssets("t:Game.Localization.LocalizationDBSO");
                if (guids != null && guids.Length > 0)
                {
                    _locDb = AssetDatabase.LoadAssetAtPath<LocalizationDBSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
        }

        private void RebuildIdCache()
        {
            if (_msgDb == null)
            {
                _cachedIds = new List<string> { string.Empty };
                return;
            }
            _cachedIds = GetEntries(_msgDb)
                .Select(e => e.id)
                .Where(s => !string.IsNullOrEmpty(s))
                .OrderBy(s => s)
                .ToList();
            _cachedIds.Insert(0, string.Empty);
        }

        // Low-level helpers -----------------------------------------------
        private static List<string> GetLanguages(LocalizationDBSO db)
        {
            var prop = typeof(LocalizationDBSO).GetProperty("Languages");
            var langs = ((IReadOnlyList<string>)prop.GetValue(db))?.ToList() ?? new List<string>();
            return langs;
        }

        private static void SetLanguages(LocalizationDBSO db, List<string> langs)
        {
            var field = typeof(LocalizationDBSO).GetField("languages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(db, langs);
            EditorUtility.SetDirty(db);
        }

        private static IEnumerable<MessageDBSO.Entry> GetEntries(MessageDBSO db)
        {
            if (!db) yield break;
            var field = typeof(MessageDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = field?.GetValue(db) as List<MessageDBSO.Entry>;
            if (list == null) yield break;
            foreach (var e in list) if (e != null) yield return e;
        }

        private static Dictionary<string, MessageDBSO.Entry> BuildIdIndex(MessageDBSO db)
        {
            var dict = new Dictionary<string, MessageDBSO.Entry>(StringComparer.Ordinal);
            foreach (var e in GetEntries(db))
            {
                if (!string.IsNullOrEmpty(e.id) && !dict.ContainsKey(e.id)) dict.Add(e.id, e);
            }
            return dict;
        }

        private static void AddEntry(MessageDBSO db, MessageDBSO.Entry e)
        {
            var field = typeof(MessageDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = field?.GetValue(db) as List<MessageDBSO.Entry>;
            if (list == null) return;
            list.Add(e);
            EditorUtility.SetDirty(db);
        }

        private static void ApplyLocalizationCell(LocalizationDBSO db, string key, string lang, string text, bool dryRun, ref int created, ref int updated)
        {
            // Obtener lista entries (privada)
            var entriesField = typeof(LocalizationDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var entries = entriesField?.GetValue(db) as System.Collections.IList;
            if (entries == null) return;

            // Buscar entrada por key
            object entryObj = null; int entryIndex = -1;
            for (int i = 0; i < entries.Count; i++)
            {
                var it = entries[i]; if (it == null) continue;
                var et = it.GetType(); var k = (string)et.GetField("key").GetValue(it);
                if (string.Equals(k, key, StringComparison.Ordinal)) { entryObj = it; entryIndex = i; break; }
            }

            if (entryObj == null)
            {
                if (dryRun) { created++; return; }
                // Crear nueva entry
                var entryType = entries.GetType().GetGenericArguments()[0];
                entryObj = Activator.CreateInstance(entryType);
                entryType.GetField("key").SetValue(entryObj, key);
                var list = new List<LocalizationDBSO.LocalizedValue>();
                entryType.GetField("values").SetValue(entryObj, list);
                entries.Add(entryObj);
            }

            // Map language->value
            var eType = entryObj.GetType();
            var values = eType.GetField("values").GetValue(entryObj) as System.Collections.IList;
            object lvObj = null;
            if (values != null)
            {
                for (int j = 0; j < values.Count; j++)
                {
                    var v = values[j]; if (v == null) continue;
                    var vt = v.GetType(); var L = (string)vt.GetField("language").GetValue(v);
                    if (string.Equals(L, lang, StringComparison.OrdinalIgnoreCase)) { lvObj = v; break; }
                }
            }
            if (lvObj == null)
            {
                if (dryRun) { created++; return; }
                var lv = new LocalizationDBSO.LocalizedValue { language = lang, text = text };
                values.Add(lv);
                EditorUtility.SetDirty(db);
                created++;
            }
            else
            {
                if (dryRun) { updated++; return; }
                var vt = lvObj.GetType();
                vt.GetField("text").SetValue(lvObj, text);
                EditorUtility.SetDirty(db);
                updated++;
            }
        }

        private static void CreateAssetBackup(UnityEngine.Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) return;
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string bak = Path.Combine(dir, $"{name}.{DateTime.Now:yyyyMMdd_HHmmss}.bak.asset");
            AssetDatabase.CopyAsset(path, bak);
        }

        // CSV helpers -----------------------------------------------------
        private static string E(string s)
        {
            if (s == null) s = string.Empty;
            bool needQuotes = s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r");
            s = s.Replace("\"", "\"\"");
            return needQuotes ? "\"" + s + "\"" : s;
        }

        private static List<string> SplitLines(string content)
        {
            var list = new List<string>();
            using (var sr = new StringReader(content))
            {
                string line; bool first = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (first && !string.IsNullOrEmpty(line) && line[0] == '\uFEFF') line = line.Substring(1);
                    list.Add(line); first = false;
                }
            }
            return list;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>(); if (line == null) { result.Add(string.Empty); return result; }
            var sb = new StringBuilder(); bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        bool nextIsQuote = (i + 1 < line.Length) && line[i + 1] == '"';
                        if (nextIsQuote) { sb.Append('"'); i++; }
                        else { inQuotes = false; }
                    }
                    else sb.Append(c);
                }
                else
                {
                    if (c == ',') { result.Add(sb.ToString()); sb.Length = 0; }
                    else if (c == '"') { inQuotes = true; }
                    else sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result;
        }

        private static float ParseFloat(string s, float def)
        {
            if (string.IsNullOrWhiteSpace(s)) return def;
            if (float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)) return v;
            return def;
        }

        private static bool ParseBool(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().ToLowerInvariant();
            return s == "1" || s == "true" || s == "yes" || s == "y";
        }

        private static string Get(List<string> cols, Dictionary<string,int> idx, string name)
        {
            if (!idx.TryGetValue(name, out var i)) return null;
            if (i < 0 || i >= cols.Count) return null;
            return cols[i];
        }
    }
}
#endif
