using UnityEngine;

public class VisualSystem : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer backgroundRenderer;
    public SpriteRenderer vehicleRenderer;

    private StageDataSO currentStage;
    private VehicleDataSO currentVehicle;

    public void SetStage(StageDataSO stage)
    {
        currentStage = stage;
        UpdateVisuals();
    }

    public void SetVehicle(VehicleDataSO vehicle)
    {
        currentVehicle = vehicle;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (currentStage != null)
        {
            backgroundRenderer.sprite = currentStage.backgroundSprite;
        }

        if (currentVehicle != null)
        {
            vehicleRenderer.sprite = currentVehicle.vehicleSprite;
        }
    }
}
