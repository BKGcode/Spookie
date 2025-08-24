using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Datos persistentes por slot. KISS: campos públicos serializados en JSON.
    /// Versionar al añadir/quitar campos. Campos opcionales conservan compatibilidad.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version;
        public int slotId;
        public bool isGameCompleted;

        // Día / escena
        public int absoluteDay = 1; // Día 1-based
        public string sceneName;
        public float timeOfDaySeconds; // segundos transcurridos del día actual (o restante según convención; decidimos transcurridos: 0 = amanecer)
        public float dayLengthSeconds; // guardado para compatibilidad si cambia config
        public string dayState; // Day, DuskWarning, Exhaustion, Sleep, Faint, Night (solo para robustez)

        // Jugador
        public Vector3 playerPosition;
        public float playerYaw;

        // Penalización post-faint
        public bool penaltyActive;
        public float penaltyRemainingSeconds;

        // Progresión mensajes / interacciones
        public List<string> seenMessageIds = new List<string>();

        // Minería / mundo (IDs deterministas)
        public int miningSeed; // global por partida
        public List<string> openedSectionIds = new List<string>();
        public List<string> minedLayerIds = new List<string>();

        // Flags genéricos (enteros) para puzzles / estados (clave -> valor int)
        public Dictionary<string, int> flags = new Dictionary<string, int>();

        // Métricas / metadata
        public double cumulativePlaySeconds;
        public string lastRealWorldSaveUtc; // ISO8601
        public string lastSaveKind; // Auto / Quick / Exit / Manual

        // Integridad
        public string hash; // CRC32 base64/hex del JSON sin este campo
    }
}

// ScriptRole: Modelo de datos persistentes por slot de guardado.
// RelatedScripts: SaveGameManager, SaveSerializer
// UsesSO: No
// ReceivesFrom: Sistemas de juego (participantes de guardado)
// SendsTo: Disco (JSON)
// Adjuntar: No aplica (clase serializable).