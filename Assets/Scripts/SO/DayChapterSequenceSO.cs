using System.Collections.Generic;
using UnityEngine;

namespace Game.Scenes
{
    /// <summary>
    /// Secuencia de capítulos/días: cada entrada define una escena y cuántos días consecutivos dura.
    /// Permite que una misma escena abarque varios días. La última entrada puede marcar fin del juego.
    /// ResolveSceneForDay realiza búsqueda lineal (listas pequeñas). Si el día excede la secuencia: retorna última.
    /// KISS: no cache expandido; si escala, se podría precomputar array día->índice.
    /// </summary>
    [CreateAssetMenu(menuName = "Spookie/Scenes/Day Chapter Sequence", fileName = "DayChapterSequence")]
    public class DayChapterSequenceSO : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            [Tooltip("Nombre exacto de la escena (Build Settings).")]
            public string sceneName;
            [Tooltip("Número de días que esta escena permanece activa de forma consecutiva (>=1).")]
            [Min(1)] public int dayCount = 1;
            [Tooltip("Marca que este capítulo termina la partida al finalizar su último día.")]
            public bool endGameOnLastDay;
            [Tooltip("Notas opcionales para diseño.")]
            [TextArea] public string notes;
        }

        [Header("Sequence (orden cronológico)")]
        public List<Entry> entries = new List<Entry>();

        /// <summary>Resultado de resolución de escena para un día dado.</summary>
        public struct ResolveResult
        {
            public string sceneName;
            public int chapterIndex;
            public bool isFinalChapterLastDay; // true si este día es el último día del capítulo marcado endGameOnLastDay
            public int firstDayOfChapter;
            public int lastDayOfChapter;
        }

        /// <summary>Devuelve la escena para un día (1-based). Si day &lt; 1, trata como 1.</summary>
        public ResolveResult ResolveSceneForDay(int day)
        {
            ResolveResult rr = new ResolveResult
            {
                sceneName = null,
                chapterIndex = -1,
                isFinalChapterLastDay = false,
                firstDayOfChapter = 1,
                lastDayOfChapter = 1
            };

            if (entries == null || entries.Count == 0)
                return rr;

            int d = Mathf.Max(1, day);
            int cursor = 1;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                int first = cursor;
                int last = cursor + Mathf.Max(1, e.dayCount) - 1;
                if (d >= first && d <= last)
                {
                    rr.sceneName = e.sceneName;
                    rr.chapterIndex = i;
                    rr.firstDayOfChapter = first;
                    rr.lastDayOfChapter = last;
                    rr.isFinalChapterLastDay = (e.endGameOnLastDay && d == last);
                    return rr;
                }
                cursor = last + 1;
            }

            // Si día excede rango: devolver última entrada
            var lastEntry = entries[entries.Count - 1];
            int totalLastFirst = cursor - Mathf.Max(1, lastEntry.dayCount); // aproximado, no crítico
            rr.sceneName = lastEntry.sceneName;
            rr.chapterIndex = entries.Count - 1;
            rr.firstDayOfChapter = totalLastFirst;
            rr.lastDayOfChapter = cursor - 1;
            rr.isFinalChapterLastDay = lastEntry.endGameOnLastDay; // fuera de rango: tratar como completado
            return rr;
        }

        private void OnValidate()
        {
            // Sanitizar
            if (entries != null)
            {
                bool endGameSeen = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].dayCount < 1) entries[i].dayCount = 1;
                    if (entries[i].endGameOnLastDay)
                    {
                        if (endGameSeen)
                        {
                            // Solo permitir un capítulo final marcado; si hay más de uno, limpiar banderas siguientes.
                            entries[i].endGameOnLastDay = false;
#if UNITY_EDITOR
                            Debug.LogWarning($"[DayChapterSequenceSO] Se encontró más de un endGameOnLastDay. Se desactiva en índice {i}.");
#endif
                        }
                        else endGameSeen = true;
                    }
                }
            }
        }
    }
}

// ScriptRole: Define la secuencia de capítulos (escenas) y cuántos días dura cada uno.
// RelatedScripts: SceneFlowService, SaveGameManager
// UsesSO: Sí (este mismo asset)
// ReceivesFrom: SaveGameManager / DayNightManager (consulta)
// SendsTo: — (solo datos)
// Adjuntar: Asset en carpeta SO_Assets. Configurar entries en Inspector.