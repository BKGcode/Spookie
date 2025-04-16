using UnityEngine;

public class DataPersistenceSystem : MonoBehaviour
{
    private const string XPKey = "XP";
    private const string LevelKey = "Level";
    private const string CurrencyKey = "Currency";
    private const string SelectedStageKey = "SelectedStage";
    private const string SelectedVehicleKey = "SelectedVehicle";

    public CustomizationSystem customizationSystem;

    private GamificationSystem gamificationSystem;

    private void Start()
    {
        gamificationSystem = FindObjectOfType<GamificationSystem>();

        LoadProgress();
        LoadCustomization();
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(XPKey, gamificationSystem.currentXP);
        PlayerPrefs.SetInt(LevelKey, gamificationSystem.currentLevel);
        PlayerPrefs.SetInt(CurrencyKey, gamificationSystem.currentCurrency);
        PlayerPrefs.Save();

        Debug.Log("Player progress saved.");
    }

    public void LoadProgress()
    {
        gamificationSystem.currentXP = PlayerPrefs.GetInt(XPKey, 0);
        gamificationSystem.currentLevel = PlayerPrefs.GetInt(LevelKey, 1);
        gamificationSystem.currentCurrency = PlayerPrefs.GetInt(CurrencyKey, 0);

        Debug.Log("Player progress loaded.");
    }

    public void SaveCustomization(int stageID, int vehicleID)
    {
        PlayerPrefs.SetInt(SelectedStageKey, stageID);
        PlayerPrefs.SetInt(SelectedVehicleKey, vehicleID);
        PlayerPrefs.Save();
        Debug.Log("Customization saved.");
    }

    public void LoadCustomization()
    {
        int stageID = PlayerPrefs.GetInt(SelectedStageKey, 0);
        int vehicleID = PlayerPrefs.GetInt(SelectedVehicleKey, 0);

        customizationSystem.SelectStage(stageID);
        customizationSystem.SelectVehicle(vehicleID);
        Debug.Log("Customization loaded.");
    }
}
