using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CropToolbarUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Objeto padre donde se instanciarán los botones de cultivo.")]
    [SerializeField] private Transform buttonContainer;
    [Tooltip("Prefab del botón que representa un cultivo seleccionable.")]
    [SerializeField] private GameObject cropButtonPrefab;

    // Referencias internas
    private FarmingManager farmingManager;
    private List<CropButtonUI> cropButtons = new List<CropButtonUI>(); // Para gestionar botones instanciados
    private CropButtonUI selectedButton = null;

    void Start()
    {
        farmingManager = FindObjectOfType<FarmingManager>();
        if (farmingManager == null)
        {
            Debug.LogError("CropToolbarUI: No se encontró FarmingManager en la escena.");
            enabled = false;
            return;
        }

        if (buttonContainer == null)
        {
             Debug.LogError("CropToolbarUI: Falta referencia a Button Container.");
             enabled = false;
             return;
        }
         if (cropButtonPrefab == null)
        {
             Debug.LogError("CropToolbarUI: Falta referencia a Crop Button Prefab.");
             enabled = false;
             return;
        }
         if (cropButtonPrefab.GetComponent<CropButtonUI>() == null)
         {
             Debug.LogError("CropToolbarUI: El Crop Button Prefab debe tener un script CropButtonUI.");
             enabled = false;
             return;
         }


        SubscribeToEvents();
        PopulateInitialToolbar();
         Debug.Log("CropToolbarUI: Inicializado y suscrito a eventos.");
    }

     void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

     private void SubscribeToEvents()
    {
         if (farmingManager == null) return;
        farmingManager.OnCropUnlocked += HandleCropUnlocked;
        farmingManager.OnCropSelectedForPlanting += HandleCropSelected;
        farmingManager.OnPlantingModeExited += HandlePlantingModeExited;
    }

    private void UnsubscribeFromEvents()
    {
        if (farmingManager == null) return;
        farmingManager.OnCropUnlocked -= HandleCropUnlocked;
        farmingManager.OnCropSelectedForPlanting -= HandleCropSelected;
        farmingManager.OnPlantingModeExited -= HandlePlantingModeExited;
    }

    private void PopulateInitialToolbar()
    {
        // Limpiar botones existentes (por si acaso)
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        cropButtons.Clear();
        selectedButton = null;

        List<CropSO> unlockedCrops = farmingManager.GetUnlockedCrops();
        Debug.Log($"CropToolbarUI: Poblando toolbar inicial con {unlockedCrops.Count} cultivos desbloqueados.");
        foreach (CropSO crop in unlockedCrops)
        {
            InstantiateCropButton(crop);
        }
         // Seleccionar visualmente el que esté seleccionado en el manager (si lo hubiera al inicio)
         HandleCropSelected(farmingManager.GetSelectedCropForPlanting());
    }

    private void InstantiateCropButton(CropSO crop)
    {
         if (crop == null) return;

         // Evitar duplicados si ya existe por alguna razón
         if (cropButtons.Any(b => b.RepresentedCrop == crop))
         {
             Debug.LogWarning($"CropToolbarUI: Intento de instanciar botón duplicado para {crop.name}");
             return;
         }

        GameObject buttonGO = Instantiate(cropButtonPrefab, buttonContainer);
        CropButtonUI buttonUI = buttonGO.GetComponent<CropButtonUI>();

        if (buttonUI != null)
        {
            buttonUI.Initialize(crop, farmingManager); // Pasamos el crop y el manager
            cropButtons.Add(buttonUI);
             Debug.Log($"CropToolbarUI: Botón instanciado para {crop.name}.");
        }
        else
        {
            Debug.LogError($"CropToolbarUI: El prefab {cropButtonPrefab.name} no tiene el script CropButtonUI.");
            Destroy(buttonGO); // Limpiar objeto inútil
        }
    }

    // --- Manejadores de Eventos ---

    private void HandleCropUnlocked(CropSO crop)
    {
         Debug.Log($"CropToolbarUI: Recibido evento OnCropUnlocked para {crop.name}.");
        InstantiateCropButton(crop); // Añadir nuevo botón a la barra
    }

     private void HandleCropSelected(CropSO crop)
    {
        selectedButton = null; // Resetear selección
        foreach (var button in cropButtons)
        {
            bool isSelected = (button.RepresentedCrop == crop);
            button.SetSelected(isSelected); // Actualizar estado visual del botón
            if (isSelected)
            {   
                selectedButton = button; // Guardar referencia al botón seleccionado
                 Debug.Log($"CropToolbarUI: Botón para {crop?.name ?? "null"} marcado como seleccionado.");
            }
        }
         if (crop == null && selectedButton != null) // Asegurarse que no queda nada seleccionado si crop es null
         {
            selectedButton.SetSelected(false);
            selectedButton = null;
         } else if (crop != null && selectedButton == null && cropButtons.Count > 0) { // Solo advertir si hay botones 
              Debug.LogWarning($"CropToolbarUI: Se seleccionó el cultivo {crop.name} pero no se encontró su botón UI correspondiente.");
         }
    }

    private void HandlePlantingModeExited()
    {
         Debug.Log("CropToolbarUI: Recibido evento OnPlantingModeExited.");
         if (selectedButton != null)
         {
             selectedButton.SetSelected(false); // Desmarcar visualmente
             selectedButton = null;
         }
    }
}

// ScriptRole: Gestiona la barra de herramientas que muestra los cultivos desbloqueados y permite seleccionar uno para plantar.
// RelatedScripts: FarmingManager, CropButtonUI, CropSO
// UsesSO: CropSO (indirectamente, a través de FarmingManager)
// ReceivesFrom: FarmingManager (eventos)
// SendsTo: CropButtonUI (inicialización, estado seleccionado) 