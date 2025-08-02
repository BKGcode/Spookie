using UnityEngine;
using UnityEditor;
using Terrain;
using Terrain.Data;
using SO;

namespace EditorScripts
{
    /// <summary>
    /// Main editor window for viewing and manipulating terrain data via TerrainDataAssets.
    /// </summary>
    public class TerrainEditorWindow : EditorWindow
    {
        private TerrainDataAsset currentAsset;
        private Terrain.TerrainData currentTerrainData;
        
        private TerrainEditorSettingsSO editorSettings;
        private TerrainGridRenderer gridRenderer;
        private TerrainPaintTool paintTool;

        // Generation settings
        private int generationSeed;
        
        private Vector2Int hoveredTileCoords = new Vector2Int(-1, -1);
        private bool isDirty = false; // Flag to track unsaved changes
        private bool isDraggingMap = false;

        [MenuItem("Window/Spookie/Terrain Editor")]
        public static void ShowWindow()
        {
            GetWindow<TerrainEditorWindow>("Terrain Editor");
        }

        void OnEnable()
        {
            // Load editor settings
            string[] guids = AssetDatabase.FindAssets("t:TerrainEditorSettingsSO");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                editorSettings = AssetDatabase.LoadAssetAtPath<TerrainEditorSettingsSO>(path);
            }
            
            if (editorSettings == null)
            {
                Debug.LogError("TerrainEditorWindow: No TerrainEditorSettingsSO found in the project. Please create one via Assets > Create > Spookie > Editor > Terrain Editor Settings");
                return;
            }

            gridRenderer = new TerrainGridRenderer(editorSettings);
            paintTool = new TerrainPaintTool();
            titleContent = new GUIContent("Terrain Editor");
        }
        
        void OnGUI()
        {
            if (editorSettings == null)
            {
                EditorGUILayout.HelpBox("TerrainEditorSettingsSO not found. Please create one via Assets > Create > Spookie > Editor > Terrain Editor Settings", MessageType.Error);
                if (GUILayout.Button("Create Settings Asset"))
                {
                    CreateSettingsAsset();
                }
                return;
            }

            DrawToolbar();

            if (currentTerrainData == null)
            {
                EditorGUILayout.HelpBox("Create a new terrain or load a Terrain Level Asset to begin editing.", MessageType.Info);
                return;
            }

            // The rest of the UI
            GUILayout.BeginHorizontal();
            DrawGridArea();
            DrawInfoPanel();
            GUILayout.EndHorizontal();

            HandleEditorInput();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New", EditorStyles.toolbarButton))
            {
                CreateNewTerrainAsset();
            }

            if (GUILayout.Button("Load", EditorStyles.toolbarButton))
            {
                LoadExistingTerrainAsset();
            }
            
            // Add a visual indicator for unsaved changes
            string saveButtonText = isDirty ? "Save*" : "Save";
            GUI.enabled = currentAsset != null;
            if (GUILayout.Button(saveButtonText, EditorStyles.toolbarButton))
            {
                SaveCurrentTerrainAsset();
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            // --- Generation Button ---
            GUI.enabled = currentTerrainData != null && editorSettings?.MaterialDatabase != null;
            if (GUILayout.Button("Generate New", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Confirm Generation", 
                    "This will overwrite the current terrain data. Are you sure you want to proceed?", 
                    "Generate", "Cancel"))
                {
                    GenerateNewProceduralTerrain();
                }
            }
            GUI.enabled = true;
            // -------------------------

            GUILayout.EndHorizontal();
        }

        private void DrawGridArea()
        {
            Rect gridRect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            gridRenderer.DrawGrid(gridRect, currentTerrainData);
            HandleMouseEvents(gridRect);
            Repaint();
        }

        private void DrawInfoPanel()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(250), GUILayout.ExpandHeight(true));
            
            // Hover Info
            GUILayout.Label("Tile Info", EditorStyles.boldLabel);
            if (currentTerrainData.IsValidPosition(hoveredTileCoords.x, hoveredTileCoords.y))
            {
                TerrainTile tile = currentTerrainData.GetTile(hoveredTileCoords.x, hoveredTileCoords.y);
                MaterialSO material = tile.GetMaterial();

                EditorGUILayout.LabelField("Coordinates:", $"{hoveredTileCoords.x}, {hoveredTileCoords.y}");
                EditorGUILayout.LabelField("Material:", material != null ? material.MaterialName : "None");
                EditorGUILayout.LabelField("Mining State:", tile.GetMiningState().ToString());
                EditorGUILayout.LabelField("Special Property:", tile.GetSpecialProperty().ToString());
            }
            else
            {
                EditorGUILayout.LabelField("Hover over a tile to see its info.");
            }

            EditorGUILayout.Space();
            paintTool.DrawUI(editorSettings.MaterialDatabase);
            EditorGUILayout.Space();
            DrawGenerationUI();
            
            GUILayout.EndVertical();
        }
        
        private void HandleMouseEvents(Rect gridRect)
        {
            Event current = Event.current;
            if (!gridRect.Contains(current.mousePosition)) return;

            hoveredTileCoords = gridRenderer.GetTileCoordinatesFromMouse(gridRect, current.mousePosition, currentTerrainData);

            // Left click for painting
            if ((current.type == EventType.MouseDrag || current.type == EventType.MouseDown) && current.button == 0)
            {
                if (currentTerrainData.IsValidPosition(hoveredTileCoords.x, hoveredTileCoords.y))
                {
                    paintTool.Paint(currentTerrainData, hoveredTileCoords, editorSettings.MaterialDatabase);
                    isDirty = true; // Mark as having unsaved changes
                    current.Use();
                }
            }
        }

        private void HandleEditorInput()
        {
            Event current = Event.current;

            // Middle mouse button for panning
            if (current.button == 2)
            {
                if (current.type == EventType.MouseDown)
                {
                    isDraggingMap = true;
                    current.Use();
                }
                else if (current.type == EventType.MouseDrag && isDraggingMap)
                {
                    gridRenderer.HandlePan(current.delta);
                    current.Use();
                }
                else if (current.type == EventType.MouseUp)
                {
                    isDraggingMap = false;
                    current.Use();
                }
            }

            // Mouse wheel for zooming
            if (current.type == EventType.ScrollWheel)
            {
                gridRenderer.HandleZoom(-current.delta.y);
                current.Use();
            }
        }

        private void CreateSettingsAsset()
        {
            string path = "Assets/Scripts/SO/TerrainEditorSettings.asset";
            
            // Ensure the directory exists
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            TerrainEditorSettingsSO asset = CreateInstance<TerrainEditorSettingsSO>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            editorSettings = asset;
            Debug.Log("Created TerrainEditorSettings asset at " + path);
        }

        private void DrawGenerationUI()
        {
            GUILayout.Label("Procedural Generation", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            generationSeed = EditorGUILayout.IntField("Seed", generationSeed);
            if (GUILayout.Button("Random", GUILayout.Width(70)))
            {
                generationSeed = System.DateTime.Now.GetHashCode();
            }
            EditorGUILayout.EndHorizontal();
        }

        // --- Asset Management Logic ---

        private void GenerateNewProceduralTerrain()
        {
            Debug.Log($"Attempting to generate new terrain with seed: {generationSeed}");

            // Instantiate the generator directly, no more temporary GameObjects.
            var generator = new TerrainGenerator(editorSettings.MaterialDatabase, currentTerrainData.Width, currentTerrainData.Height);
            
            // Generate the new data
            Terrain.TerrainData newTerrainData = generator.GenerateTerrain(generationSeed);
            
            if (newTerrainData != null)
            {
                currentTerrainData = newTerrainData;
                isDirty = true;
                Repaint();
                Debug.Log("Procedural terrain generated successfully in editor.");
            }
            else
            {
                Debug.LogError("Failed to generate new terrain data in editor.");
            }
        }

        private void CreateNewTerrainAsset()
        {
            if (PromptToSaveIfDirty())
            {
                // For simplicity, new terrains are always 100x100. This could be a dialog.
                int width = 100;
                int height = 100;
                
                string path = EditorUtility.SaveFilePanelInProject("Create New Terrain Level", "NewTerrainLevel", "asset", "Please enter a file name to save the new terrain level to.");
                if (string.IsNullOrEmpty(path)) return;

                TerrainDataAsset newAsset = CreateInstance<TerrainDataAsset>();
                newAsset.LevelData = new TerrainSaveData(System.DateTime.Now.GetHashCode(), width, height, "Editor");
                
                MaterialSO baseMaterial = editorSettings?.MaterialDatabase != null ? editorSettings.MaterialDatabase.GetBaseMaterial() : null;

                for(int i = 0; i < width * height; i++)
                {
                    newAsset.LevelData.AllTiles.Add(new SavedTileData{
                        MaterialGuid = baseMaterial?.Guid,
                        MiningState = MiningState.Intact,
                        SpecialProperty = SpaceType.Empty // Cambiado a Empty para que sea walkable cuando se mine
                    });
                }

                AssetDatabase.CreateAsset(newAsset, path);
                AssetDatabase.SaveAssets();

                LoadTerrainDataFromAsset(newAsset);
            }
        }
        
        private void LoadExistingTerrainAsset()
        {
            if (PromptToSaveIfDirty())
            {
                string path = EditorUtility.OpenFilePanel("Load Terrain Level Asset", "Assets/", "asset");
                if (string.IsNullOrEmpty(path)) return;
                
                // Need to convert absolute path to a relative project path
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }

                TerrainDataAsset asset = AssetDatabase.LoadAssetAtPath<TerrainDataAsset>(path);
                if (asset != null)
                {
                    LoadTerrainDataFromAsset(asset);
                }
                else
                {
                    EditorUtility.DisplayDialog("Load Error", "The selected file is not a valid TerrainDataAsset.", "OK");
                }
            }
        }

        private void SaveCurrentTerrainAsset()
        {
            if (currentAsset == null) return;
            
            // Ensure the asset's lists are ready for the new data
            currentAsset.LevelData.AllTiles.Clear();
            currentAsset.LevelData.DwarfSpawnPoints.Clear();
            currentAsset.LevelData.BedSpawnPoints.Clear();
            currentAsset.LevelData.DwarfSpawnPoints.AddRange(currentTerrainData.DwarfSpawnPoints);
            currentAsset.LevelData.BedSpawnPoints.AddRange(currentTerrainData.BedSpawnPoints);
            
            // Transfer data from our editor's TerrainData back to the asset's serializable format
            for (int y = 0; y < currentTerrainData.Height; y++)
            {
                for (int x = 0; x < currentTerrainData.Width; x++)
                {
                    var tile = currentTerrainData.GetTile(x, y);
                    // Instead of trying to access by index, we add to the cleared list.
                    currentAsset.LevelData.AllTiles.Add(new SavedTileData
                    {
                        MaterialGuid = tile.GetMaterial()?.Guid,
                        MiningState = tile.GetMiningState(),
                        SpecialProperty = tile.GetSpecialProperty()
                    });
                }
            }
            
            EditorUtility.SetDirty(currentAsset);
            AssetDatabase.SaveAssets();
            isDirty = false;
            Debug.Log($"Saved changes to {currentAsset.name}");
        }

        private void LoadTerrainDataFromAsset(TerrainDataAsset asset)
        {
            if (asset.LevelData == null)
            {
                EditorUtility.DisplayDialog("Load Error", "The asset's level data is null. Cannot load.", "OK");
                return;
            }

            currentAsset = asset;
            generationSeed = asset.LevelData.MapSeed; // Load seed from asset
            
            // Use the centralized factory to create the terrain data
            currentTerrainData = TerrainDataFactory.CreateFromSaveData(asset.LevelData, editorSettings.MaterialDatabase);

            if (currentTerrainData == null)
            {
                EditorUtility.DisplayDialog("Load Error", "Failed to create terrain data. Check the console for errors.", "OK");
                return;
            }
            isDirty = false;
            Debug.Log($"Loaded terrain level: {asset.name}");
        }

        private bool PromptToSaveIfDirty()
        {
            if (!isDirty) return true;

            int choice = EditorUtility.DisplayDialogComplex(
                "Unsaved Changes",
                "You have unsaved changes. Would you like to save them before continuing?",
                "Save",
                "Don't Save",
                "Cancel"
            );

            if (choice == 0) // Save
            {
                SaveCurrentTerrainAsset();
                return true;
            }
            if (choice == 1) // Don't Save
            {
                return true;
            }
            
            return false; // Cancel
        }
    }
}