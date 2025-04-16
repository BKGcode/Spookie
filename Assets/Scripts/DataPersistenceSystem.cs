using UnityEngine;

public class DataPersistenceSystem : MonoBehaviour
{
    private const string XPKey = "XP";
    private const string LevelKey = "Level";
    private const string CurrencyKey = "Currency";

    private GamificationSystem gamificationSystem;

    private void Start()
    {
        gamificationSystem = FindObjectOfType<GamificationSystem>();

        LoadProgress();
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
}
