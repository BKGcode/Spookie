using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class FarmingManager : MonoBehaviour
{
    [Header("Configuración Inicial")]
    [Tooltip("Define el estado inicial de todas las parcelas del juego.")]
    [SerializeField] private List<PlotData> initialPlots = new List<PlotData>();

    [Tooltip("Lista de todos los ScriptableObjects de cultivos posibles en el juego.")]
    [SerializeField] private List<CropSO> allPossibleCrops = new List<CropSO>();

    // --- Estado Interno ---
    private Dictionary<int, PlotData> plots = new Dictionary<int, PlotData>();
    private HashSet<CropSO> unlockedCrops = new HashSet<CropSO>();
    private CropSO selectedCropForPlanting = null;

    // --- Eventos Públicos ---
    public event Action<CropSO> OnCropSelectedForPlanting; // Para UI (Toolbar)
    public event Action OnPlantingModeExited; // Para UI (Toolbar)
    public event Action<int, CropSO> OnCropAssigned; // Para PlotUI y Guardado
    public event Action<int, CropSO, int> OnCropHarvested; // Para FeedbackController y Economía
    public event Action<int> OnPlotEmptied; // Para PlotUI (después de feedback) y Guardado
    public event Action<int, float, float> OnPlotGrowthUpdate; // Para PlotUI (progreso visual)
    public event Action<int> OnPlotUnlocked; // Para UI y Guardado
    public event Action<CropSO> OnCropUnlocked; // Para UI (Toolbar) y Guardado

    void Awake()
    {
        Debug.Log("FarmingManager: Awake_Start");
        InitializePlots();
        InitializeUnlockedCrops();
        Debug.Log("FarmingManager: Awake_End");
    }

    void Update()
    {
        CheckCropGrowth();
    }

    private void InitializePlots()
    {
        Debug.Log("FarmingManager: InitializePlots_Start");
        plots.Clear();
        foreach (var plotData in initialPlots)
        {
            if (!plots.ContainsKey(plotData.plotId))
            {
                // Creamos una nueva instancia para evitar modificar el estado inicial directamente
                plots.Add(plotData.plotId, new PlotData(plotData.plotId, plotData.isUnlocked, plotData.purchaseCost));
                // Si la parcela inicial ya tenía algo plantado (para debug), lo respetamos,
                // pero normalmente empezarán vacías o según datos guardados.
                if (plotData.assignedCropSO != null)
                {
                     // Ojo: Esto es simplificado, idealmente se cargaría el estado guardado.
                    plots[plotData.plotId].assignedCropSO = plotData.assignedCropSO;
                    plots[plotData.plotId].growthStartTime = plotData.growthStartTime > 0 ? plotData.growthStartTime : Time.time; // Asegurar tiempo válido
                    plots[plotData.plotId].currentGrowthDurationMinutes = plotData.currentGrowthDurationMinutes > 0 ? plotData.currentGrowthDurationMinutes : plotData.assignedCropSO.BaseGrowthTimeMinutes;
                }
            }
            else
            {
                Debug.LogWarning($"FarmingManager: Plot ID {plotData.plotId} duplicado en initialPlots. Ignorando duplicado.");
            }
        }
         Debug.Log($"FarmingManager: Inicializadas {plots.Count} parcelas.");
         Debug.Log("FarmingManager: InitializePlots_End");
    }

     private void InitializeUnlockedCrops()
    {
        Debug.Log("FarmingManager: InitializeUnlockedCrops_Start");
        unlockedCrops.Clear();
        foreach (var crop in allPossibleCrops)
        {
            if (crop != null && crop.IsUnlockedByDefault)
            {
                unlockedCrops.Add(crop);
                 Debug.Log($"FarmingManager: Cultivo inicial desbloqueado: {crop.name}");
            }
        }
        Debug.Log("FarmingManager: InitializeUnlockedCrops_End");
    }

    private void CheckCropGrowth()
    {
        // Usamos ToList para evitar modificar la colección mientras iteramos
        foreach (var plot in plots.Values.ToList())
        {
            if (plot.isUnlocked && plot.assignedCropSO != null)
            {
                if (plot.IsReadyToHarvest())
                {
                    HarvestCrop(plot.plotId);
                }
                else
                {
                    // Calcular progreso para UI
                    float elapsedMinutes = (Time.time - plot.growthStartTime) / 60.0f;
                    OnPlotGrowthUpdate?.Invoke(plot.plotId, elapsedMinutes, plot.currentGrowthDurationMinutes);
                }
            }
        }
    }

    private void HarvestCrop(int plotId)
    {
        if (plots.TryGetValue(plotId, out PlotData plot) && plot.assignedCropSO != null && plot.IsReadyToHarvest())
        {
            CropSO harvestedCrop = plot.assignedCropSO;
            int reward = harvestedCrop.RewardValue;

            Debug.Log($"FarmingManager: Cosechando {harvestedCrop.name} de parcela {plotId}. Recompensa: {reward}");

            // 1. Disparar evento para que otros sistemas reaccionen (VFX, SFX, Economía)
            OnCropHarvested?.Invoke(plotId, harvestedCrop, reward);

            // 2. TEMPORAL: Vaciar la parcela inmediatamente hasta tener HarvestFeedbackController
            // En el futuro, esto lo llamaría el HarvestFeedbackController cuando terminen los efectos.
            MarkPlotAsEmpty(plotId);
        }
        else if(plot.assignedCropSO != null && !plot.IsReadyToHarvest())
        {
            // No es un error, solo aún no está listo. No requiere log a menos que sea para debug intensivo.
        }
        else
        {
             Debug.LogWarning($"FarmingManager: Intento de cosechar parcela {plotId} vacía o no lista.");
        }
    }

    // --- Métodos Públicos para Interacción ---

    public void SelectCropForPlanting(CropSO crop)
    {
        if (crop != null && unlockedCrops.Contains(crop))
        {
            selectedCropForPlanting = crop;
            OnCropSelectedForPlanting?.Invoke(crop);
            Debug.Log($"FarmingManager: Cultivo seleccionado para plantar: {crop.name}");
        }
        else if (crop == null) // Permitir deseleccionar
        {
             ExitPlantingMode();
        }
        else if (crop != null && !unlockedCrops.Contains(crop))
        {
             Debug.LogWarning($"FarmingManager: Intento de seleccionar cultivo no desbloqueado: {crop.name}");
        }
    }

     public void ExitPlantingMode()
    {
        if (selectedCropForPlanting != null)
        {
            selectedCropForPlanting = null;
            OnPlantingModeExited?.Invoke();
            Debug.Log("FarmingManager: Saliendo del modo plantar.");
        }
    }


    public bool AttemptPlantOnPlot(int plotId)
    {
        if (selectedCropForPlanting == null)
        {
             //Debug.Log("FarmingManager: Intento de plantar sin cultivo seleccionado.");
            return false; // No hay cultivo seleccionado
        }

        if (plots.TryGetValue(plotId, out PlotData plot))
        {
            if (!plot.isUnlocked)
            {
                //Debug.Log($"FarmingManager: Intento de plantar en parcela bloqueada: {plotId}");
                return false; // Parcela bloqueada
            }

            if (plot.assignedCropSO != null)
            {
                //Debug.Log($"FarmingManager: Intento de plantar en parcela ocupada: {plotId}");
                return false; // Parcela ya ocupada
            }

            // ¡Éxito! Plantar el cultivo
            plot.assignedCropSO = selectedCropForPlanting;
            plot.growthStartTime = Time.time;
            // Por ahora, usamos el tiempo base. Modificadores se aplicarían aquí.
            plot.currentGrowthDurationMinutes = selectedCropForPlanting.BaseGrowthTimeMinutes;

            OnCropAssigned?.Invoke(plotId, selectedCropForPlanting);
            Debug.Log($"FarmingManager: Plantado {selectedCropForPlanting.name} en parcela {plotId}. Duración: {plot.currentGrowthDurationMinutes} min.");
            return true;
        }
        else
        {
            Debug.LogWarning($"FarmingManager: Intento de plantar en Plot ID inválido: {plotId}");
            return false; // ID de parcela no encontrado
        }
    }

    // Llamado por el futuro HarvestFeedbackController o temporalmente por HarvestCrop
    public void MarkPlotAsEmpty(int plotId)
    {
         if (plots.TryGetValue(plotId, out PlotData plot))
         {
            if (plot.assignedCropSO != null) // Solo si realmente había algo
            {
                plot.ClearPlot();
                OnPlotEmptied?.Invoke(plotId);
                Debug.Log($"FarmingManager: Parcela {plotId} vaciada.");
            }
         }
         else
         {
            Debug.LogWarning($"FarmingManager: Intento de vaciar parcela con ID inválido: {plotId}");
         }
    }


    // --- Métodos para Desbloqueo (Simulados por ahora) ---

    public bool UnlockPlot(int plotId)
    {
        if (plots.TryGetValue(plotId, out PlotData plot) && !plot.isUnlocked)
        {
            // Aquí iría la lógica de verificar si el jugador tiene suficientes monedas
            // Por ahora, simplemente desbloqueamos
            plot.isUnlocked = true;
            OnPlotUnlocked?.Invoke(plotId);
            Debug.Log($"FarmingManager: Parcela {plotId} desbloqueada.");
            return true;
            // Faltaría restar monedas del jugador
        }
        if(plot != null && plot.isUnlocked)
        {
            Debug.Log($"FarmingManager: Parcela {plotId} ya está desbloqueada.");
        }
        else
        {
            Debug.LogWarning($"FarmingManager: Intento de desbloquear parcela con ID inválido: {plotId}");
        }
        return false;
    }

    public bool UnlockCrop(CropSO crop)
    {
        if (crop == null)
        {
            Debug.LogWarning($"FarmingManager: Intento de desbloquear un CropSO nulo.");
            return false;
        }
        if (!allPossibleCrops.Contains(crop))
        {
            Debug.LogWarning($"FarmingManager: Intento de desbloquear CropSO '{crop.name}' que no está en la lista 'allPossibleCrops'.");
            return false;
        }

        if (!unlockedCrops.Contains(crop))
        {
            // Aquí iría la lógica de verificar si el jugador tiene suficientes monedas/condiciones
            unlockedCrops.Add(crop);
            OnCropUnlocked?.Invoke(crop);
            Debug.Log($"FarmingManager: Cultivo {crop.name} desbloqueado.");
            return true;
        }
        else
        {
            Debug.Log($"FarmingManager: Cultivo {crop.name} ya está desbloqueado.");
        }
        return false;
    }

    // --- Getters para otros sistemas (UI, Guardado) ---

    public PlotData GetPlotData(int plotId)
    {
        if (plots.TryGetValue(plotId, out PlotData plot))
        {
            return plot;
        }
        Debug.LogWarning($"FarmingManager: No se encontró PlotData para el ID: {plotId}.");
        return null; 
    }

    public List<PlotData> GetAllPlotData()
    {
        return new List<PlotData>(plots.Values); // Devolvemos una nueva lista para proteger la original
    }

     public List<CropSO> GetUnlockedCrops()
    {
        return new List<CropSO>(unlockedCrops); // Devolvemos una nueva lista
    }

      public CropSO GetSelectedCropForPlanting()
    {
        return selectedCropForPlanting;
    }
}

// ScriptRole: Gestiona la lógica central del sistema de farming, incluyendo el estado de las parcelas, crecimiento, cosecha y selección de cultivos.
// RelatedScripts: PlotData, PlotUI, CropToolbarUI, CropSO, (Futuro: HarvestFeedbackController, EconomyManager, SaveLoadManager)
// UsesSO: CropSO
// SendsTo: PlotUI, CropToolbarUI, HarvestFeedbackController, EconomyManager, SaveLoadManager (a través de eventos) 