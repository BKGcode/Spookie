// #if UNITY_EDITOR
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using Game.Interaction; // DBTextInteractable
// using Game.UI; // MessageTrigger

// namespace Game.EditorTools
// {
//     /// <summary>
//     /// Unified maintenance tools for clearing development/testing data (PlayerPrefs).
//     /// </summary>
//     internal static class SpookieMaintenanceTools
//     {
//         // --- Constants from OneShotCleaner ---
//         private const string MSG_PREFIX = "Spookie.MsgSeen.";      // Legacy Lower
//         private const string PO_PREFIX  = "Spookie.PO.";          // Legacy Oniric
//         private const string NEW_LOWER_PREFIX  = "spk.lower.seen.";   // Orchestrator Lower
//         private const string NEW_ONIRIC_PREFIX = "spk.oniric.seen.";  // Orchestrator Oniric

//         // --- Constants from SpookieTools ---
//         private const string PP_DayKey = "DN_CurrentDay";
//         private const string PP_PenaltyKey = "DN_NextDayPenalty";
//         private const string PP_PenaltyUntilKey = "DN_NextDayPenalty_UntilSeconds";

//         // --- Menu Items from OneShotCleaner ---

//         [MenuItem("Tools/Spookie/Maintenance/Clear One-Shot Flags (Open Scenes)")]
//         public static void ClearOpenScenes()
//         {
//             if (!EditorUtility.DisplayDialog("Clear One-Shot Flags",
//                 "This will clear one-shot message flags found in open scenes.\nContinue?",
//                 "Yes, Clear", "Cancel"))
//             {
//                 return;
//             }

//             var lowerIds = new HashSet<string>();
//             var poIds = new HashSet<string>();
//             CollectFromOpenScenes(lowerIds, poIds);
//             int removed = 0;
//             removed += DeleteKeys(lowerIds, MSG_PREFIX);
//             removed += DeleteKeys(lowerIds, NEW_LOWER_PREFIX);
//             removed += DeleteKeys(poIds, PO_PREFIX);
//             removed += DeleteKeys(poIds, NEW_ONIRIC_PREFIX);

//             PlayerPrefs.Save();
//             EditorUtility.DisplayDialog("Clear One-Shot Flags",
//                 $"Flags cleared: {removed}\nLower IDs in scenes: {lowerIds.Count}\nP.O. IDs in scenes: {poIds.Count}",
//                 "OK");
//         }

//         [MenuItem("Tools/Spookie/Maintenance/Clear One-Shot Flags (Open Scenes + Prefabs)")]
//         public static void ClearOpenScenesAndPrefabs()
//         {
//             if (!EditorUtility.DisplayDialog("Clear One-Shot Flags (All)",
//                 "This will find IDs in open scenes and all project prefabs, then clear their one-shot flags.\nContinue?",
//                 "Yes, Clear", "Cancel"))
//             {
//                 return;
//             }

//             var lowerIds = new HashSet<string>();
//             var poIds = new HashSet<string>();
//             CollectFromOpenScenes(lowerIds, poIds);
//             CollectFromPrefabs(lowerIds, poIds);

//             int removed = 0;
//             removed += DeleteKeys(lowerIds, MSG_PREFIX);
//             removed += DeleteKeys(lowerIds, NEW_LOWER_PREFIX);
//             removed += DeleteKeys(poIds, PO_PREFIX);
//             removed += DeleteKeys(poIds, NEW_ONIRIC_PREFIX);
//             PlayerPrefs.Save();

//             EditorUtility.DisplayDialog("Clear One-Shot Flags (All)",
//                 $"Flags cleared: {removed}\nTotal Lower IDs: {lowerIds.Count}\nTotal P.O. IDs: {poIds.Count}",
//                 "OK");
//         }

//         // --- Menu Items from SpookieTools ---

//         [MenuItem("Tools/Spookie/Maintenance/Clear DayNight Penalty")]
//         private static void ClearDayNightPenalty()
//         {
//             PlayerPrefs.SetInt(PP_PenaltyKey, 0);
//             PlayerPrefs.DeleteKey(PP_PenaltyUntilKey);
//             PlayerPrefs.Save();
//             Debug.Log("[SpookieTools] Cleared DayNight penalty (sprint re-enabled next play).");
//         }

//         [MenuItem("Tools/Spookie/Maintenance/Reset DayNight State")]
//         private static void ResetDayNightState()
//         {
//             PlayerPrefs.DeleteKey(PP_DayKey);
//             PlayerPrefs.SetInt(PP_PenaltyKey, 0);
//             PlayerPrefs.DeleteKey(PP_PenaltyUntilKey);
//             PlayerPrefs.Save();
//             Debug.Log("[SpookieTools] Reset DayNight state and cleared penalty.");
//         }

//         [MenuItem("Tools/Spookie/Maintenance/Clear All PlayerPrefs [DANGER]")]
//         public static void ClearAllPlayerPrefs()
//         {
//             if (!EditorUtility.DisplayDialog("Clear All PlayerPrefs?",
//                 "This will call PlayerPrefs.DeleteAll() for this project (Company/Product). This is irreversible.\nContinue?", "Yes, Delete All", "Cancel"))
//             {
//                 return;
//             }
//             PlayerPrefs.DeleteAll();
//             PlayerPrefs.Save();
//             Debug.Log("[SpookieTools] PlayerPrefs.DeleteAll() executed.");
//         }


//         // --- Helper Methods from OneShotCleaner ---

//         private static void CollectFromOpenScenes(HashSet<string> lowerIds, HashSet<string> poIds)
//         {
//             int count = EditorSceneManager.sceneCount;
//             for (int i = 0; i < count; i++)
//             {
//                 var scene = EditorSceneManager.GetSceneAt(i);
//                 if (!scene.isLoaded) continue;
//                 foreach (var go in scene.GetRootGameObjects())
//                 {
//                     CollectFromGO(go, lowerIds, poIds);
//                 }
//             }
//         }

//         private static void CollectFromPrefabs(HashSet<string> lowerIds, HashSet<string> poIds)
//         {
//             var guids = AssetDatabase.FindAssets("t:Prefab");
//             foreach (var guid in guids)
//             {
//                 var path = AssetDatabase.GUIDToAssetPath(guid);
//                 var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
//                 if (prefab == null) continue;
//                 CollectFromGO(prefab, lowerIds, poIds);
//             }
//         }

//         private static void CollectFromGO(GameObject root, HashSet<string> lowerIds, HashSet<string> poIds)
//         {
//             var msgComps = root.GetComponentsInChildren<DBTextInteractable>(true);
//             foreach (var c in msgComps)
//             {
//                 var persist = GetPrivateString(c, "persistentIdOverride");
//                 if (string.IsNullOrWhiteSpace(persist))
//                 {
//                     persist = GetPrivateString(c, "messageId");
//                 }
//                 if (!string.IsNullOrWhiteSpace(persist)) lowerIds.Add(persist);
//             }

//             var trigComps = root.GetComponentsInChildren<MessageTrigger>(true);
//             foreach (var t in trigComps)
//             {
//                 var id = GetPrivateString(t, "messageId");
//                 if (!string.IsNullOrWhiteSpace(id)) poIds.Add(id);
//             }
//         }

//         private static int DeleteKeys(HashSet<string> ids, string prefix)
//         {
//             int removed = 0;
//             foreach (var id in ids)
//             {
//                 string key = prefix + id;
//                 if (PlayerPrefs.HasKey(key))
//                 {
//                     PlayerPrefs.DeleteKey(key);
//                     removed++;
//                 }
//             }
//             return removed;
//         }

//         private static string GetPrivateString(Object obj, string field)
//         {
//             if (obj == null) return null;
//             var so = new SerializedObject(obj);
//             var sp = so.FindProperty(field);
//             if (sp != null && sp.propertyType == SerializedPropertyType.String)
//             {
//                 return sp.stringValue;
//             }
//             return null;
//         }
//     }
// }
// #endif
