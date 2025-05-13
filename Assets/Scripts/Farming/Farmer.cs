using UnityEngine;
using System.Collections; // Necesario para Coroutines

public enum FarmerState
{
    Idle,
    MovingToTarget, // Moviéndose a la parcela o a un objeto secundario (pozo/granja)
    PerformingTask // Realizando la animación de la tarea en el sitio
}

public class Farmer : MonoBehaviour
{
    public int FarmerId { get; private set; }
    public FarmerState CurrentState { get; private set; }
    private FarmerTask currentTask;
    private Transform visualTarget; // Pozo, Granja, o la propia Parcela (necesitaremos la posición de la parcela)
    private FarmerManager farmerManager; // Referencia al manager

    // TODO: Añadir velocidad de movimiento, referencias a Animator, etc.
    [SerializeField] private float moveSpeed = 2f;

    public void Initialize(int id, FarmerManager manager)
    {
        FarmerId = id;
        farmerManager = manager; // Guardar referencia
        CurrentState = FarmerState.Idle;
        gameObject.name = $"Farmer_{FarmerId}";
        Debug.Log($"{gameObject.name}: Inicializado y en estado Idle.");
    }

    public bool IsIdle()
    {
        return CurrentState == FarmerState.Idle;
    }

    public void AssignTask(FarmerTask task, Transform plotTransform, Transform secondaryTargetTransform = null)
    {
        if (CurrentState != FarmerState.Idle)
        {
            Debug.LogWarning($"{gameObject.name}: Intento de asignar tarea pero no estoy Idle. Estado actual: {CurrentState}");
            return;
        }

        currentTask = task;
        Debug.Log($"{gameObject.name}: Tarea asignada: {currentTask.TaskType} para Plot ID {currentTask.TargetPlot.plotId}");

        // Lógica de movimiento y ejecución de tarea
        StartCoroutine(PerformTaskRoutine(plotTransform, secondaryTargetTransform));
    }

    private IEnumerator PerformTaskRoutine(Transform plotTransform, Transform secondaryTargetTransform)
    {
        CurrentState = FarmerState.MovingToTarget;
        FarmerTask taskBeingPerformed = currentTask; // Copiar la referencia por si currentTask cambia

        // Fase 1: Moverse al primer destino (Pozo si es Regar, Parcela si es Plantar/Cosechar directo)
        Transform firstDestination = null;
        string firstDestName = "";

        if (currentTask.TaskType == FarmerTaskType.Water && secondaryTargetTransform != null) // secondaryTargetTransform sería el Pozo
        {
            firstDestination = secondaryTargetTransform;
            firstDestName = $"Pozo ({secondaryTargetTransform.name})";
        }
        else // Para Plantar y Cosechar (primera fase), o Regar sin pozo definido (directo a parcela)
        { 
            firstDestination = plotTransform;
            firstDestName = $"Parcela ({plotTransform.name})";
        }
        
        Debug.Log($"{gameObject.name} ({currentTask.TaskType}): Moviéndome a {firstDestName} en {firstDestination.position}");
        yield return StartCoroutine(MoveTo(firstDestination.position));
        Debug.Log($"{gameObject.name} ({currentTask.TaskType}): Llegué a {firstDestName}.");

        // Fase 1.5: Realizar acción en el primer destino si es necesario (ej. recoger agua del pozo)
        CurrentState = FarmerState.PerformingTask;
        if (currentTask.TaskType == FarmerTaskType.Water && secondaryTargetTransform != null)
        {
            Debug.Log($"{gameObject.name} ({currentTask.TaskType}): Recogiendo agua del {firstDestName} (simulado).");
            yield return new WaitForSeconds(1f); // Simular tiempo de acción
        }

        // Fase 2: Moverse al segundo destino (Parcela si venimos del Pozo, Granja si es Cosechar)
        if ((currentTask.TaskType == FarmerTaskType.Water && secondaryTargetTransform != null) || 
            (currentTask.TaskType == FarmerTaskType.Harvest && secondaryTargetTransform != null)) // secondaryTargetTransform sería la Granja para Cosecha
        {
            Transform secondDestination = (currentTask.TaskType == FarmerTaskType.Water) ? plotTransform : secondaryTargetTransform;
            string secondDestName = (currentTask.TaskType == FarmerTaskType.Water) ? $"Parcela ({plotTransform.name})" : $"Granja ({secondaryTargetTransform.name})";

            CurrentState = FarmerState.MovingToTarget;
            Debug.Log($"{gameObject.name} ({currentTask.TaskType}): Moviéndome a {secondDestName} en {secondDestination.position}");
            yield return StartCoroutine(MoveTo(secondDestination.position));
            Debug.Log($"{gameObject.name} ({currentTask.TaskType}): Llegué a {secondDestName}.");
        }

        // Fase 3: Realizar la acción principal en la parcela (Plantar, Regar-efectivo) o en la Granja (Entregar cosecha)
        CurrentState = FarmerState.PerformingTask;
        Debug.Log($"{gameObject.name} ({currentTask.TaskType}): Realizando tarea principal en {(currentTask.TaskType == FarmerTaskType.Harvest && secondaryTargetTransform != null ? secondaryTargetTransform.name : plotTransform.name)} (simulado).");
        // Aquí irían las animaciones específicas de la tarea
        yield return new WaitForSeconds(1.5f); // Simular tiempo de acción

        Debug.Log($"{gameObject.name}: Tarea {taskBeingPerformed.TaskType} para Plot ID {taskBeingPerformed.TargetPlot.plotId} completada.");
        
        // Notificar al FarmerManager ANTES de limpiar el estado del granjero
        if (farmerManager != null)
        {
            farmerManager.NotifyTaskCompleted(taskBeingPerformed.TargetPlot.plotId, taskBeingPerformed.TaskType);
        }

        CurrentState = FarmerState.Idle;
        this.currentTask = null; // Usar this.currentTask para evitar ambigüedad
        visualTarget = null;
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        // Simulación de movimiento. En un juego real, usaríamos NavMeshAgent o similar.
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            // Simular rotación hacia el objetivo
            if (targetPosition - transform.position != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
            }
            yield return null;
        }
        transform.position = targetPosition; // Asegurar posición exacta al final
    }
}

// ScriptRole: Represents an individual farmer unit, capable of performing tasks like planting, watering, and harvesting.
// Dependencies: None (movement/animation systems would be external or integrated later)
// HandlesEvents: None (receives tasks from FarmerManager)
// TriggersEvents: None (could trigger an event OnTaskCompleted for FarmerManager)
// UsesSO: None directly
// NeedsSetup: Prefab with this script. Initialized by FarmerManager. Needs references to Plot Transforms for movement. 