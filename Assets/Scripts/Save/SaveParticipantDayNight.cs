using UnityEngine;
using Game.DayNight;

namespace Game.Save
{
    /// <summary>
    /// Delegado de guardado para el DayNightManager. Escribe estado de día, tiempo transcurrido y penalización.
    /// La restauración se hace dentro de DayNightManager.Start (FillSaveData solo necesita escritura).
    /// </summary>
    [AddComponentMenu("Spookie/Save/Participant DayNight")] 
    public class SaveParticipantDayNight : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private DayNightManager dayNightManager;
        private void Reset()
        {
            if (dayNightManager == null) dayNightManager = FindObjectOfType<DayNightManager>();
        }
        private void OnEnable() => SaveGameManager.Instance?.Register(this);
        private void OnDisable() => SaveGameManager.Instance?.Unregister(this);

        public void WriteTo(SaveData data)
        {
            if (data == null || dayNightManager == null) return;
            dayNightManager.FillSaveData(data);
        }

        public void ReadFrom(SaveData data)
        {
            // Restauración la hace DayNightManager en Start tras InitializeDay.
        }
    }
}

// ScriptRole: Persistencia del estado del ciclo día/noche.
// RelatedScripts: DayNightManager, SaveGameManager
// UsesSO: No
// ReceivesFrom: SaveGameManager (write callbacks)
// SendsTo: SaveData (día, tiempos, penalización)
// Adjuntar: GameObject con DayNightManager o _Systems (referencia por Inspector).