using UnityEngine;

public class CustomizationSystem : MonoBehaviour
{
    public VisualSystem visualSystem;

    public StageDataSO[] availableStages;
    public VehicleDataSO[] availableVehicles;

    private StageDataSO selectedStage;
    private VehicleDataSO selectedVehicle;

    public void SelectStage(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < availableStages.Length)
        {
            selectedStage = availableStages[stageIndex];
            visualSystem.SetStage(selectedStage);
            Debug.Log($"Stage selected: {selectedStage.stageName}");
        }
    }

    public void SelectVehicle(int vehicleIndex)
    {
        if (vehicleIndex >= 0 && vehicleIndex < availableVehicles.Length)
        {
            selectedVehicle = availableVehicles[vehicleIndex];
            visualSystem.SetVehicle(selectedVehicle);
            Debug.Log($"Vehicle selected: {selectedVehicle.vehicleName}");
        }
    }
}
