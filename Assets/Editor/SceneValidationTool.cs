#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.UI;
using Game.Messages;

namespace Game.EditorTools
{
    public static class SceneValidationTool
    {
    [MenuItem("Tools/Spookie/Testing/Validate Messages Scene", priority = 50)]
        public static void ValidateScene()
        {
            int sceneCount = EditorSceneManager.sceneCount;
            if (sceneCount == 0)
            {
                EditorUtility.DisplayDialog("Validate Scene", "No hay escenas abiertas.", "OK");
                return;
            }

            var sb = new StringBuilder();
            int totalErrors = 0, totalWarnings = 0;

            for (int i = 0; i < sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                int errors = 0, warnings = 0;
                sb.AppendLine($"[Scene] {scene.name}");

                // Find components in this scene (exclude assets/prefabs)
                T FindOne<T>() where T : Component
                {
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        var c = root.GetComponentInChildren<T>(true);
                        if (c != null) return c;
                    }
                    return null;
                }

                var pause = FindOne<PauseManager>();
                var lower = FindOne<LowerMessageController>();
                var oniric = FindOne<OniricOverlayController>();
                var orch = FindOne<MessageOrchestrator>();

                if (pause == null) { sb.AppendLine("  ERROR: Falta PauseManager"); errors++; }
                if (lower == null) { sb.AppendLine("  ERROR: Falta LowerMessageController"); errors++; }
                if (oniric == null) { sb.AppendLine("  ERROR: Falta OniricOverlayController"); errors++; }
                if (orch == null) { sb.AppendLine("  ERROR: Falta MessageOrchestrator"); errors++; }

                if (lower != null)
                {
                    if (GetPrivRef(lower, "messageText") == null) { sb.AppendLine("  ERROR: Lower.messageText sin asignar"); errors++; }
                    if (GetPrivRef(lower, "bannerRoot") == null) { sb.AppendLine("  WARNING: Lower.bannerRoot sin asignar"); warnings++; }
                    if (GetPrivRef(lower, "pauseManager") == null) { sb.AppendLine("  WARNING: Lower.pauseManager sin asignar"); warnings++; }
                }

                if (oniric != null)
                {
                    if (GetPrivRef(oniric, "oniricText") == null) { sb.AppendLine("  ERROR: Oniric.oniricText sin asignar"); errors++; }
                    if (GetPrivRef(oniric, "overlayRoot") == null) { sb.AppendLine("  WARNING: Oniric.overlayRoot sin asignar"); warnings++; }
                    if (GetPrivRef(oniric, "pauseManager") == null) { sb.AppendLine("  WARNING: Oniric.pauseManager sin asignar"); warnings++; }
                }

                if (orch != null)
                {
                    if (GetPrivRef(orch, "messageDB") == null) { sb.AppendLine("  ERROR: Orchestrator.messageDB sin asignar"); errors++; }
                    if (GetPrivRef(orch, "lower") == null) { sb.AppendLine("  ERROR: Orchestrator.lower sin asignar"); errors++; }
                    if (GetPrivRef(orch, "oniric") == null) { sb.AppendLine("  ERROR: Orchestrator.oniric sin asignar"); errors++; }
                    if (GetPrivRef(orch, "pauseManager") == null) { sb.AppendLine("  WARNING: Orchestrator.pauseManager sin asignar"); warnings++; }
                    // localizationDB/locale opcionales
                }

                sb.AppendLine($"  Result: {errors} error(es), {warnings} warning(s)");
                totalErrors += errors; totalWarnings += warnings;
                sb.AppendLine();
            }

            if (totalErrors == 0 && totalWarnings == 0)
            {
                EditorUtility.DisplayDialog("Validate Scene", "OK: Todo parece correcto.", "OK");
            }
            else
            {
                Debug.Log(sb.ToString());
                EditorUtility.DisplayDialog("Validate Scene",
                    $"Errores: {totalErrors}\nWarnings: {totalWarnings}\nDetalles en Console.", "OK");
            }
        }

        private static Object GetPrivRef(Object obj, string field)
        {
            if (obj == null) return null;
            var so = new SerializedObject(obj);
            var sp = so.FindProperty(field);
            if (sp == null) return null;
            if (sp.propertyType == SerializedPropertyType.ObjectReference) return sp.objectReferenceValue;
            return null;
        }
    }
}
#endif
