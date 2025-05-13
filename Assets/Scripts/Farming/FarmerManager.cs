using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FarmerManager : MonoBehaviour
{
    [Header("Configuración de Granjeros")]
    [SerializeField] private GameObject farmerPrefab; // Prefab del Granjero
    [SerializeField] private int initialFarmerCount = 1;
    [SerializeField] private Transform farmerSpawnPoint; // Punto donde aparecen los granjeros inicialmente

    [Header("Referencias de Escena")]
    // Necesitaremos una forma de obtener las posiciones de las parcelas. 
    // Por ahora, podríamos requerir que las PlotUI tengan un transform identificable o un script específico.
    // De momento, el Farmer.AssignTask recibirá el Transform de la PlotUI directamente.
    [SerializeField] private List<Transform> wellTransforms = new List<Transform>(); // Lista de todos los pozos
    [SerializeField] private List<Transform> farmBuildingTransforms = new List<Transform>(); // Lista de todas las granjas/silos

    private List<Farmer> allFarmers = new List<Farmer>();
    private Queue<FarmerTask> taskQueue = new Queue<FarmerTask>();
    private FarmingManager farmingManager; // Referencia al gestor de la lógica de cultivo

    // Para evitar que múltiples granjeros intenten trabajar en la misma tarea/parcela simultáneamente
    private Dictionary<int, HashSet<FarmerTaskType>> plotTaskLocks = new Dictionary<int, HashSet<FarmerTaskType>>();


    void Awake()
    {
        Debug.Log("FarmerManager: Awake_Start");
        farmingManager = FindObjectOfType<FarmingManager>();
        if (farmingManager == null)
        {
            Debug.LogError("FarmerManager: No se encontró FarmingManager en la escena. FarmerManager no funcionará correctamente.");
            enabled = false;
            return;
        }
        Debug.Log("FarmerManager: Awake_End");
    }

    void Start()
    {
        Debug.Log("FarmerManager: Start_Start");
        if (!enabled) return;

        InitializeFarmers();
        SubscribeToFarmingEvents();
        Debug.Log($"FarmerManager: Inicializado con {allFarmers.Count} granjeros y suscrito a eventos.");
        Debug.Log("FarmerManager: Start_End");
    }

    private void InitializeFarmers()
    {
        Debug.Log("FarmerManager: InitializeFarmers_Start");
        if (farmerPrefab == null)
        {
            Debug.LogError("FarmerManager: Prefab de granjero no asignado en el Inspector.");
            return;
        }
        for (int i = 0; i < initialFarmerCount; i++)
        {
            GameObject farmerGO = Instantiate(farmerPrefab, farmerSpawnPoint ? farmerSpawnPoint.position : transform.position, Quaternion.identity, transform); // Hijo del FarmerManager
            Farmer farmer = farmerGO.GetComponent<Farmer>();
            if (farmer != null)
            {
                farmer.Initialize(i, this);
                allFarmers.Add(farmer);
            }
            else
            {
                Debug.LogError("FarmerManager: Prefab de granjero no contiene el script Farmer.");
                Destroy(farmerGO);
            }
        }
        Debug.Log($"FarmerManager: {allFarmers.Count} granjeros creados.");
        Debug.Log("FarmerManager: InitializeFarmers_End");
    }

    private void SubscribeToFarmingEvents()
    {
        if (farmingManager == null) return;
        farmingManager.OnCropAssigned += HandleCropAssigned;
        farmingManager.OnCropHarvested += HandleCropHarvested; // Usaremos este para la animación de cosecha
        // Podríamos necesitar un evento OnPlotEmptied si la cosecha es en dos fases (granjero va, cosecha, luego parcela se vacía)
        // Por ahora, OnCropHarvested implica que el cultivo está listo para ser "recogido" visualmente.
    }

    private void OnDestroy()
    {
        if (farmingManager != null)
        {
            farmingManager.OnCropAssigned -= HandleCropAssigned;
            farmingManager.OnCropHarvested -= HandleCropHarvested;
        }
    }

    private void HandleCropAssigned(int plotId, CropSO crop)
    {
        Debug.Log($"FarmerManager: Evento OnCropAssigned recibido para Plot ID {plotId} con {crop.name}.");
        PlotData plotData = farmingManager.GetPlotData(plotId);
        PlotUI plotUI = FindPlotUI(plotId); // Necesitamos un método para encontrar el PlotUI y su Transform

        if (plotData == null || plotUI == null)
        {
            Debug.LogError($"FarmerManager: No se pudo encontrar PlotData o PlotUI para Plot ID {plotId} al asignar cultivo.");
            return;
        }

        // Tarea de Plantar (visual)
        AddTaskToQueue(new FarmerTask(FarmerTaskType.Plant, plotData, crop), plotUI.transform, null);
        // Tarea de Regar (visual)
        Transform nearestWell = GetNearestTransform(plotUI.transform, wellTransforms);
        if(nearestWell != null)
        {
            AddTaskToQueue(new FarmerTask(FarmerTaskType.Water, plotData, nearestWell), plotUI.transform, nearestWell);
        }
        else
        {
            Debug.LogWarning($"FarmerManager: No hay pozos (Well) configurados para la tarea de riego en Plot ID {plotId}.");
            // Opcionalmente, crear tarea de riego sin pozo, directo a parcela:
            // AddTaskToQueue(new FarmerTask(FarmerTaskType.Water, plotData), plotUI.transform, null);
        }
    }

    private void HandleCropHarvested(int plotId, CropSO crop, int reward)
    {
        Debug.Log($"FarmerManager: Evento OnCropHarvested recibido para Plot ID {plotId} ({crop.name}).");
        PlotData plotData = farmingManager.GetPlotData(plotId);
        PlotUI plotUI = FindPlotUI(plotId);
        Transform nearestFarmBuilding = GetNearestTransform(plotUI.transform, farmBuildingTransforms);

        if (plotData == null || plotUI == null)
        {
            Debug.LogError($"FarmerManager: No se pudo encontrar PlotData o PlotUI para Plot ID {plotId} al cosechar.");
            return;
        }

        if (nearestFarmBuilding == null)
        {
            Debug.LogWarning($"FarmerManager: No hay edificios de granja (Granja) configurados para la tarea de cosecha de Plot ID {plotId}.");
            // Si no hay granja, el granjero podría simplemente hacer la animación en la parcela.
            // AddTaskToQueue(new FarmerTask(FarmerTaskType.Harvest, plotData), plotUI.transform, null);
            return; // O simplemente no crear la tarea visual de cosecha si no hay destino.
        }
        
        AddTaskToQueue(new FarmerTask(FarmerTaskType.Harvest, plotData, nearestFarmBuilding), plotUI.transform, nearestFarmBuilding);
    }

    void Update()
    {
        ProcessTaskQueue();
    }

    private void AddTaskToQueue(FarmerTask task, Transform plotTransform, Transform secondaryTargetTransform)
    {
        if (!IsTaskLocked(task.TargetPlot.plotId, task.TaskType))
        {
            taskQueue.Enqueue(task);
            LockTask(task.TargetPlot.plotId, task.TaskType);
            Debug.Log($"FarmerManager: Tarea {task.TaskType} para Plot ID {task.TargetPlot.plotId} añadida a la cola.");
            ProcessTaskQueue(); // Intentar procesar inmediatamente
        }
        else
        {
            Debug.Log($"FarmerManager: Tarea {task.TaskType} para Plot ID {task.TargetPlot.plotId} ya está en progreso o en cola (bloqueada). No se añade.");
        }
    }

    private void ProcessTaskQueue()
    {
        if (taskQueue.Count > 0)
        {
            Farmer availableFarmer = GetAvailableFarmer();
            if (availableFarmer != null)
            {
                FarmerTask taskToAssign = taskQueue.Dequeue(); 
                PlotUI plotUI = FindPlotUI(taskToAssign.TargetPlot.plotId); // Necesario para el transform de la parcela
                if (plotUI != null)
                {                
                    // El FarmerTask ya tiene DestinationObject para pozo/granja, aquí pasamos el plotTransform
                    // y el DestinationObject del task como secondaryTargetTransform para el Farmer.
                    Transform taskSpecificSecondaryTarget = null;
                    if(taskToAssign.TaskType == FarmerTaskType.Water || taskToAssign.TaskType == FarmerTaskType.Harvest)
                    {
                        taskSpecificSecondaryTarget = taskToAssign.DestinationObject; // Esto sería el Pozo o la Granja
                    }

                    Debug.Log($"FarmerManager: Asignando tarea {taskToAssign.TaskType} de Plot ID {taskToAssign.TargetPlot.plotId} a {availableFarmer.name}.");
                    availableFarmer.AssignTask(taskToAssign, plotUI.transform, taskSpecificSecondaryTarget);
                    // El bloqueo se mantiene hasta que el granjero termine y lo libere.
                    // Considerar un callback o evento Farmer.OnTaskCompleted para liberar el lock.
                    // Por ahora, para simplificar, el lock se quita después de un tiempo o al asignar.
                    // **MEJORA FUTURA: El Farmer debería liberar el Lock al terminar su tarea.**
                    // StartCoroutine(UnlockTaskAfterDelay(taskToAssign.TargetPlot.plotId, taskToAssign.TaskType, 5f)); // Simulación temporal
                }
                else
                {
                    Debug.LogError($"FarmerManager: No se pudo encontrar PlotUI para Plot ID {taskToAssign.TargetPlot.plotId} al procesar la cola. Tarea descartada.");
                    UnlockTask(taskToAssign.TargetPlot.plotId, taskToAssign.TaskType); // Liberar lock si la tarea no se puede ejecutar
                }
            }
            // else Debug.Log("FarmerManager: Tareas en cola pero no hay granjeros disponibles.");
        }
    }

    private Farmer GetAvailableFarmer()
    {
        return allFarmers.FirstOrDefault(f => f.IsIdle());
    }

    // --- Gestión de Bloqueo de Tareas por Parcela ---
    private bool IsTaskLocked(int plotId, FarmerTaskType taskType)
    {
        return plotTaskLocks.ContainsKey(plotId) && plotTaskLocks[plotId].Contains(taskType);
    }

    private void LockTask(int plotId, FarmerTaskType taskType)
    {
        if (!plotTaskLocks.ContainsKey(plotId))
        {
            plotTaskLocks[plotId] = new HashSet<FarmerTaskType>();
        }
        plotTaskLocks[plotId].Add(taskType);
        Debug.Log($"FarmerManager: Bloqueada tarea {taskType} para Plot ID {plotId}.");
    }

    // ESTE MÉTODO DEBERÍA SER LLAMADO POR EL GRANJERO CUANDO COMPLETA SU TAREA
    public void NotifyTaskCompleted(int plotId, FarmerTaskType taskType) // Hecho public para que Farmer lo llame
    {
        UnlockTask(plotId, taskType);
        ProcessTaskQueue(); // Intentar procesar más tareas de la cola
    }

    private void UnlockTask(int plotId, FarmerTaskType taskType)
    {
        if (plotTaskLocks.ContainsKey(plotId))
        {
            plotTaskLocks[plotId].Remove(taskType);
            Debug.Log($"FarmerManager: Desbloqueada tarea {taskType} para Plot ID {plotId}.");
            if (plotTaskLocks[plotId].Count == 0)
            {
                plotTaskLocks.Remove(plotId);
            }
        }
    }
    
    // --- Utilidades ---
    private PlotUI FindPlotUI(int plotId)
    {
        // Esto es ineficiente. Idealmente, PlotUI se registraría con FarmerManager
        // o tendríamos un diccionario PlotID -> PlotUI.
        foreach (PlotUI plotUI_Instance in FindObjectsOfType<PlotUI>()) // Cuidado con FindObjectsOfType en Update
        {
            if (plotUI_Instance.PlotId == plotId) // Corregido: Usar la propiedad pública PlotId y quitar GetComponent redundante
            {
                return plotUI_Instance;
            }
        }
        return null;
    }

    private Transform GetNearestTransform(Transform origin, List<Transform> targets)
    {
        if (targets == null || targets.Count == 0) return null;
        
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (Transform target in targets)
        {
            if (target == null) continue;
            float distance = Vector3.Distance(origin.position, target.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = target;
            }
        }
        return nearest;
    }
}

// ScriptRole: Manages Farmer units, assigns them tasks based on FarmingManager events, and handles task queuing.
// Dependencies: FarmingManager, Farmer (Prefab), PlotUI (for transforms), Pozo (for transforms), Granja (for transforms)
// HandlesEvents: FarmingManager.OnCropAssigned, FarmingManager.OnCropHarvested
// TriggersEvents: None (directly assigns tasks to Farmers)
// UsesSO: None
// NeedsSetup: farmerPrefab, initialFarmerCount, farmerSpawnPoint, lists of wellTransforms and farmBuildingTransforms.
