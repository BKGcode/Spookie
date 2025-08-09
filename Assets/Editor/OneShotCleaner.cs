#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Interaction; // TextMessageInteractable, OniricOverlayTrigger
using Game.UI; // OniricOverlayTrigger

namespace Game.EditorTools
{
    internal static class OneShotCleaner
    {
        private const string MSG_PREFIX = "Spookie.MsgSeen."; // TextMessageInteractable
        private const string PO_PREFIX  = "Spookie.PO.";       // OniricOverlayController/Trigger

        [MenuItem("Spookie/Testing/Clear One-Shot Flags (Open Scenes)")] 
        public static void ClearOpenScenes()
        {
            if (!EditorUtility.DisplayDialog("Clear One-Shot Flags",
                "Esto borrará las flags de mensajes únicos (Lower y P.O.) encontradas en las escenas abiertas.\n¿Continuar?",
                "Sí, borrar", "Cancelar"))
            {
                return;
            }

            var lowerIds = new HashSet<string>();
            var poIds = new HashSet<string>();
            CollectFromOpenScenes(lowerIds, poIds);
            int removed = DeleteKeys(lowerIds, MSG_PREFIX) + DeleteKeys(poIds, PO_PREFIX);

            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Clear One-Shot Flags",
                $"Flags borradas: {removed}\nLower IDs en escenas: {lowerIds.Count}\nP.O. IDs en escenas: {poIds.Count}",
                "OK");
        }

        [MenuItem("Spookie/Testing/Clear One-Shot Flags (Open Scenes + Prefabs)")] 
        public static void ClearOpenScenesAndPrefabs()
        {
            if (!EditorUtility.DisplayDialog("Clear One-Shot Flags (All)",
                "Esto buscará IDs en escenas abiertas y en todos los prefabs del proyecto, y borrará sus flags one-shot.\n¿Continuar?",
                "Sí, borrar", "Cancelar"))
            {
                return;
            }

            var lowerIds = new HashSet<string>();
            var poIds = new HashSet<string>();
            CollectFromOpenScenes(lowerIds, poIds);
            CollectFromPrefabs(lowerIds, poIds);

            int removed = DeleteKeys(lowerIds, MSG_PREFIX) + DeleteKeys(poIds, PO_PREFIX);
            PlayerPrefs.Save();

            EditorUtility.DisplayDialog("Clear One-Shot Flags (All)",
                $"Flags borradas: {removed}\nLower IDs totales: {lowerIds.Count}\nP.O. IDs totales: {poIds.Count}",
                "OK");
        }

        [MenuItem("Spookie/Testing/Clear PlayerPrefs (ALL) [DANGER]")]
        public static void ClearAllPlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog("Clear PlayerPrefs (ALL)",
                "BORRARÁ TODAS las PlayerPrefs. Úsalo solo en desarrollo.",
                "Entiendo, borrar todo", "Cancelar"))
            {
                return;
            }
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("PlayerPrefs", "Todas las PlayerPrefs han sido borradas.", "OK");
        }

        private static void CollectFromOpenScenes(HashSet<string> lowerIds, HashSet<string> poIds)
        {
            int count = EditorSceneManager.sceneCount;
            for (int i = 0; i < count; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var go in scene.GetRootGameObjects())
                {
                    CollectFromGO(go, lowerIds, poIds);
                }
            }
        }

        private static void CollectFromPrefabs(HashSet<string> lowerIds, HashSet<string> poIds)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                CollectFromGO(prefab, lowerIds, poIds);
            }
        }

        private static void CollectFromGO(GameObject root, HashSet<string> lowerIds, HashSet<string> poIds)
        {
            var msgComps = root.GetComponentsInChildren<TextMessageInteractable>(true);
            foreach (var c in msgComps)
            {
                var id = GetPrivateString(c, "persistentId");
                if (!string.IsNullOrWhiteSpace(id)) lowerIds.Add(id);
            }

            var poComps = root.GetComponentsInChildren<OniricOverlayTrigger>(true);
            foreach (var c in poComps)
            {
                var id = GetPrivateString(c, "persistentId");
                if (!string.IsNullOrWhiteSpace(id)) poIds.Add(id);
            }
        }

        private static int DeleteKeys(HashSet<string> ids, string prefix)
        {
            int removed = 0;
            foreach (var id in ids)
            {
                string key = prefix + id;
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                    removed++;
                }
            }
            return removed;
        }

        // Access serialized private fields safely in Editor without reflection harshness
        private static string GetPrivateString(Object obj, string field)
        {
            if (obj == null) return null;
            var so = new SerializedObject(obj);
            var sp = so.FindProperty(field);
            if (sp != null && sp.propertyType == SerializedPropertyType.String)
            {
                return sp.stringValue;
            }
            return null;
        }
    }
}
#endif
