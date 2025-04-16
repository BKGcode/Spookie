using UnityEngine;

[CreateAssetMenu(fileName = "VehicleData", menuName = "GameData/VehicleData")]
public class VehicleDataSO : ScriptableObject
{
    [Header("Vehicle Info")]
    public string vehicleName;
    public int vehicleID;

    [Header("Visuals")]
    public Sprite vehicleSprite;

    [Header("Unlock Info")]
    public int requiredLevel; // Nivel necesario para desbloquear
}
