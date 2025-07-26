using UnityEngine;

public class DwarfStats : MonoBehaviour
{
    [SerializeField] private DwarfDataSO baseStats;
    [SerializeField] private GameSettingsSO gameSettings;

    // Instance-specific data
    public string dwarfName { get; private set; }
    public Sprite dwarfIcon { get; private set; }
    public int age { get; private set; }
    public string currentStatus { get; private set; } = "Initializing";

    public float CurrentStamina { get; private set; }
    
    // Not used in MVP, but good to have for the future
    public int CurrentHealth { get; private set; } 

    public DwarfDataSO BaseStats => baseStats;

    private void Awake()
    {
        // Initialization is now handled by an external spawner/manager
    }

    public void Initialize()
    {
        if (baseStats == null)
        {
            Debug.LogError("BaseStats (DwarfDataSO) is not assigned!");
            return;
        }

        // Procedural generation of identity
        if (baseStats.possibleNames.Count > 0)
            dwarfName = baseStats.possibleNames[Random.Range(0, baseStats.possibleNames.Count)];
        else
            dwarfName = "Dwarf";

        if (baseStats.possibleIcons.Count > 0)
            dwarfIcon = baseStats.possibleIcons[Random.Range(0, baseStats.possibleIcons.Count)];

        age = Random.Range(20, 150);
        CurrentStamina = baseStats.maxStamina;

        Debug.Log($"Dwarf '{dwarfName}' (age {age}) created. Max Stamina: {CurrentStamina}");
    }

    public void UpdateStatus(string newStatus)
    {
        currentStatus = newStatus;
    }

    public void ConsumeStamina(float deltaTime)
    {
        if (gameSettings == null) return;
        CurrentStamina -= gameSettings.staminaConsumptionRate * deltaTime;
        if (CurrentStamina < 0) CurrentStamina = 0;
    }

    public void RecoverStamina(float deltaTime)
    {
        if (gameSettings == null) return;
        CurrentStamina += gameSettings.staminaRecoveryRate * deltaTime;
        if (CurrentStamina > baseStats.maxStamina) CurrentStamina = baseStats.maxStamina;
    }
}

// ScriptRole: Manages the runtime stats of a dwarf, like stamina.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: DwarfDataSO, GameSettingsSO
// NeedsSetup: Assign the DwarfDataSO asset defining this dwarf's base stats and the global GameSettingsSO. 