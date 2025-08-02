using UnityEngine;
using System;
using Terrain.Data;

namespace SO
{
    [CreateAssetMenu(fileName = "TerrainEditorSettings", menuName = "Spookie/Editor/Terrain Editor Settings")]
    public class TerrainEditorSettingsSO : ScriptableObject
    {
        [Header("Global References")]
        [SerializeField] private MaterialDatabase materialDatabase;
        
        [Header("Editor Colors")]
        [SerializeField] private Color dwarfSpawnerColor = Color.cyan;
        [SerializeField] private Color bedSpawnerColor = new Color(0.8f, 0.6f, 1f, 0.7f);
        
        [Header("Space Type Colors")]
        [SerializeField] private Color emptySpaceColor = Color.gray;
        [SerializeField] private Color treasureSpaceColor = Color.yellow;
        [SerializeField] private Color enemySpaceColor = Color.red;

        [Header("Editor Settings")]
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 3f;
        [SerializeField] private float panSpeed = 1f;

        // Getters
        public MaterialDatabase MaterialDatabase => materialDatabase;
        public Color DwarfSpawnerColor => dwarfSpawnerColor;
        public Color BedSpawnerColor => bedSpawnerColor;
        public float ZoomSpeed => zoomSpeed;
        public float MinZoom => minZoom;
        public float MaxZoom => maxZoom;
        public float PanSpeed => panSpeed;

        public Color GetSpaceTypeColor(SpaceType type)
        {
            switch (type)
            {
                case SpaceType.Empty: return emptySpaceColor;
                case SpaceType.Treasure: return treasureSpaceColor;
                case SpaceType.Enemy: return enemySpaceColor;
                default: return Color.clear;
            }
        }
    }
}

// ScriptRole: Stores global settings and references for the terrain editor
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: MaterialDatabase
// NeedsSetup: Create via Assets > Create > Spookie > Editor > Terrain Editor Settings