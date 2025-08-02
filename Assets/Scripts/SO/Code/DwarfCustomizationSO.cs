using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Shared;

namespace SO
{
    [CreateAssetMenu(fileName = "DwarfCustomization", menuName = "Spookie/Dwarfs/Customization")]
    public class DwarfCustomizationSO : ScriptableObject
    {
        [Serializable]
        public class DwarfIcon
        {
            [SerializeField]
            [Required]
            [Tooltip("The sprite to use for this icon")]
            private Sprite icon;
            public Sprite Icon => icon;
        }

        [Serializable]
        public class GenderedCustomization
        {
            [SerializeField]
            [Tooltip("List of possible names for this gender")]
            private List<string> names = new();
            public IReadOnlyList<string> Names => names;

            [SerializeField]
            [Tooltip("Available icons for this gender")]
            private List<DwarfIcon> icons = new();
            public List<DwarfIcon> Icons => icons;

            public string GetRandomName() => 
                names.Count > 0 ? names[UnityEngine.Random.Range(0, names.Count)] : "Unnamed Dwarf";

            public DwarfIcon GetRandomIcon()
            {
                // Filtrar los iconos válidos (con sprite asignado)
                var validIcons = icons.Where(i => i.Icon != null).ToList();
                return validIcons.Count > 0 ? validIcons[UnityEngine.Random.Range(0, validIcons.Count)] : null;
            }
        }

        [Header("Male Customization")]
        [SerializeField]
        private GenderedCustomization maleCustomization = new();
        public GenderedCustomization MaleCustomization => maleCustomization;

        [Header("Female Customization")]
        [SerializeField]
        private GenderedCustomization femaleCustomization = new();
        public GenderedCustomization FemaleCustomization => femaleCustomization;

        public DwarfGender GenerateRandomGender()
        {
            return UnityEngine.Random.value < 0.5f ? DwarfGender.Female : DwarfGender.Male;
        }

        private GenderedCustomization GetCustomizationForGender(DwarfGender gender)
        {
            return gender == DwarfGender.Female ? femaleCustomization : maleCustomization;
        }

        public string GenerateRandomName(DwarfGender gender)
        {
            return GetCustomizationForGender(gender).GetRandomName();
        }

        public Sprite GetRandomIcon(DwarfGender gender)
        {
            var icon = GetCustomizationForGender(gender).GetRandomIcon();
            return icon?.Icon;
        }

        private void OnValidate()
        {
            ValidateCustomization(maleCustomization, "Male");
            ValidateCustomization(femaleCustomization, "Female");
        }

        private void ValidateCustomization(GenderedCustomization customization, string genderName)
        {
            if (customization == null) return;

            // Solo advertir si no hay nombres definidos
            if (customization.Names.Count == 0)
            {
                Debug.LogWarning($"No names defined for {genderName} dwarfs, will use default name", this);
            }
        }
    }
}

// ScriptRole: Manages customization data for dwarfs including names and icons with gender-specific options
// Dependencies: Core.Shared (DwarfGender enum)
// Features:
//   - Fixed 50/50 gender distribution
//   - Gender-specific name and icon selection
//   - Validation for missing or invalid data
// NeedsSetup: 
//   1. Create via Assets > Create > Spookie > Dwarfs > Customization
//   2. Configure for each gender:
//      - List of names
//      - Icons with unique names and sprites
//   3. Set gender distribution and title probabilities