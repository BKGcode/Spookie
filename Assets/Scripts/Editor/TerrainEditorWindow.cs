using UnityEngine;
using UnityEditor;

/// <summary>
/// Main editor window for viewing and manipulating terrain data.
/// </summary>
public class TerrainEditorWindow : EditorWindow
{
    private TerrainData currentTerrainData;
    private TerrainGridRenderer gridRenderer;
    private TerrainPaintTool paintTool;
    private TerrainStatistics statistics;
    private Vector2Int hoveredTileCoords = new Vector2Int(-1, -1);
    
    [MenuItem("Window/Terrain Generator/Editor")]
    public static void ShowWindow()
    {
        GetWindow<TerrainEditorWindow>("Terrain Editor");
    }

    void OnEnable()
    {
        Debug.Log("TerrainEditorWindow: Enabled.");
        gridRenderer = new TerrainGridRenderer();
        paintTool = new TerrainPaintTool();
        statistics = new TerrainStatistics();
        
        if (Application.isPlaying && TerrainManager.Instance != null)
        {
            currentTerrainData = TerrainManager.Instance.GetTerrainData();
        }
    }

    void OnGUI()
    {
        DrawToolbar();
        
        if (currentTerrainData == null)
        {
            EditorGUILayout.HelpBox("No terrain data loaded. Run the game with a TerrainManager or load data manually.", MessageType.Info);
            return;
        }

        GUILayout.BeginHorizontal();
        
        // --- Grid Area (Left) ---
        Rect gridRect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        gridRenderer.DrawGrid(gridRect, currentTerrainData);
        
        // --- Info Panel (Right) ---
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(250), GUILayout.ExpandHeight(true));
        DrawInfoPanel();
        GUILayout.EndVertical();
        
        GUILayout.EndHorizontal();
        
        HandleMouseEvents(gridRect);
        Repaint(); // Force repaint for smooth interaction
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("Load From Scene", EditorStyles.toolbarButton))
        {
            LoadDataFromScene();
        }
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Analyze Terrain", EditorStyles.toolbarButton))
        {
            statistics.Analyze(currentTerrainData);
        }

        GUILayout.EndHorizontal();
    }

    private void DrawInfoPanel()
    {
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
        
        // Paint Tool
        paintTool.DrawUI();

        EditorGUILayout.Space();

        // Statistics
        statistics.DrawUI();
    }

    private void HandleMouseEvents(Rect gridRect)
    {
        Event current = Event.current;

        if (gridRect.Contains(current.mousePosition))
        {
            // Hover
            if (current.type == EventType.MouseMove || current.type == EventType.MouseDrag)
            {
                hoveredTileCoords = gridRenderer.GetTileCoordinatesFromMouse(gridRect, current.mousePosition, currentTerrainData);
            }
        
            // Paint
            if ((current.type == EventType.MouseDrag || current.type == EventType.MouseDown) && current.button == 0)
            {
                Vector2Int coords = gridRenderer.GetTileCoordinatesFromMouse(gridRect, current.mousePosition, currentTerrainData);
                if (currentTerrainData.IsValidPosition(coords.x, coords.y))
                {
                    paintTool.Paint(currentTerrainData, coords);
                    current.Use(); // Consume the event
                }
            }
        }
    }

    private void LoadDataFromScene()
    {
        if (Application.isPlaying && TerrainManager.Instance != null)
        {
            currentTerrainData = TerrainManager.Instance.GetTerrainData();
            if (currentTerrainData != null)
            {
                Debug.Log("TerrainEditorWindow: Successfully loaded terrain data from scene.");
                statistics.Analyze(currentTerrainData); // Analyze on load
            }
            else
            {
                Debug.LogWarning("TerrainEditorWindow: TerrainManager is running, but terrain data is null.");
            }
        }
        else
        {
            Debug.LogError("TerrainEditorWindow: Must be in Play Mode with an active TerrainManager to load data from scene.");
        }
        Repaint();
    }
}

// ScriptRole: Provides a custom editor window for viewing and manipulating terrain.
// Dependencies: TerrainData, other editor tool classes (to be created)
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: Open via Window/Terrain Generator/Editor menu. 