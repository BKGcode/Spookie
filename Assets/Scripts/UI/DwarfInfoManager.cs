using UnityEngine;
using System.Collections.Generic;
using Dwarfs;

namespace UI
{
    public class DwarfInfoManager : MonoBehaviour
    {
        // Singleton pattern to prevent multiple instances
        public static DwarfInfoManager Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private GameObject dwarfInfoPanelPrefab;
        public GameObject DwarfInfoPanelPrefab 
        { 
            get => dwarfInfoPanelPrefab;
            set => dwarfInfoPanelPrefab = value;
        }

        [SerializeField] private Transform panelsContainer;
        public Transform PanelsContainer
        {
            get => panelsContainer;
            set => panelsContainer = value;
        }

        [SerializeField] private int maxVisiblePanels = 5;

        private readonly Dictionary<int, DwarfInfoPanel> _activePanels = new();
        private readonly Queue<DwarfInfoPanel> _panelsQueue = new();

        private void Awake()
        {
            // Singleton pattern implementation
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("DwarfInfoManager: Singleton instance created");
            }
            else if (Instance != this)
            {
                Debug.LogWarning("DwarfInfoManager: Duplicate instance detected, destroying this one");
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            DwarfSpawner.OnDwarfSpawned += HandleDwarfSpawned;
        }

        private void OnDisable()
        {
            DwarfSpawner.OnDwarfSpawned -= HandleDwarfSpawned;
        }

        private void HandleDwarfSpawned(DwarfData dwarfData, DwarfBrain dwarfBrain)
        {
            CreateInfoPanel(dwarfData, dwarfBrain);
        }

        private void CreateInfoPanel(DwarfData dwarfData, DwarfBrain dwarfBrain)
        {
            // Validar configuración
            if (dwarfInfoPanelPrefab == null)
            {
                Debug.LogError("DwarfInfoManager: dwarfInfoPanelPrefab is not assigned!");
                return;
            }

            if (panelsContainer == null)
            {
                Debug.LogError("DwarfInfoManager: panelsContainer is not assigned!");
                return;
            }

            // Si ya existe un panel para este enano, no crear otro
            if (_activePanels.ContainsKey(dwarfData.DwarfId))
            {
                Debug.LogWarning($"Panel already exists for Dwarf {dwarfData.DwarfId}");
                return;
            }

            Debug.Log($"DwarfInfoManager: Creating panel for {dwarfData.name} (ID: {dwarfData.DwarfId})");
            Debug.Log($"DwarfInfoManager: Using container: {panelsContainer.name}");

            // Instanciar el nuevo panel como hijo del container
            GameObject panelGO = Instantiate(dwarfInfoPanelPrefab, panelsContainer);
            DwarfInfoPanel panel = panelGO.GetComponent<DwarfInfoPanel>();
            
            if (panel == null)
            {
                Debug.LogError("DwarfInfoPanel component not found on prefab!");
                Destroy(panelGO);
                return;
            }

            Debug.Log($"DwarfInfoManager: Panel created successfully. Parent: {panelGO.transform.parent?.name ?? "NO PARENT"}");

            // Inicializar el panel
            panel.Initialize(dwarfData, dwarfBrain);
            
            // Añadir a las colecciones
            _activePanels[dwarfData.DwarfId] = panel;
            _panelsQueue.Enqueue(panel);

            // Gestionar el límite de paneles visibles
            if (_panelsQueue.Count > maxVisiblePanels)
            {
                DwarfInfoPanel oldestPanel = _panelsQueue.Dequeue();
                int dwarfId = GetDwarfIdForPanel(oldestPanel);
                if (dwarfId != -1)
                {
                    _activePanels.Remove(dwarfId);
                }
                Destroy(oldestPanel.gameObject);
            }
        }

        private int GetDwarfIdForPanel(DwarfInfoPanel panel)
        {
            foreach (var kvp in _activePanels)
            {
                if (kvp.Value == panel)
                {
                    return kvp.Key;
                }
            }
            return -1;
        }
    }
}

// ScriptRole: Manages the creation, positioning and lifecycle of dwarf info UI panels
// Dependencies: DwarfInfoPanel, DwarfSpawner
// NeedsSetup: Create empty GameObject in canvas, attach this script, assign panel prefab and container