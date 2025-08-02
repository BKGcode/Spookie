using UnityEditor;
using UnityEngine;
using System.Linq;
using SO;

namespace EditorScripts
{
    /// <summary>
    /// Editor script that ensures every MaterialSO has a unique GUID.
    /// It runs automatically when assets are modified.
    /// </summary>
    [InitializeOnLoad]
    public class MaterialSOGuidAssigner
    {
        static MaterialSOGuidAssigner()
        {
            EditorApplication.delayCall += AssignGuidsToAllMaterials;
        }

        [MenuItem("Spookie/Tools/Assign Missing Material GUIDs")]
        private static void AssignGuidsToAllMaterials()
        {
            string[] guids = AssetDatabase.FindAssets("t:MaterialSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MaterialSO materialSO = AssetDatabase.LoadAssetAtPath<MaterialSO>(path);

                if (materialSO != null)
                {
                    if (string.IsNullOrEmpty(materialSO.Guid))
                    {
                        materialSO.GenerateGuid();
                        EditorUtility.SetDirty(materialSO);
                        Debug.Log($"Assigned new GUID to {materialSO.name}");
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
}