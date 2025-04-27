using UnityEngine;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Spookie/Farming/Crop")]
public class CropSO : ScriptableObject
{
    [Tooltip("Clave para buscar el nombre localizado en FeedbackMessagesSO")]
    [SerializeField] private string cropNameKey = "default_crop_name";

    [Tooltip("Icono que representa al cultivo")]
    [SerializeField] private Sprite icon;

    [Tooltip("Tiempo base de crecimiento en MINUTOS")]
    [SerializeField] private float baseGrowthTimeMinutes = 5.0f;

    [Tooltip("Recompensa en monedas al cosechar (se usará con el sistema de economía)")]
    [SerializeField] private int rewardValue = 1;

    [Tooltip("Coste en monedas para desbloquear este cultivo (si isUnlockedByDefault es false)")]
    [SerializeField] private int purchaseCost = 10;

    [Tooltip("¿Está este cultivo desbloqueado desde el inicio del juego?")]
    [SerializeField] private bool isUnlockedByDefault = false;

    // Propiedades públicas para acceder a los datos
    public string CropNameKey => cropNameKey;
    public Sprite Icon => icon;
    public float BaseGrowthTimeMinutes => baseGrowthTimeMinutes;
    public int RewardValue => rewardValue;
    public int PurchaseCost => purchaseCost;
    public bool IsUnlockedByDefault => isUnlockedByDefault;

    // --- Estado dinámico (gestionado por FarmingManager o similar) ---
    // No almacenar aquí si está desbloqueado o no en la partida actual,
    // eso debe gestionarlo un sistema central (ej. PlayerProgress o FarmingManager).
}

// ScriptRole: Define las propiedades estáticas de un tipo de cultivo.
// UsesSO: FeedbackMessagesSO (indirectamente, a través de cropNameKey) 