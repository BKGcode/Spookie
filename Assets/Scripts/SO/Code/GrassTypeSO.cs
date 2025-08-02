using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "GrassType_", menuName = "Spookie/Terrain/Grass Type")]
    public class GrassTypeSO : ScriptableObject
    {
        [Tooltip("The name of this grass type (e.g., 'Regular', 'With Flowers')")]
        public string GrassName;

        [Tooltip("The color to use when representing this grass in the custom terrain editor.")]
        public Color EditorColor = Color.green;

        [Tooltip("The actual material used to render the grass surface.")]
        public Material Material;
    }
}
