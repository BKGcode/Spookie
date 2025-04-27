using UnityEngine;
using UnityEngine.UI; // Necesario para Image y Button
using TMPro; // Necesario para TextMeshProUGUI
using UnityEngine.EventSystems; // Necesario para IPointerClickHandler

[RequireComponent(typeof(Image))] // Asegura que haya un componente Image
public class PlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración de Parcela")]
    [Tooltip("ID único de esta parcela. Debe coincidir con uno de los IDs en FarmingManager -> InitialPlots.")]
    [SerializeField] private int plotId;

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

    void Start()
    {
        // Buscar el FarmingManager (forma simple, podría mejorarse con Singleton o inyección)
        farmingManager = FindObjectOfType<FarmingManager>();
        if (farmingManager == null)
        {
            Debug.LogError($"PlotUI ({plotId}): No se encontró FarmingManager en la escena.");
            enabled = false; // Deshabilitar si no hay manager
            return;
        }

        // Validar configuración inicial
        if (plotImage == null) plotImage = GetComponent<Image>();
        if (cropIconImage == null) Debug.LogWarning($"PlotUI ({plotId}): Falta referencia a Crop Icon Image.");
        if (lockedOverlay == null) Debug.LogWarning($"PlotUI ({plotId}): Falta referencia a Locked Overlay.");
        if (costText == null && lockedOverlay != null) Debug.LogWarning($"PlotUI ({plotId}): Falta referencia a Cost Text dentro de Locked Overlay.");
         if (progressBar != null) progressBar.fillAmount = 0; // Resetear barra progreso

        // Obtener estado inicial y suscribirse a eventos
        currentPlotData = farmingManager.GetPlotData(plotId);
        if (currentPlotData == null)
        {
             Debug.LogError($"PlotUI ({plotId}): No se encontró PlotData para este ID en FarmingManager. Asegúrate que el ID existe en InitialPlots.");
             enabled = false;
             return;
        }

        SubscribeToEvents();
        UpdateVisuals(); // Mostrar estado inicial correcto
         Debug.Log($"PlotUI ({plotId}): Inicializado y suscrito a eventos.");
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (farmingManager == null) return;
        // Escuchar cambios específicos para ESTA parcela
        farmingManager.OnCropAssigned += HandleCropAssigned;
        farmingManager.OnPlotEmptied += HandlePlotEmptied;
        farmingManager.OnPlotUnlocked += HandlePlotUnlocked;
        farmingManager.OnPlotGrowthUpdate += HandleGrowthUpdate;
        // Escuchar cambios globales que podrían afectarnos
        farmingManager.OnPlantingModeExited += HandlePlantingModeExit; // Para cambiar cursor/feedback?
    }

     private void UnsubscribeFromEvents()
    {
         if (farmingManager == null) return;
        farmingManager.OnCropAssigned -= HandleCropAssigned;
        farmingManager.OnPlotEmptied -= HandlePlotEmptied;
        farmingManager.OnPlotUnlocked -= HandlePlotUnlocked;
        farmingManager.OnPlotGrowthUpdate -= HandleGrowthUpdate;
        farmingManager.OnPlantingModeExited -= HandlePlantingModeExit;
    }

    // --- Manejadores de Eventos del FarmingManager ---

    private void HandleCropAssigned(int changedPlotId, CropSO crop)
    {
        if (changedPlotId == this.plotId)
        {
             Debug.Log($"PlotUI ({plotId}): Recibido evento OnCropAssigned.");
            // No necesitamos recargar data aquí porque el FarmingManager ya la actualizó.
            UpdateVisuals();
        }
    }

     private void HandlePlotEmptied(int changedPlotId)
    {
        if (changedPlotId == this.plotId)
        {
            Debug.Log($"PlotUI ({plotId}): Recibido evento OnPlotEmptied.");
            UpdateVisuals();
        }
    }

      private void HandlePlotUnlocked(int changedPlotId)
    {
         if (changedPlotId == this.plotId)
        {
             Debug.Log($"PlotUI ({plotId}): Recibido evento OnPlotUnlocked.");
            UpdateVisuals();
        }
    }

    private void HandleGrowthUpdate(int changedPlotId, float elapsedMinutes, float totalMinutes)
    {
         if (changedPlotId == this.plotId && progressBar != null)
         {
             if (totalMinutes > 0)
             {
                 progressBar.fillAmount = Mathf.Clamp01(elapsedMinutes / totalMinutes);
             }
             else
             {
                  progressBar.fillAmount = 0; // Evitar división por cero
             }
         }
    }

    private void HandlePlantingModeExit()
    {
        // Podríamos cambiar el cursor o algún feedback visual aquí si es necesario
    }


    // --- Lógica de Interacción ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (farmingManager == null || currentPlotData == null) return;

         Debug.Log($"PlotUI ({plotId}): Click detectado.");

        // 1. Intentar plantar si estamos en modo plantar
        if (farmingManager.GetSelectedCropForPlanting() != null)
        {
            bool planted = farmingManager.AttemptPlantOnPlot(plotId);
             if (planted)
             {
                  Debug.Log($"PlotUI ({plotId}): Plantación exitosa (llamada a FM).");
                 // UpdateVisuals() se llamará automáticamente por el evento OnCropAssigned
             }
             else
             {
                 Debug.Log($"PlotUI ({plotId}): Plantación fallida (llamada a FM). Plot bloqueado u ocupado?");
                 // Podríamos dar feedback aquí (sonido de error, etc.)
             }
        }
        // 2. Si no estamos plantando y la parcela está bloqueada, intentar desbloquear (simulado)
        else if (!currentPlotData.isUnlocked)
        {
             Debug.Log($"PlotUI ({plotId}): Intentando desbloquear (llamada simulada a FM).");
            // Aquí iría la lógica para mostrar un panel de confirmación de compra
            // y si se confirma, llamar a farmingManager.UnlockPlot(plotId)
            // Por ahora, lo simulamos directamente si hacemos clic:
            bool unlocked = farmingManager.UnlockPlot(plotId); // Simulación directa
             if (unlocked)
             {
                 // UpdateVisuals() se llamará por el evento OnPlotUnlocked
             }
        }
         else
         {
              Debug.Log($"PlotUI ({plotId}): Click en parcela desbloqueada/vacía pero sin cultivo seleccionado.");
         }
    }

    // --- Actualización Visual ---

    private void UpdateVisuals()
    {
        if (currentPlotData == null) return;

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
         //Debug.Log($"PlotUI ({plotId}): Visuals actualizados.");
    }

    private void ShowLockedState()
    {
        plotImage.sprite = lockedPlotSprite;
        if (cropIconImage != null) cropIconImage.enabled = false;
        if (lockedOverlay != null) lockedOverlay.SetActive(true);
        if (costText != null) costText.text = currentPlotData.purchaseCost.ToString(); // Mostrar coste
         if (progressBar != null) progressBar.fillAmount = 0;
         //Debug.Log($"PlotUI ({plotId}): Mostrando estado BLOQUEADO.");
    }

    private void ShowEmptyState()
    {
        plotImage.sprite = emptyPlotSprite;
        if (cropIconImage != null) cropIconImage.enabled = false;
        if (lockedOverlay != null) lockedOverlay.SetActive(false);
         if (progressBar != null) progressBar.fillAmount = 0;
        // Debug.Log($"PlotUI ({plotId}): Mostrando estado VACÍO.");
    }

    private void ShowGrowingState(CropSO crop)
    {
        plotImage.sprite = emptyPlotSprite; // Fondo vacío
        if (cropIconImage != null)
        {
            cropIconImage.sprite = crop.Icon;
            cropIconImage.enabled = true;
        }
        if (lockedOverlay != null) lockedOverlay.SetActive(false);
         if (progressBar != null)
         {   
             // El progreso se actualiza en HandleGrowthUpdate
         }
          //Debug.Log($"PlotUI ({plotId}): Mostrando estado CRECIENDO ({crop.name}).");
    }
}

// ScriptRole: Controla la representación visual y la interacción de una única parcela de cultivo.
// RelatedScripts: FarmingManager, PlotData
// UsesSO: CropSO (indirectamente, para obtener icono)
// ReceivesFrom: FarmingManager (eventos), User Input (clicks)
// SendsTo: FarmingManager (AttemptPlantOnPlot, UnlockPlot) 