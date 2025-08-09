// SPDX-License-Identifier: MIT
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Game.Localization;

namespace Game.EditorTools.Localization
{
    /// <summary>
    /// Minimal CSV export/import for LocalizationDBSO.
    /// Header: key,<lang1>,<lang2>,...
    /// Merge by key on import; only non-empty cells overwrite; adds missing keys/languages.
    /// KISS: no object pickers; uses selected DB or first found in project.
    /// </summary>
    public static class LocalizationCSVUtility
    {
        private const string MenuRoot = "Spookie/Localization/";

        [MenuItem(MenuRoot + "Export CSV (Selected DB)...", priority = 10)]
        public static void ExportCsv()
        {
            var db = GetSelectedOrFirstDB();
            if (db == null)
            {
                EditorUtility.DisplayDialog("Export CSV", "Select a LocalizationDBSO asset in Project view.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Export Localization CSV", Application.dataPath, "Localization", "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var sb = new StringBuilder();
                // Header
                sb.Append(E("key"));
                var langs = db.Languages;
                for (int i = 0; i < langs.Count; i++)
                {
                    sb.Append(',').Append(E(langs[i] ?? string.Empty));
                }
                sb.AppendLine();

                // Rows
                var entriesField = typeof(LocalizationDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = entriesField?.GetValue(db) as System.Collections.IList;
                if (entries != null)
                {
                    foreach (var obj in entries)
                    {
                        if (obj == null) continue;
                        var entryType = obj.GetType();
                        var key = (string)entryType.GetField("key").GetValue(obj);
                        var values = entryType.GetField("values").GetValue(obj) as System.Collections.IList;
                        sb.Append(E(key ?? string.Empty));
                        for (int i = 0; i < langs.Count; i++)
                        {
                            string lang = langs[i];
                            string text = FindText(values, lang);
                            sb.Append(',').Append(E(text ?? string.Empty));
                        }
                        sb.AppendLine();
                    }
                }

                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true)); // BOM for Excel friendliness
                EditorUtility.RevealInFinder(path);
                Debug.Log($"[LocalizationCSV] Exported to {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationCSV] Export failed: " + ex.Message);
            }
        }

        [MenuItem(MenuRoot + "Import CSV (Selected DB)...", priority = 11)]
        public static void ImportCsv()
        {
            var db = GetSelectedOrFirstDB();
            if (db == null)
            {
                EditorUtility.DisplayDialog("Import CSV", "Select a LocalizationDBSO asset in Project view.", "OK");
                return;
            }

            string path = EditorUtility.OpenFilePanel("Import Localization CSV", Application.dataPath, "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string content = File.ReadAllText(path, Encoding.UTF8);
                var lines = SplitLines(content);
                if (lines.Count == 0)
                {
                    EditorUtility.DisplayDialog("Import CSV", "File is empty.", "OK");
                    return;
                }

                // Header
                var header = ParseCsvLine(lines[0]);
                if (header.Count < 1 || !string.Equals(header[0], "key", StringComparison.OrdinalIgnoreCase))
                {
                    EditorUtility.DisplayDialog("Import CSV", "Invalid header. First column must be 'key'.", "OK");
                    return;
                }
                var importLangs = new List<string>();
                for (int i = 1; i < header.Count; i++)
                {
                    var lang = (header[i] ?? string.Empty).Trim();
                    if (!string.IsNullOrEmpty(lang)) importLangs.Add(lang);
                }

                // Ensure db has languages (merge)
                var langs = new List<string>(db.Languages);
                foreach (var l in importLangs)
                {
                    bool exists = langs.Exists(x => string.Equals(x, l, StringComparison.OrdinalIgnoreCase));
                    if (!exists) langs.Add(l);
                }

                Undo.RecordObject(db, "Import Localization CSV");
                // Set back languages via reflection (Languages is read-only)
                var langsField = typeof(LocalizationDBSO).GetField("languages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                langsField?.SetValue(db, langs);

                // Access entries list
                var entriesField = typeof(LocalizationDBSO).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = entriesField?.GetValue(db) as System.Collections.IList;
                if (entries == null)
                {
                    EditorUtility.DisplayDialog("Import CSV", "DB has no entries list.", "OK");
                    return;
                }

                // Map keys to entries for quick lookup
                var keyToEntry = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var obj in entries)
                {
                    if (obj == null) continue;
                    var entryType = obj.GetType();
                    var key = (string)entryType.GetField("key").GetValue(obj);
                    if (!string.IsNullOrEmpty(key) && !keyToEntry.ContainsKey(key))
                        keyToEntry.Add(key, obj);
                }

                // Process rows
                for (int li = 1; li < lines.Count; li++)
                {
                    if (string.IsNullOrWhiteSpace(lines[li])) continue;
                    var cols = ParseCsvLine(lines[li]);
                    if (cols.Count == 0) continue;
                    var key = cols[0]?.Trim();
                    if (string.IsNullOrEmpty(key)) continue;

                    // Get or create entry
                    object entryObj;
                    if (!keyToEntry.TryGetValue(key, out entryObj))
                    {
                        // Create new Entry instance via reflection
                        var entryType = entries.GetType().GetGenericArguments()[0];
                        entryObj = Activator.CreateInstance(entryType);
                        entryType.GetField("key").SetValue(entryObj, key);
                        entryType.GetField("values").SetValue(entryObj, new List<LocalizationDBSO.LocalizedValue>());
                        entries.Add(entryObj);
                        keyToEntry.Add(key, entryObj);
                    }

                    var eType = entryObj.GetType();
                    var values = eType.GetField("values").GetValue(entryObj) as System.Collections.IList;
                    if (values == null) continue;

                    // Build map language->LocalizedValue
                    var langToVal = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var vobj in values)
                    {
                        if (vobj == null) continue;
                        var vt = vobj.GetType();
                        var lang = (string)vt.GetField("language").GetValue(vobj);
                        if (string.IsNullOrEmpty(lang)) continue;
                        if (!langToVal.ContainsKey(lang)) langToVal.Add(lang, vobj);
                    }

                    // Apply non-empty cells
                    for (int ci = 1; ci < cols.Count && (ci - 1) < importLangs.Count; ci++)
                    {
                        string lang = importLangs[ci - 1];
                        string text = cols[ci];
                        if (string.IsNullOrEmpty(lang) || string.IsNullOrEmpty(text)) continue; // non-destructive

                        if (langToVal.TryGetValue(lang, out var lvObj))
                        {
                            var vt = lvObj.GetType();
                            vt.GetField("text").SetValue(lvObj, text);
                        }
                        else
                        {
                            // Create new LocalizedValue
                            var lv = new LocalizationDBSO.LocalizedValue { language = lang, text = text };
                            values.Add(lv);
                            langToVal[lang] = lv;
                        }
                    }
                }

                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();
                Debug.Log($"[LocalizationCSV] Imported from {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationCSV] Import failed: " + ex.Message);
            }
        }

        // Helpers ----------------------------------------------------------

        private static LocalizationDBSO GetSelectedOrFirstDB()
        {
            var db = Selection.activeObject as LocalizationDBSO;
            if (db != null) return db;
            var guids = AssetDatabase.FindAssets("t:Game.Localization.LocalizationDBSO");
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<LocalizationDBSO>(path);
            }
            return null;
        }

        private static string E(string s)
        {
            if (s == null) s = string.Empty;
            bool needQuotes = s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r");
            s = s.Replace("\"", "\"\"");
            return needQuotes ? "\"" + s + "\"" : s;
        }

        private static string FindText(System.Collections.IList values, string language)
        {
            if (values == null || string.IsNullOrEmpty(language)) return null;
            foreach (var vobj in values)
            {
                if (vobj == null) continue;
                var vt = vobj.GetType();
                var lang = (string)vt.GetField("language").GetValue(vobj);
                if (string.Equals(lang, language, StringComparison.OrdinalIgnoreCase))
                {
                    return (string)vt.GetField("text").GetValue(vobj);
                }
            }
            return null;
        }

        private static List<string> SplitLines(string content)
        {
            var list = new List<string>();
            using (var sr = new StringReader(content))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Remove BOM from first line if present
                    if (list.Count == 0 && !string.IsNullOrEmpty(line) && line[0] == '\uFEFF')
                        line = line.Substring(1);
                    list.Add(line);
                }
            }
            return list;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(line)) { result.Add(string.Empty); return result; }

            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '\"')
                    {
                        bool nextIsQuote = (i + 1 < line.Length) && line[i + 1] == '\"';
                        if (nextIsQuote)
                        {
                            sb.Append('\"'); // escaped quote
                            i++; // skip next
                        }
                        else
                        {
                            inQuotes = false; // end quote
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    else if (c == '\"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            result.Add(sb.ToString());
            return result;
        }
    }
}
#endif
