#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Interaction.Editor
{
    public static class CreateInteractablePrefab
    {
        [MenuItem("Spookie/Create/Example Interactable Prefab", priority = 10)]
        public static void CreatePrefab()
        {
            // Create root object
            var root = new GameObject("Example_Interactable");

            // Try set layer to 'Interactable' if it exists
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer != -1)
            {
                root.layer = interactableLayer;
            }

            // Add mesh (Cube) for visibility
            var mf = root.AddComponent<MeshFilter>();
            mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var mr = root.AddComponent<MeshRenderer>();

            // Add collider so raycast can hit this object
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = false;

            // Add components
            var interactable = root.AddComponent<ExampleInteractable>();
            var highlighter = root.AddComponent<OutlineHighlighter>();

            // Add an AudioSource and assign to ExampleInteractable (clip remains to be assigned by user)
            var audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            {
                var so = new SerializedObject(interactable);
                so.FindProperty("audioSource").objectReferenceValue = audio;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Try to find an outline material at a known path
            var outlineMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ART/Shaders/Outline/URP_SilhouetteOutline.mat");
            if (outlineMat == null)
            {
                // Try create a material if shader exists
                var shader = Shader.Find("Spookie/URP/SilhouetteOutline");
                if (shader != null)
                {
                    outlineMat = new Material(shader);
                    AssetDatabase.CreateAsset(outlineMat, "Assets/ART/Shaders/Outline/URP_SilhouetteOutline.mat");
                }
            }
            if (outlineMat != null)
            {
                var so = new SerializedObject(highlighter);
                so.FindProperty("outlineMaterial").objectReferenceValue = outlineMat;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Create prefab asset in project
            var path = EditorUtility.SaveFilePanelInProject("Save Example Interactable Prefab", "Example_Interactable", "prefab", "Choose location for the prefab");
            if (!string.IsNullOrEmpty(path))
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, path, InteractionMode.AutomatedAction);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            Object.DestroyImmediate(root);
        }
    }
}
#endif
