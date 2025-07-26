using UnityEngine;

public class DwarfStats : MonoBehaviour
{
    [SerializeField] private DwarfDataSO baseStats;
    [SerializeField] private GameSettingsSO gameSettings;

    public float CurrentStamina { get; private set; }
    
    // Not used in MVP, but good to have for the future
    public int CurrentHealth { get; private set; } 

    public DwarfDataSO BaseStats => baseStats;

    private void Awake()
    {
        InitializeStats();
        Debug.Log($"Dwarf '{baseStats.dwarfName}' stats initialized. Max Stamina: {CurrentStamina}");
    }

    public void InitializeStats()
    {
        if (baseStats == null)
        {
            Debug.LogError("BaseStats (DwarfDataSO) is not assigned!");
            return;
        }
        CurrentStamina = baseStats.maxStamina;
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