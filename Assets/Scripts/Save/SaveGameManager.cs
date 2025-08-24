using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Scenes;

namespace Game.Save
{
    /// <summary>
    /// Punto central de guardado/carga. Mantiene SaveData en memoria, autosave, rotación de backups y transición de día/escena.
    /// KISS: un único componente DontDestroyOnLoad.
    /// </summary>
    [AddComponentMenu("Spookie/Save/Save Game Manager")] 
    public class SaveGameManager : MonoBehaviour
    {
        public static SaveGameManager Instance { get; private set; }

        [Header("Config")]
        [Tooltip("Versión de estructura de SaveData. Cambiar al modificar campos.")]
        [SerializeField] private int saveVersion = 1;
        [Tooltip("Intervalo de autosave en minutos.")]
        [Min(1f)] [SerializeField] private float autosaveIntervalMinutes = 5f;
        [Tooltip("Secuencia de capítulos (escenas por día). Opcional; si null se mantiene escena actual.")]
        [SerializeField] private DayChapterSequenceSO daySequence;
        [Tooltip("Guardar al salir (OnApplicationQuit/Pause).")]
        [SerializeField] private bool saveOnExit = true;
        [Tooltip("Número de backups rotativos.")]
        [Range(0,2)] [SerializeField] private int backupCount = 2; // 0=solo primary, 1= +bkp1, 2= +bkp2

        [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        public SaveData CurrentData { get; private set; }
        public int CurrentSlot => CurrentData?.slotId ?? -1;
        public bool HasLoaded => CurrentData != null;

        private readonly List<ISaveParticipant> _participants = new List<ISaveParticipant>();
        private float _autosaveTimer;
        private bool _paused;
        private double _sessionPlaySeconds; // acumulado desde último save

        // Eventos
        public event Action<SaveData> OnAfterLoad;
        public event Action<SaveData> OnBeforeSave;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!HasLoaded) return;
            if (!_paused)
            {
                _sessionPlaySeconds += Time.unscaledDeltaTime;
            }
            _autosaveTimer += Time.unscaledDeltaTime;
            if (_autosaveTimer >= autosaveIntervalMinutes * 60f)
            {
                _autosaveTimer = 0f;
                AutoSave();
            }
        }

        public void Register(ISaveParticipant participant)
        {
            if (participant == null || _participants.Contains(participant)) return;
            _participants.Add(participant);
            _participants.Sort((a,b)=> a.Order.CompareTo(b.Order));
            if (HasLoaded) // si ya hay datos, sincronizar inmediatamente
            {
                try { participant.ReadFrom(CurrentData); } catch (Exception ex) { Log($"Participant Read error: {ex.Message}"); }
            }
        }

        public void Unregister(ISaveParticipant participant)
        {
            _participants.Remove(participant);
        }

        public void NewGame(int slotId, string initialScene, int startDay = 1, float dayLengthSeconds = 300f)
        {
            CurrentData = new SaveData
            {
                version = saveVersion,
                slotId = slotId,
                absoluteDay = Mathf.Max(1, startDay),
                sceneName = string.IsNullOrEmpty(initialScene) ? SceneManager.GetActiveScene().name : initialScene,
                timeOfDaySeconds = 0f,
                dayLengthSeconds = dayLengthSeconds,
                dayState = "Day",
                miningSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue),
                lastRealWorldSaveUtc = DateTime.UtcNow.ToString("o"),
                lastSaveKind = "New"
            };
            _sessionPlaySeconds = 0;
            _autosaveTimer = 0;
            NotifyParticipantsRead();
            SaveInternal("New");
        }

        public bool LoadSlot(int slotId)
        {
            var primary = ReadFile(slotId, 0);
            if (primary != null)
            {
                CurrentData = primary;
                PostLoad();
                return true;
            }
            // Fallback backups
            for (int i = 1; i <= backupCount; i++)
            {
                var b = ReadFile(slotId, i);
                if (b != null)
                {
                    CurrentData = b;
                    PostLoad();
                    return true;
                }
            }
            return false;
        }

        private void PostLoad()
        {
            if (CurrentData.version != saveVersion)
            {
                // Migraciones futuras: por ahora solo log
                Log($"Version mismatch save({CurrentData.version}) != manager({saveVersion})");
            }
            _sessionPlaySeconds = 0;
            _autosaveTimer = 0;
            NotifyParticipantsRead();
            OnAfterLoad?.Invoke(CurrentData);
        }

        public void SaveManual() => SaveInternal("Manual");
        public void QuickSave() => SaveInternal("Quick");
        private void AutoSave() => SaveInternal("Auto");
        private void SaveExit() => SaveInternal("Exit");

        private void SaveInternal(string kind)
        {
            if (!HasLoaded) return;
            if (CurrentData.isGameCompleted && kind != "Exit") return; // congelar estado final
            CurrentData.version = saveVersion;
            CurrentData.lastSaveKind = kind;
            CurrentData.lastRealWorldSaveUtc = DateTime.UtcNow.ToString("o");
            CurrentData.cumulativePlaySeconds += _sessionPlaySeconds;
            _sessionPlaySeconds = 0;

            OnBeforeSave?.Invoke(CurrentData);
            NotifyParticipantsWrite();

            string json = SaveSerializer.ToJson(CurrentData);
            RotateAndWrite(json, CurrentData.slotId);
            Log($"Saved slot {CurrentData.slotId} ({kind})");
        }

        public void DeleteSlot(int slotId)
        {
            string dir = GetDir();
            FileDeleteIfExists(Path.Combine(dir, FileName(slotId, 0)));
            for (int i = 1; i <= backupCount; i++)
                FileDeleteIfExists(Path.Combine(dir, FileName(slotId, i)));
        }

        private void RotateAndWrite(string primaryJson, int slotId)
        {
            string dir = GetDir();
            Directory.CreateDirectory(dir);
            // Rotación: bkp2 <- bkp1 <- primary
            if (backupCount >= 2)
            {
                SafeMove(FileName(slotId,1), FileName(slotId,2));
            }
            if (backupCount >= 1)
            {
                SafeMove(FileName(slotId,0), FileName(slotId,1));
            }
            // Escribir primary
            File.WriteAllText(Path.Combine(dir, FileName(slotId,0)), primaryJson);
        }

        private void SafeMove(string fromName, string toName)
        {
            string dir = GetDir();
            string from = Path.Combine(dir, fromName);
            string to = Path.Combine(dir, toName);
            if (File.Exists(from))
            {
                try { File.Copy(from, to, overwrite:true); } catch { }
            }
        }

        private SaveData ReadFile(int slotId, int index)
        {
            try
            {
                string path = Path.Combine(GetDir(), FileName(slotId, index));
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                if (SaveSerializer.TryFromJson(json, out var data))
                {
                    if (data.slotId != slotId) data.slotId = slotId; // robustez
                    return data;
                }
            }
            catch (Exception ex) { Log($"Read error slot {slotId} idx {index}: {ex.Message}"); }
            return null;
        }

        private void NotifyParticipantsWrite()
        {
            foreach (var p in _participants)
            {
                try { p.WriteTo(CurrentData); } catch (Exception ex) { Log($"Write participant error: {ex.Message}"); }
            }
        }
        private void NotifyParticipantsRead()
        {
            foreach (var p in _participants)
            {
                try { p.ReadFrom(CurrentData); } catch (Exception ex) { Log($"Read participant error: {ex.Message}"); }
            }
        }

        private string GetDir() => Path.Combine(Application.persistentDataPath, "saves");
        private string FileName(int slotId, int index) => $"save_slot{slotId}_{(index==0?"primary":$"bkp{index}")}.json";
        private void FileDeleteIfExists(string path) { if (File.Exists(path)) { try { File.Delete(path); } catch { } } }

        public void MarkPaused(bool paused) => _paused = paused;

        private void OnApplicationQuit() { if (saveOnExit) SaveExit(); }
        private void OnApplicationPause(bool pauseStatus) { if (pauseStatus && saveOnExit) SaveExit(); }

        private void Log(string msg)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.Log($"[SaveGameManager] {msg}");
#endif
        }

        // Resolución de escena para día (API simple de consulta externa)
        public string GetSceneForDay(int day)
        {
            if (daySequence == null) return CurrentData?.sceneName;
            var rr = daySequence.ResolveSceneForDay(day);
            return string.IsNullOrEmpty(rr.sceneName) ? CurrentData?.sceneName : rr.sceneName;
        }
    }
}

// ScriptRole: Gestor central de guardado/carga, autosave y rotación de backups.
// RelatedScripts: SaveData, SaveSerializer, DayChapterSequenceSO
// UsesSO: DayChapterSequenceSO (opcional)
// ReceivesFrom: Sistemas de juego (registro participantes), DayNightManager (flujos)
// SendsTo: Disco, participantes (callbacks)
// Adjuntar: GameObject raíz persistente (_Systems). Asignar Sequence SO y tuning autosave.