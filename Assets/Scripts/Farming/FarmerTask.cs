using UnityEngine;

public enum FarmerTaskType 
{
    Plant,
    Water,
    Harvest
}

[System.Serializable]
public class FarmerTask
{
    public FarmerTaskType TaskType { get; private set; }
    public PlotData TargetPlot { get; private set; } // Referencia a los datos de la parcela
    public CropSO CropToPlant { get; private set; } // Solo para tareas de Plantar
    public Transform DestinationObject { get; private set; } // Para Pozo o Granja

    // Constructor para tareas de Plantar
    public FarmerTask(FarmerTaskType taskType, PlotData targetPlot, CropSO cropToPlant, Transform destination = null)
    {
        if (taskType != FarmerTaskType.Plant)
        {
            Debug.LogError("FarmerTask: Constructor incorrecto para tareas que no son Plantar. Falta CropToPlant.");
        }
        TaskType = taskType;
        TargetPlot = targetPlot;
        CropToPlant = cropToPlant;
        DestinationObject = destination; // Plot es el destino inicial
        Debug.Log($"FarmerTask Creada: {TaskType} en Plot {TargetPlot.plotId} con Cultivo {CropToPlant?.name ?? "N/A"}. Destino Visual: {DestinationObject?.name ?? "Plot"}");
    }

    // Constructor para tareas de Regar o Cosechar (que pueden tener un destino visual adicional)
    public FarmerTask(FarmerTaskType taskType, PlotData targetPlot, Transform destinationObject = null)
    {
        if (taskType == FarmerTaskType.Plant)
        {
            Debug.LogError("FarmerTask: Constructor incorrecto para tareas de Plantar. Usar el constructor que incluye CropSO.");
        }
        TaskType = taskType;
        TargetPlot = targetPlot;
        CropToPlant = null; // No aplica directamente aquí
        DestinationObject = destinationObject; // Pozo o Granja (o la parcela si no hay destino secundario)
        Debug.Log($"FarmerTask Creada: {TaskType} en Plot {TargetPlot.plotId}. Destino Visual: {DestinationObject?.name ?? "Plot"}");
    }
}

// ScriptRole: Defines the structure for a task that a Farmer can perform.
// Dependencies: PlotData, CropSO
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: CropSO (optionally, for planting tasks)
// NeedsSetup: Parameters are set via constructor by FarmerManager. 