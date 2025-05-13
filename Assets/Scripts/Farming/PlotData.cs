using UnityEngine;

[System.Serializable]
public class PlotData
{
    [Tooltip("Identificador único de la parcela.")]
    public int plotId;

    [Tooltip("Indica si esta parcela está desbloqueada por el jugador.")]
    public bool isUnlocked;

    [Tooltip("Coste en monedas para desbloquear esta parcela (si isUnlocked es false).")]
    public int purchaseCost;

    [Tooltip("ScriptableObject del cultivo asignado a esta parcela (null si está vacía).")]
    public CropSO assignedCropSO;

    [Tooltip("Momento (usando Time.time) en que se inició el crecimiento del cultivo actual.")]
    public float growthStartTime; // En segundos

    [Tooltip("Duración total en MINUTOS requerida para el crecimiento del cultivo actual (podría incluir modificadores).")]
    public float currentGrowthDurationMinutes;

    public PlotData(int id, bool unlocked, int cost)
    {
        // No es necesario un Debug.Log aquí ya que esta clase es principalmente un contenedor de datos
        // y se instanciará frecuentemente.
        plotId = id;
        isUnlocked = unlocked;
        purchaseCost = cost;
        assignedCropSO = null;
        growthStartTime = 0f;
        currentGrowthDurationMinutes = 0f;
    }

    public bool IsReadyToHarvest()
    {
        if (!isUnlocked || assignedCropSO == null || currentGrowthDurationMinutes <= 0)
        {
            return false;
        }
        // growthStartTime está en segundos, currentGrowthDurationMinutes está en minutos
        float elapsedSeconds = Time.time - growthStartTime;
        float requiredSeconds = currentGrowthDurationMinutes * 60.0f;
        return elapsedSeconds >= requiredSeconds;
    }
    
    public float GetRemainingGrowthTimeSeconds()
    {
        if (!isUnlocked || assignedCropSO == null || currentGrowthDurationMinutes <= 0 || growthStartTime <= 0)
        {
            return 0f;
        }
        float requiredSeconds = currentGrowthDurationMinutes * 60.0f;
        float elapsedSeconds = Time.time - growthStartTime;
        return Mathf.Max(0f, requiredSeconds - elapsedSeconds);
    }

    public float GetNormalizedProgress()
    {
        if (!isUnlocked || assignedCropSO == null || currentGrowthDurationMinutes <= 0 || growthStartTime <= 0)
        {
            return 0f;
        }
        float requiredSeconds = currentGrowthDurationMinutes * 60.0f;
        float elapsedSeconds = Time.time - growthStartTime;
        return Mathf.Clamp01(elapsedSeconds / requiredSeconds);
    }

    public void ClearPlot()
    {
        // Debug.Log($"PlotData ({plotId}): Limpiando parcela."); // Podría ser útil para debug
        assignedCropSO = null;
        growthStartTime = 0f;
        currentGrowthDurationMinutes = 0f;
    }
}

// ScriptRole: Stores the dynamic state of a single farm plot. It's a data container class.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: CropSO (as a data field 'assignedCropSO')
// NeedsSetup: Fields are typically set by FarmingManager or initial data. 