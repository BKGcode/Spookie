using UnityEngine;
using UnityEditor;

/// <summary>
/// Main editor window for viewing and manipulating terrain data via TerrainDataAssets.
/// </summary>
public class TerrainEditorWindow : EditorWindow
{
    private TerrainDataAsset currentAsset;
    private TerrainData currentTerrainData;
    
    private MaterialDatabase materialDatabase;
    private TerrainGridRenderer gridRenderer;
    private TerrainPaintTool paintTool;
    private TerrainStatistics statistics;

    // Generation settings
    private int generationSeed;
    
    private Vector2Int hoveredTileCoords = new Vector2Int(-1, -1);
    private bool isDirty = false; // Flag to track unsaved changes

    [MenuItem("Window/Spookie/Terrain Editor")]
    public static void ShowWindow()
    {
        GetWindow<TerrainEditorWindow>("Terrain Editor");
    }

    void OnEnable()
    {
        gridRenderer = new TerrainGridRenderer();
        paintTool = new TerrainPaintTool();
        statistics = new TerrainStatistics();
        titleContent = new GUIContent("Terrain Editor");
    }
    
    void OnGUI()
    {
        DrawToolbar();

        materialDatabase = (MaterialDatabase)EditorGUILayout.ObjectField("Material Database", materialDatabase, typeof(MaterialDatabase), false);

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
        GUI.enabled = currentTerrainData != null && materialDatabase != null;
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

        GUI.enabled = currentTerrainData != null;
        if (GUILayout.Button("Analyze", EditorStyles.toolbarButton))
        {
            statistics.Analyze(currentTerrainData);
        }
        GUI.enabled = true;

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
        paintTool.DrawUI(materialDatabase);
        EditorGUILayout.Space();
        DrawGenerationUI();
        EditorGUILayout.Space();
        statistics.DrawUI();
        
        GUILayout.EndVertical();
    }
    
    private void HandleMouseEvents(Rect gridRect)
    {
        Event current = Event.current;
        if (!gridRect.Contains(current.mousePosition)) return;

        hoveredTileCoords = gridRenderer.GetTileCoordinatesFromMouse(gridRect, current.mousePosition, currentTerrainData);

        if ((current.type == EventType.MouseDrag || current.type == EventType.MouseDown) && current.button == 0)
        {
            if (currentTerrainData.IsValidPosition(hoveredTileCoords.x, hoveredTileCoords.y))
            {
                paintTool.Paint(currentTerrainData, hoveredTileCoords, materialDatabase);
                isDirty = true; // Mark as having unsaved changes
                current.Use();
            }
        }
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

        // Create a temporary generator object
        GameObject generatorGO = new GameObject("TempGenerator");
        generatorGO.hideFlags = HideFlags.HideAndDontSave; // Keep it clean
        
        TerrainGenerator generator = generatorGO.AddComponent<TerrainGenerator>();
        generator.Initialize(materialDatabase, currentTerrainData.Width, currentTerrainData.Height);
        
        // Generate the new data
        TerrainData newTerrainData = generator.GenerateTerrain(generationSeed);
        
        // Clean up the temporary object immediately
        DestroyImmediate(generatorGO);

        if (newTerrainData != null)
        {
            currentTerrainData = newTerrainData;
            isDirty = true;
            statistics.Analyze(currentTerrainData);
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
            
            MaterialSO baseMaterial = materialDatabase != null ? materialDatabase.GetBaseMaterial() : null;

            for(int i = 0; i < width * height; i++)
            {
                newAsset.LevelData.AllTiles.Add(new SavedTileData{
                    MaterialGuid = baseMaterial?.Guid,
                    MiningState = MiningState.Intact,
                    SpecialProperty = SpaceType.None
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
        
        // Transfer data from our editor's TerrainData back to the asset's serializable format
        for (int y = 0; y < currentTerrainData.Height; y++)
        {
            for (int x = 0; x < currentTerrainData.Width; x++)
            {
                int index = y * currentTerrainData.Width + x;
                var tile = currentTerrainData.GetTile(x, y);
                currentAsset.LevelData.AllTiles[index] = new SavedTileData
                {
                    MaterialGuid = tile.GetMaterial()?.Guid,
                    MiningState = tile.GetMiningState(),
                    SpecialProperty = tile.GetSpecialProperty()
                };
            }
        }
        
        EditorUtility.SetDirty(currentAsset);
        AssetDatabase.SaveAssets();
        isDirty = false;
        Debug.Log($"Saved changes to {currentAsset.name}");
    }

    private void LoadTerrainDataFromAsset(TerrainDataAsset asset)
    {
        if (asset.LevelData == null || asset.LevelData.AllTiles == null)
        {
            EditorUtility.DisplayDialog("Load Error", "The asset's level data is corrupted or empty.", "OK");
            return;
        }

        currentAsset = asset;
        generationSeed = asset.LevelData.MapSeed; // Load seed from asset
        currentTerrainData = new TerrainData(asset.LevelData.Width, asset.LevelData.Height, asset.LevelData.MapSeed);
        
        for (int i = 0; i < asset.LevelData.AllTiles.Count; i++)
        {
            var savedTile = asset.LevelData.AllTiles[i];
            int x = i % asset.LevelData.Width;
            int y = i / asset.LevelData.Width;

            MaterialSO material = materialDatabase.GetMaterialByGuid(savedTile.MaterialGuid);
            if (material == null && !string.IsNullOrEmpty(savedTile.MaterialGuid))
            {
                Debug.LogWarning($"Could not find material with GUID '{savedTile.MaterialGuid}'.");
            }

            TerrainTile newTile = new TerrainTile(material);
            newTile.SetMiningState(savedTile.MiningState);
            newTile.SetSpecialProperty(savedTile.SpecialProperty);
            
            currentTerrainData.SetTile(x, y, newTile);
        }

        statistics.Analyze(currentTerrainData);
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