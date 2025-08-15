// Tools to quickly clear Day/Night penalties and reset PlayerPrefs during development.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class SpookieTools
    {
        private const string PP_DayKey = "DN_CurrentDay";
        private const string PP_PenaltyKey = "DN_NextDayPenalty";
        private const string PP_PenaltyUntilKey = "DN_NextDayPenalty_UntilSeconds";

    [MenuItem("Tools/Spookie/Dev/Clear DayNight Penalty")] 
        private static void ClearDayNightPenalty()
        {
            PlayerPrefs.SetInt(PP_PenaltyKey, 0);
            PlayerPrefs.DeleteKey(PP_PenaltyUntilKey);
            PlayerPrefs.Save();
            Debug.Log("[SpookieTools] Cleared DayNight penalty (sprint re-enabled next play).");
        }

    [MenuItem("Tools/Spookie/Dev/Reset DayNight State")] 
        private static void ResetDayNightState()
        {
            PlayerPrefs.DeleteKey(PP_DayKey);
            PlayerPrefs.SetInt(PP_PenaltyKey, 0);
            PlayerPrefs.DeleteKey(PP_PenaltyUntilKey);
            PlayerPrefs.Save();
            Debug.Log("[SpookieTools] Reset DayNight state and cleared penalty.");
        }

    [MenuItem("Tools/Spookie/Dev/Clear All Spookie PlayerPrefs")]
        private static void ClearAllSpookiePlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog("Clear All PlayerPrefs?",
                "This will call PlayerPrefs.DeleteAll() for this project (Company/Product). Continue?", "Yes", "No"))
            {
                return;
            }
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[SpookieTools] PlayerPrefs.DeleteAll() executed.");
        }
    }
}
#endif
