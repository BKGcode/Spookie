using UnityEngine;

[System.Serializable]
public class PlotData
{
    [Tooltip("Identificador único de la parcela")]
    public int plotId; // Hacemos públicos los campos para facilitar la serialización y el acceso desde FarmingManager

    [Tooltip("¿Está esta parcela desbloqueada por el jugador?")]
    public bool isUnlocked;

    [Tooltip("Coste en monedas para desbloquear esta parcela (si isUnlocked es false)")]
    public int purchaseCost;

    [Tooltip("Cultivo asignado a esta parcela (null si está vacía)")]
    public CropSO assignedCropSO;

    [Tooltip("Momento (Time.time) en que se inició el crecimiento del cultivo actual")]
    public float growthStartTime;

    [Tooltip("Duración total en MINUTOS requerida para el crecimiento del cultivo actual (puede diferir del baseGrowthTime del CropSO por modificadores)")]
    public float currentGrowthDurationMinutes;

    // Constructor opcional para inicialización si es necesario
    public PlotData(int id, bool unlocked, int cost)
    {
        plotId = id;
        isUnlocked = unlocked;
        purchaseCost = cost;
        assignedCropSO = null;
        growthStartTime = 0f;
        currentGrowthDurationMinutes = 0f;
    }

    // Método helper para saber si la parcela está lista (simplifica lógica en FarmingManager)
    public bool IsReadyToHarvest()
    {
        if (!isUnlocked || assignedCropSO == null || currentGrowthDurationMinutes <= 0)
        {
            return false;
        }

        float elapsedMinutes = (Time.time - growthStartTime) / 60.0f;
        return elapsedMinutes >= currentGrowthDurationMinutes;
    }

    // Método para limpiar la parcela después de cosechar o al inicializar
    public void ClearPlot()
    {
        assignedCropSO = null;
        growthStartTime = 0f;
        currentGrowthDurationMinutes = 0f;
    }
}

// ScriptRole: Almacena el estado dinámico de una única parcela de cultivo.
// RelatedScripts: FarmingManager, PlotUI
// UsesSO: CropSO 