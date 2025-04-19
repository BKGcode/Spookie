using UnityEngine;

/// <summary>
/// Holds all UI visible texts for easy localization and editing.
/// </summary>
[CreateAssetMenu(fileName = "UITextSO", menuName = "Configs/UITextSO")]
public class UITextSO : ScriptableObject
{
    [Header("Timer Labels")]
    public string workLabel = "Trabajo";
    public string breakLabel = "Descanso";
    public string pausedLabel = "Pausado";
    public string stoppedLabel = "Detenido";
    public string startWorkLabel = "Iniciar Trabajo";
    public string startBreakLabel = "Iniciar Descanso";
    public string pauseLabel = "Pausar";
    public string resumeLabel = "Reanudar";
    public string stopLabel = "Detener";
    public string timeLeftLabel = "Tiempo Restante:";
}
