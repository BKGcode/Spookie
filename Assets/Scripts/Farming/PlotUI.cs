using UnityEngine;
using UnityEngine.UI; // Necesario para Image y Button
using TMPro; // Necesario para TextMeshProUGUI
using UnityEngine.EventSystems; // Necesario para IPointerClickHandler

[RequireComponent(typeof(Image))] // Asegura que haya un componente Image
public class PlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración de Parcela")]
    [Tooltip("ID único de esta parcela. Debe coincidir con uno de los IDs en FarmingManager -> InitialPlots.")]
    [SerializeField] private int _plotId; // Renombrado para usar backing field
    public int PlotId => _plotId; // Propiedad pública de solo lectura

    [Header("Referencias UI")]
    [Tooltip("Imagen principal que muestra el estado (bloqueado, vacío, icono cultivo).")]
    [SerializeField] private Image plotImage;
    [Tooltip("Imagen secundaria para mostrar el icono del cultivo cuando está plantado.")]
    [SerializeField] private Image cropIconImage;
    [Tooltip("Objeto que contiene la info de 'bloqueado' (icono candado, texto coste).")]
    [SerializeField] private GameObject lockedOverlay;
    [Tooltip("Texto para mostrar el coste de desbloqueo (opcional).")]
    [SerializeField] private TextMeshProUGUI costText;
    [Tooltip("Slider o Image (fill) para mostrar el progreso del crecimiento (opcional).")]
    [SerializeField] private Image progressBar;

    [Header("Sprites de Estado")]
    [Tooltip("Sprite para mostrar cuando la parcela está vacía y desbloqueada.")]
    [SerializeField] private Sprite emptyPlotSprite;
    [Tooltip("Sprite para mostrar cuando la parcela está bloqueada.")]
    [SerializeField] private Sprite lockedPlotSprite;

    // Referencias internas
    private FarmingManager farmingManager;
    private PlotData currentPlotData;

    void Awake()
    {
        Debug.Log($"PlotUI ({PlotId}): Awake_Start");
        if (plotImage == null) plotImage = GetComponent<Image>(); // Asegurar que plotImage esté asignada
        // Es mejor buscar FarmingManager en Awake para que esté disponible para Start de otros scripts si es necesario.
        farmingManager = FindObjectOfType<FarmingManager>();
        if (farmingManager == null)
        {
            Debug.LogError($"PlotUI ({PlotId}): No se encontró FarmingManager en la escena. El script se deshabilitará.");
            enabled = false; 
        }
        Debug.Log($"PlotUI ({PlotId}): Awake_End");
    }

    void Start()
    {
        Debug.Log($"PlotUI ({PlotId}): Start_Start");
        if (!enabled) return; // Si se deshabilitó en Awake

        ValidateReferences();
        
        currentPlotData = farmingManager.GetPlotData(PlotId);
        if (currentPlotData == null)
        {
             Debug.LogError($"PlotUI ({PlotId}): No se encontró PlotData para este ID ({PlotId}) en FarmingManager. Asegúrate que el ID existe en InitialPlots y es único. El script se deshabilitará.");
             enabled = false;
             return;
        }

        SubscribeToEvents();
        UpdateVisuals();
        Debug.Log($"PlotUI ({PlotId}): Inicializado y suscrito a eventos. Estado inicial: IsUnlocked={currentPlotData.isUnlocked}, Crop={currentPlotData.assignedCropSO?.name ?? "Ninguno"}.");
        Debug.Log($"PlotUI ({PlotId}): Start_End");
    }
    
    private void ValidateReferences()
    {
        // plotImage ya se asigna en Awake si es nulo.
        if (cropIconImage == null) Debug.LogWarning($"PlotUI ({PlotId}): Referencia a 'Crop Icon Image' no asignada en el Inspector.");
        if (lockedOverlay == null) Debug.LogWarning($"PlotUI ({PlotId}): Referencia a 'Locked Overlay' no asignada en el Inspector.");
        // Solo advertir sobre costText si lockedOverlay SÍ está asignado, ya que costText depende de él.
        if (lockedOverlay != null && costText == null) Debug.LogWarning($"PlotUI ({PlotId}): Referencia a 'Cost Text' no asignada (y 'Locked Overlay' sí lo está).");
        if (progressBar != null) progressBar.fillAmount = 0; 
        else Debug.LogWarning($"PlotUI ({PlotId}): Referencia a 'Progress Bar' no asignada en el Inspector (opcional pero recomendado).");

        if (emptyPlotSprite == null) Debug.LogWarning($"PlotUI ({PlotId}): Sprite 'Empty Plot Sprite' no asignado.");
        if (lockedPlotSprite == null) Debug.LogWarning($"PlotUI ({PlotId}): Sprite 'Locked Plot Sprite' no asignado.");
    }

    void OnEnable()
    {
        // Si el objeto se reactiva, es buena idea re-suscribirse y actualizar,
        // especialmente si FarmingManager podría haber cambiado o los eventos se perdieron.
        // Sin embargo, dado que Start() ya lo hace y OnDestroy() desuscribe,
        // solo lo haremos si farmingManager no es nulo (evitando error si Awake falló).
        if (farmingManager != null && currentPlotData != null) // Asegurar que Start tuvo éxito
        {
            Debug.Log($"PlotUI ({PlotId}): OnEnable_Start - Re-suscribiendo y actualizando visuales.");
            SubscribeToEvents(); // Suscribir por si acaso (aunque Start ya lo hace)
            UpdateVisuals(); // Actualizar visuales al reactivar
            Debug.Log($"PlotUI ({PlotId}): OnEnable_End");
        }
    }

    void OnDisable()
    {
        // Es más robusto desuscribir en OnDisable que en OnDestroy
        // para evitar problemas si el objeto se desactiva y reactiva.
        Debug.Log($"PlotUI ({PlotId}): OnDisable_Start - Desuscribiendo de eventos.");
        UnsubscribeFromEvents();
        Debug.Log($"PlotUI ({PlotId}): OnDisable_End");
    }

    private void SubscribeToEvents()
    {
        if (farmingManager == null) return;
        
        // Remover primero para evitar doble suscripción si OnEnable se llama múltiples veces
        UnsubscribeFromEvents(); 

        farmingManager.OnCropAssigned += HandleCropAssigned;
        farmingManager.OnPlotEmptied += HandlePlotEmptied;
        farmingManager.OnPlotUnlocked += HandlePlotUnlocked;
        farmingManager.OnPlotGrowthUpdate += HandleGrowthUpdate;
        farmingManager.OnPlantingModeExited += HandlePlantingModeExited;
        Debug.Log($"PlotUI ({PlotId}): Suscrito a eventos de FarmingManager.");
    }

     private void UnsubscribeFromEvents()
    {
         if (farmingManager == null) return;
        farmingManager.OnCropAssigned -= HandleCropAssigned;
        farmingManager.OnPlotEmptied -= HandlePlotEmptied;
        farmingManager.OnPlotUnlocked -= HandlePlotUnlocked;
        farmingManager.OnPlotGrowthUpdate -= HandleGrowthUpdate;
        farmingManager.OnPlantingModeExited -= HandlePlantingModeExited;
        // No es necesario un log aquí si SubscribeToEvents ya loguea la suscripción.
    }

    private void HandleCropAssigned(int changedPlotId, CropSO crop)
    {
        if (changedPlotId == this.PlotId)
        {
             Debug.Log($"PlotUI ({PlotId}): Evento OnCropAssigned recibido para {crop?.name ?? "ninguno"}. Actualizando visuales.");
            currentPlotData = farmingManager.GetPlotData(PlotId); // Re-obtener data por si acaso.
            UpdateVisuals();
        }
    }

     private void HandlePlotEmptied(int changedPlotId)
    {
        if (changedPlotId == this.PlotId)
        {
            Debug.Log($"PlotUI ({PlotId}): Evento OnPlotEmptied recibido. Actualizando visuales.");
            currentPlotData = farmingManager.GetPlotData(PlotId);
            UpdateVisuals();
        }
    }

      private void HandlePlotUnlocked(int changedPlotId)
    {
         if (changedPlotId == this.PlotId)
        {
             Debug.Log($"PlotUI ({PlotId}): Evento OnPlotUnlocked recibido. Actualizando visuales.");
            currentPlotData = farmingManager.GetPlotData(PlotId);
            UpdateVisuals();
        }
    }

    private void HandleGrowthUpdate(int changedPlotId, float elapsedNormalized, float totalDurationMinutes)
    {
         if (changedPlotId == this.PlotId)
         {
             if (progressBar != null)
             {
                 // Asumiendo que elapsedNormalized es el progreso de 0 a 1.
                 // Si no, calcular: progressBar.fillAmount = Mathf.Clamp01(elapsedSeconds / (totalDurationMinutes * 60f));
                 progressBar.fillAmount = Mathf.Clamp01(elapsedNormalized); 
             }
             // No es necesario un log aquí, puede ser muy frecuente.
         }
    }

    private void HandlePlantingModeExited()
    {
        Debug.Log($"PlotUI ({PlotId}): Evento OnPlantingModeExited recibido.");
        // Aquí podríamos, por ejemplo, cambiar el cursor si esta parcela estaba resaltada.
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (farmingManager == null || currentPlotData == null)
        {
            Debug.LogWarning($"PlotUI ({PlotId}): Click ignorado, FarmingManager o PlotData no están disponibles.");
            return;
        }

        Debug.Log($"PlotUI ({PlotId}): Click detectado. Crop seleccionado: {farmingManager.GetSelectedCropForPlanting()?.name ?? "Ninguno"}. Parcela desbloqueada: {currentPlotData.isUnlocked}.");

        CropSO selectedCrop = farmingManager.GetSelectedCropForPlanting();

        if (selectedCrop != null)
        {
            Debug.Log($"PlotUI ({PlotId}): Intentando plantar {selectedCrop.name}.");
            bool planted = farmingManager.AttemptPlantOnPlot(PlotId);
             if (planted)
             {
                  Debug.Log($"PlotUI ({PlotId}): Plantación delegada a FarmingManager (éxito esperado). Visuales se actualizarán por evento.");
             }
             else
             {
                 Debug.LogWarning($"PlotUI ({PlotId}): Plantación delegada a FarmingManager (fallo esperado). Parcela bloqueada, ocupada o ID erróneo?");
                 // Aquí se podría disparar un evento local para feedback de UI (ej. sonido de error)
                 // GameEvents.OnUIFeedback?.Invoke(FeedbackType.ErrorSound);
             }
        }
        else if (!currentPlotData.isUnlocked)
        {
            Debug.Log($"PlotUI ({PlotId}): Parcela bloqueada. Intentando desbloquear (simulado). Coste: {currentPlotData.purchaseCost}");
            // En un juego real, aquí se abriría un diálogo de confirmación de compra.
            // farmingManager.AttemptUnlockPlot(PlotId, currentPlotData.purchaseCost);
            // Por ahora, desbloqueo directo simulado:
            bool unlocked = farmingManager.UnlockPlot(PlotId); 
             if (unlocked)
             {
                 Debug.Log($"PlotUI ({PlotId}): Desbloqueo delegado a FarmingManager (éxito esperado).");
             }
             else
             {
                 Debug.LogWarning($"PlotUI ({PlotId}): Desbloqueo delegado a FarmingManager (fallo esperado).");
             }
        }
         else
         {
              Debug.Log($"PlotUI ({PlotId}): Click en parcela desbloqueada/vacía/cultivada pero sin un cultivo seleccionado para plantar. No hay acción directa aquí.");
              // Podría ser un punto para mostrar info de la parcela si está cultivada, por ejemplo.
              if(currentPlotData.assignedCropSO != null)
              {
                Debug.Log($"PlotUI ({PlotId}): Contiene {currentPlotData.assignedCropSO.name}. Tiempo restante: {currentPlotData.GetRemainingGrowthTimeSeconds() / 60f:F1} min.");
              }
         }
    }

    private void UpdateVisuals()
    {
        if (currentPlotData == null) 
        {
            Debug.LogError($"PlotUI ({PlotId}): No se pueden actualizar visuales, currentPlotData es nulo.");
            // Podríamos intentar ocultar el PlotUI o mostrar un estado de error.
            if(plotImage != null) plotImage.gameObject.SetActive(false);
            return;
        }
        if(plotImage == null)
        {
            Debug.LogError($"PlotUI ({PlotId}): No se pueden actualizar visuales, plotImage es nulo.");
            return;
        }


        plotImage.gameObject.SetActive(true); // Asegurar que esté activo si se desactivó por error.

        if (!currentPlotData.isUnlocked)
        {
            ShowLockedState();
        }
        else if (currentPlotData.assignedCropSO != null)
        {
            ShowGrowingState(currentPlotData.assignedCropSO);
        }
        else
        {
            ShowEmptyState();
        }
        //Debug.Log($"PlotUI ({PlotId}): Visuales actualizados."); // Puede ser muy verboso
    }

    private void ShowLockedState()
    {
        if (plotImage != null) plotImage.sprite = lockedPlotSprite;
        if (cropIconImage != null) cropIconImage.enabled = false;
        if (lockedOverlay != null) lockedOverlay.SetActive(true);
        
        if (costText != null && currentPlotData != null) // Asegurar que currentPlotData no es nulo
        {
            // No necesitamos FeedbackMessagesSO aquí si es solo el número.
            // Si fuera "Cost: {0}", usaríamos feedbackMessagesSO.GetMessage("PlotUnlockCost", currentPlotData.purchaseCost);
            costText.text = currentPlotData.purchaseCost.ToString(); 
        }
        else if (costText == null && lockedOverlay != null)
        {
             // Ya se advirtió en ValidateReferences
        }
        
        if (progressBar != null) progressBar.fillAmount = 0;
        Debug.Log($"PlotUI ({PlotId}): Mostrando estado: BLOQUEADO. Coste: {currentPlotData?.purchaseCost ?? 0}");
    }

    private void ShowEmptyState()
    {
        if (plotImage != null) plotImage.sprite = emptyPlotSprite;
        if (cropIconImage != null) cropIconImage.enabled = false;
        if (lockedOverlay != null) lockedOverlay.SetActive(false);
        if (progressBar != null) progressBar.fillAmount = 0;
        Debug.Log($"PlotUI ({PlotId}): Mostrando estado: VACÍO.");
    }

    private void ShowGrowingState(CropSO crop)
    {
        if (plotImage != null) plotImage.sprite = emptyPlotSprite; // Fondo base
        if (cropIconImage != null && crop != null)
        {
            cropIconImage.sprite = crop.Icon; // Asumiendo que CropSO tiene una propiedad publica Sprite Icon
            cropIconImage.enabled = true;
        }
        else if(cropIconImage != null && crop == null) // Cultivo asignado pero SO es nulo?
        {
            Debug.LogWarning($"PlotUI ({PlotId}): assignedCropSO es nulo en ShowGrowingState, aunque la parcela indica que tiene un cultivo.");
            cropIconImage.enabled = false;
        }

        if (lockedOverlay != null) lockedOverlay.SetActive(false);
        // La barra de progreso se actualiza vía HandleGrowthUpdate, no es necesario resetearla aquí
        // a menos que queramos un valor inicial antes del primer update.
        // if (progressBar != null) progressBar.fillAmount = currentPlotData.GetNormalizedProgress(); // Si PlotData tuviera este método
        Debug.Log($"PlotUI ({PlotId}): Mostrando estado: CRECIENDO ({crop?.name ?? "Error: CropSO Nulo"}).");
    }
}

// ScriptRole: Controls the visual representation and interaction for a single farm plot UI element.
// Dependencies: Image (on the same GameObject)
// HandlesEvents: FarmingManager.OnCropAssigned, FarmingManager.OnPlotEmptied, FarmingManager.OnPlotUnlocked, FarmingManager.OnPlotGrowthUpdate, FarmingManager.OnPlantingModeExited
// TriggersEvents: None (communicates with FarmingManager via direct calls)
// UsesSO: CropSO (indirectly, to get the crop icon via PlotData)
// NeedsSetup: plotId (int), plotImage (Image), cropIconImage (Image), lockedOverlay (GameObject), costText (TextMeshProUGUI), progressBar (Image), emptyPlotSprite (Sprite), lockedPlotSprite (Sprite) 