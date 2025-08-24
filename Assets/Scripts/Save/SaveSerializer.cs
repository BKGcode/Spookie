using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Serialización JSON + hash CRC32 simple para integridad ligera.
    /// </summary>
    public static class SaveSerializer
    {
        private const string HASH_FIELD_PATTERN = "\"hash\""; // usado para limpieza ingenua

        public static string ToJson(SaveData data)
        {
            data.hash = null; // limpiar antes
            string raw = JsonUtility.ToJson(data, prettyPrint: false);
            // Remover campo hash vacio (JsonUtility no serializa null, así que no hace falta podar)
            string crc = ComputeCRC32(raw);
            data.hash = crc;
            string withHash = JsonUtility.ToJson(data, prettyPrint: false);
            return withHash;
        }

        public static bool TryFromJson(string json, out SaveData data)
        {
            data = null;
            if (string.IsNullOrEmpty(json)) return false;
            try
            {
                // Parse preliminar para extraer hash
                var tmp = JsonUtility.FromJson<SaveData>(json);
                if (tmp == null || string.IsNullOrEmpty(tmp.hash)) return false;
                string stored = tmp.hash;
                tmp.hash = null;
                string recalculated = JsonUtility.ToJson(tmp, false);
                string crc = ComputeCRC32(recalculated);
                if (!string.Equals(stored, crc, StringComparison.OrdinalIgnoreCase))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning("[SaveSerializer] Hash mismatch - posible corrupción");
#endif
                    return false;
                }
                tmp.hash = stored; // restaurar
                data = tmp;
                return true;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[SaveSerializer] Parse error: {ex.Message}");
#endif
                return false;
            }
        }

        // CRC32 (polinomio estándar 0xEDB88320)
        public static string ComputeCRC32(string input)
        {
            if (input == null) input = string.Empty;
            uint crc = 0xFFFFFFFFu;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < bytes.Length; i++)
            {
                uint b = bytes[i];
                crc ^= b;
                for (int k = 0; k < 8; k++)
                {
                    uint mask = (crc & 1u) != 0u ? 0xEDB88320u : 0u;
                    crc = (crc >> 1) ^ mask;
                }
            }
            crc ^= 0xFFFFFFFFu;
            return crc.ToString("X8");
        }
    }
}

// ScriptRole: Serialización + verificación de integridad de SaveData.
// RelatedScripts: SaveGameManager
// UsesSO: No
// ReceivesFrom: SaveGameManager
// SendsTo: SaveGameManager (strings JSON)
// Adjuntar: No aplica.