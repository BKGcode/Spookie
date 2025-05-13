using UnityEngine;
using UnityEngine.UI; // Para Button, si usamos UI real más adelante
using System.Collections.Generic;
// Asumiendo que TMPro será usado para cualquier texto en los botones
// using TMPro;

public class CropSelectionToolbarUI : MonoBehaviour
{
    [Header("Configuración UI")]
    [Tooltip("Prefab del botón para seleccionar un cultivo. Debe tener un script (ej. CropButtonUI) para manejar datos y clics.")]
    [SerializeField] private GameObject cropButtonPrefab; 
    [Tooltip("Contenedor donde se instanciarán los botones de los cultivos.")]
    [SerializeField] private Transform buttonsContainer;

    private FarmingManager farmingManager;
    private List<CropSO> currentlyDisplayedCrops = new List<CropSO>();
    // Podríamos tener una lista de GameObjects de botones para gestionarlos (ej. actualizar selección)
    private Dictionary<CropSO, Button> cropButtons = new Dictionary<CropSO, Button>(); // O un script custom en el botón

    void Awake()
    {
        Debug.Log("CropSelectionToolbarUI: Awake_Start");
        farmingManager = FindObjectOfType<FarmingManager>();
        if (farmingManager == null)
        {
            Debug.LogError("CropSelectionToolbarUI: FarmingManager no encontrado. La toolbar no funcionará.");
            enabled = false;
            return;
        }

        if (cropButtonPrefab == null)
        {
            Debug.LogWarning("CropSelectionToolbarUI: Prefab de botón de cultivo no asignado.");
            // Podríamos optar por no deshabilitar, pero no se mostrará nada.
        }
        if (buttonsContainer == null)
        {
            Debug.LogError("CropSelectionToolbarUI: Contenedor de botones no asignado. La toolbar no funcionará.");
            enabled = false;
            return;
        }
        Debug.Log("CropSelectionToolbarUI: Awake_End");
    }

    void OnEnable()
    {
        Debug.Log("CropSelectionToolbarUI: OnEnable_Start");
        if (farmingManager != null)
        {
            farmingManager.OnCropUnlocked += HandleCropUnlocked;
            farmingManager.OnCropSelectedForPlanting += HandleCropSelectedEvent;
            farmingManager.OnPlantingModeExited += HandlePlantingModeExitedEvent;
            RefreshToolbar();
            Debug.Log("CropSelectionToolbarUI: Suscrito a eventos y toolbar refrescada.");
        }
        Debug.Log("CropSelectionToolbarUI: OnEnable_End");
    }

    void OnDisable()
    {
        Debug.Log("CropSelectionToolbarUI: OnDisable_Start");
        if (farmingManager != null)
        {
            farmingManager.OnCropUnlocked -= HandleCropUnlocked;
            farmingManager.OnCropSelectedForPlanting -= HandleCropSelectedEvent;
            farmingManager.OnPlantingModeExited -= HandlePlantingModeExitedEvent;
            Debug.Log("CropSelectionToolbarUI: Desuscrito de eventos.");
        }
        Debug.Log("CropSelectionToolbarUI: OnDisable_End");
    }

    private void HandleCropUnlocked(CropSO newlyUnlockedCrop)
    {
        Debug.Log($"CropSelectionToolbarUI: Evento OnCropUnlocked recibido para {newlyUnlockedCrop.name}. Refrescando toolbar.");
        RefreshToolbar();
    }
    
    private void HandleCropSelectedEvent(CropSO selectedCrop)
    {
        Debug.Log($"CropSelectionToolbarUI: Evento OnCropSelectedForPlanting recibido para {selectedCrop?.name ?? "ninguno"}. Actualizando visual de botones.");
        UpdateButtonsVisualSelection(selectedCrop);
    }

    private void HandlePlantingModeExitedEvent()
    {
        Debug.Log("CropSelectionToolbarUI: Evento OnPlantingModeExited recibido. Limpiando selección visual de botones.");
        UpdateButtonsVisualSelection(null);
    }

    public void RefreshToolbar()
    {
        Debug.Log("CropSelectionToolbarUI: RefreshToolbar_Start");
        if (farmingManager == null || buttonsContainer == null || cropButtonPrefab == null)
        {
            Debug.LogWarning("CropSelectionToolbarUI: No se puede refrescar, FarmingManager, container o prefab no disponibles.");
            return;
        }

        // Limpiar botones existentes (simple, podría optimizarse)
        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }
        cropButtons.Clear();
        currentlyDisplayedCrops.Clear();

        List<CropSO> unlockedCrops = farmingManager.GetUnlockedCrops();
        Debug.Log($"CropSelectionToolbarUI: Obtenidos {unlockedCrops.Count} cultivos desbloqueados del FarmingManager.");

        foreach (CropSO crop in unlockedCrops)
        {
            GameObject buttonGO = Instantiate(cropButtonPrefab, buttonsContainer);
            // Asumimos que el prefab tiene un script CropButtonUI o similar para configurar el icono y texto
            // y un componente Button de Unity UI.
            // Ejemplo de configuración (si el prefab es simple y no tiene script propio):
            // Image icon = buttonGO.transform.Find("Icon").GetComponent<Image>(); // Suponiendo estructura del prefab
            // TextMeshProUGUI nameText = buttonGO.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            // if(icon != null) icon.sprite = crop.Icon;
            // if(nameText != null) nameText.text = crop.name; // O crop.DisplayNameSO si lo tuvieras

            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => SelectCrop(crop));
                cropButtons.Add(crop, buttonComponent);
                Debug.Log($"CropSelectionToolbarUI: Creado botón para {crop.name}.");
            }
            else
            {
                Debug.LogWarning($"CropSelectionToolbarUI: Prefab de botón para {crop.name} no tiene componente Button.");
            }
            currentlyDisplayedCrops.Add(crop);
        }
        UpdateButtonsVisualSelection(farmingManager.GetSelectedCropForPlanting());
        Debug.Log("CropSelectionToolbarUI: RefreshToolbar_End");
    }

    private void SelectCrop(CropSO crop)
    {
        if (farmingManager == null) return;

        CropSO currentlySelected = farmingManager.GetSelectedCropForPlanting();
        if (currentlySelected == crop) // Si se clickea el mismo cultivo ya seleccionado
        {
            farmingManager.ExitPlantingMode(); // Deseleccionar
            Debug.Log($"CropSelectionToolbarUI: Botón {crop.name} clickeado (ya estaba seleccionado). Deseleccionando.");
        }
        else
        {
            farmingManager.SelectCropForPlanting(crop);
            Debug.Log($"CropSelectionToolbarUI: Botón {crop.name} clickeado. Seleccionando en FarmingManager.");
        }
        // La actualización visual se manejará por los eventos OnCropSelectedForPlanting / OnPlantingModeExited
    }

    private void UpdateButtonsVisualSelection(CropSO selectedCrop)
    {
        // Aquí iría la lógica para cambiar el aspecto de los botones
        // (ej. color, tamaño, un borde) para indicar cuál está seleccionado.
        foreach (var entry in cropButtons)
        { 
            // Ejemplo: entry.Value.GetComponent<Image>().color = (entry.Key == selectedCrop) ? Color.green : Color.white;
            if(entry.Key == selectedCrop)
            {
                Debug.Log($"CropSelectionToolbarUI: Botón para {entry.Key.name} marcado como SELECCIONADO visualmente.");
                // TODO: Aplicar estilo visual de seleccionado al entry.Value (botón)
            }
            else
            {
                // TODO: Aplicar estilo visual de NO seleccionado al entry.Value (botón)
            }
        }
    }
}

// ScriptRole: Manages the UI toolbar for selecting available crops to plant.
// Dependencies: FarmingManager, UI Button Prefab (with a Button component, ideally also TextMeshPro and Image for icon)
// HandlesEvents: FarmingManager.OnCropUnlocked, FarmingManager.OnCropSelectedForPlanting, FarmingManager.OnPlantingModeExited
// TriggersEvents: None (Calls methods on FarmingManager)
// UsesSO: CropSO (to display and select)
// NeedsSetup: cropButtonPrefab, buttonsContainer. Relies on FarmingManager in the scene. 