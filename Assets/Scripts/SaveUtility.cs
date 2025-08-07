using UnityEngine;

namespace DayNightSystem
{
    [System.Serializable]
    public struct DayNightSaveData
    {
        public float currentTime;
        public bool isDay;
        public bool hasPenalty;
        public bool playerSleptCorrectly;
        
        public DayNightSaveData(float time, bool day, bool penalty, bool sleptCorrectly)
        {
            currentTime = time;
            isDay = day;
            hasPenalty = penalty;
            playerSleptCorrectly = sleptCorrectly;
        }
    }
    
    public static class SaveUtility
    {
        private const string TIME_KEY = "DayNight_Time";
        private const string IS_DAY_KEY = "DayNight_IsDay";
        private const string HAS_PENALTY_KEY = "DayNight_HasPenalty";
        private const string SLEPT_CORRECTLY_KEY = "DayNight_SleptCorrectly";
        
        public static void SaveDayNightState(float time, bool isDay, bool playerSleptCorrectly)
        {
            PlayerPrefs.SetFloat(TIME_KEY, time);
            PlayerPrefs.SetInt(IS_DAY_KEY, isDay ? 1 : 0);
            PlayerPrefs.SetInt(SLEPT_CORRECTLY_KEY, playerSleptCorrectly ? 1 : 0);
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveUtility] Saved Day/Night state - Time: {time:F1}s, IsDay: {isDay}, SleptCorrectly: {playerSleptCorrectly}");
        }
        
        public static DayNightSaveData LoadDayNightState()
        {
            // Check if save data exists
            if (!PlayerPrefs.HasKey(TIME_KEY))
            {
                Debug.Log("[SaveUtility] No save data found - using default values");
                return GetDefaultSaveData();
            }
            
            float time = PlayerPrefs.GetFloat(TIME_KEY, 0f);
            bool isDay = PlayerPrefs.GetInt(IS_DAY_KEY, 1) == 1;
            bool hasPenalty = PlayerPrefs.GetInt(HAS_PENALTY_KEY, 0) == 1;
            bool playerSleptCorrectly = PlayerPrefs.GetInt(SLEPT_CORRECTLY_KEY, 1) == 1; // Default to true for new saves
            
            var saveData = new DayNightSaveData(time, isDay, hasPenalty, playerSleptCorrectly);
            
            Debug.Log($"[SaveUtility] Loaded Day/Night state - Time: {time:F1}s, IsDay: {isDay}, HasPenalty: {hasPenalty}, SleptCorrectly: {playerSleptCorrectly}");
            
            return saveData;
        }
        
        public static DayNightSaveData GetDefaultSaveData()
        {
            return new DayNightSaveData(0f, true, false, true); // Default: start of day, no penalty, slept correctly
        }
        
        public static void ClearDayNightSaveData()
        {
            PlayerPrefs.DeleteKey(TIME_KEY);
            PlayerPrefs.DeleteKey(IS_DAY_KEY);
            PlayerPrefs.DeleteKey(HAS_PENALTY_KEY);
            PlayerPrefs.DeleteKey(SLEPT_CORRECTLY_KEY);
            PlayerPrefs.Save();
            
            Debug.Log("[SaveUtility] Cleared Day/Night save data");
        }
        
        public static bool HasSaveData()
        {
            return PlayerPrefs.HasKey(TIME_KEY);
        }
        
        public static void SavePlayerPenaltyState(bool hasPenalty)
        {
            PlayerPrefs.SetInt(HAS_PENALTY_KEY, hasPenalty ? 1 : 0);
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveUtility] Saved penalty state: {hasPenalty}");
        }
        
        public static bool LoadPlayerPenaltyState()
        {
            return PlayerPrefs.GetInt(HAS_PENALTY_KEY, 0) == 1;
        }
        
        public static void SaveTimeState(float time, bool isDay)
        {
            PlayerPrefs.SetFloat(TIME_KEY, time);
            PlayerPrefs.SetInt(IS_DAY_KEY, isDay ? 1 : 0);
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveUtility] Saved time state - Time: {time:F1}s, IsDay: {isDay}");
        }
        
        public static (float time, bool isDay) LoadTimeState()
        {
            float time = PlayerPrefs.GetFloat(TIME_KEY, 0f);
            bool isDay = PlayerPrefs.GetInt(IS_DAY_KEY, 1) == 1;
            
            return (time, isDay);
        }
        
        public static void SaveSleepState(bool sleptCorrectly)
        {
            PlayerPrefs.SetInt(SLEPT_CORRECTLY_KEY, sleptCorrectly ? 1 : 0);
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveUtility] Saved sleep state: {sleptCorrectly}");
        }
        
        public static bool LoadSleepState()
        {
            return PlayerPrefs.GetInt(SLEPT_CORRECTLY_KEY, 1) == 1; // Default to true for new saves
        }
        
        // New: Validate save data consistency
        public static bool ValidateSaveData()
        {
            if (!HasSaveData())
            {
                Debug.Log("[SaveUtility] No save data to validate");
                return false;
            }
            
            var saveData = LoadDayNightState();
            bool isValid = true;
            
            // Validate time is reasonable
            if (saveData.currentTime < 0f || saveData.currentTime > 86400f) // Max 24 hours
            {
                Debug.LogWarning($"[SaveUtility] Invalid time value: {saveData.currentTime}");
                isValid = false;
            }
            
            // Validate boolean values
            if (saveData.isDay != true && saveData.isDay != false)
            {
                Debug.LogWarning($"[SaveUtility] Invalid isDay value: {saveData.isDay}");
                isValid = false;
            }
            
            if (saveData.playerSleptCorrectly != true && saveData.playerSleptCorrectly != false)
            {
                Debug.LogWarning($"[SaveUtility] Invalid playerSleptCorrectly value: {saveData.playerSleptCorrectly}");
                isValid = false;
            }
            
            if (isValid)
            {
                Debug.Log("[SaveUtility] Save data validation passed");
            }
            else
            {
                Debug.LogWarning("[SaveUtility] Save data validation failed - using defaults");
            }
            
            return isValid;
        }
        
        // New: Get save data summary for debugging
        public static string GetSaveDataSummary()
        {
            if (!HasSaveData())
            {
                return "No save data found";
            }
            
            var saveData = LoadDayNightState();
            return $"Time: {saveData.currentTime:F1}s, IsDay: {saveData.isDay}, SleptCorrectly: {saveData.playerSleptCorrectly}, HasPenalty: {saveData.hasPenalty}";
        }
    }
}

// ScriptRole: Static utility for saving and loading day/night system state with validation and consistency checks
// RelatedScripts: DayNightManager, PlayerPenalty
// UsesSO: None
// ReceivesFrom: DayNightManager, PlayerPenalty
// SendsTo: PlayerPrefs
