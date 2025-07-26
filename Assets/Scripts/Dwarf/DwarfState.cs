using UnityEngine;

public class DwarfState
{
    public DwarfDataSO BaseStats { get; private set; }
    public string DwarfName { get; private set; }
    public Sprite DwarfIcon { get; private set; }
    public float CurrentStamina { get; set; }
    public string CurrentStatus { get; set; }
    public int Age { get; private set; }

    public DwarfState(DwarfDataSO baseStats)
    {
        BaseStats = baseStats;
        DwarfName = baseStats.possibleNames[Random.Range(0, baseStats.possibleNames.Count)];
        DwarfIcon = baseStats.possibleIcons[Random.Range(0, baseStats.possibleIcons.Count)];
        CurrentStamina = baseStats.maxStamina;
        CurrentStatus = "Idle"; 
        Age = Random.Range(25, 150);
    }
}

// ScriptRole: Holds the runtime state of an individual dwarf instance.
// Dependencies: DwarfDataSO
// NeedsSetup: None, this is a plain C# class. 