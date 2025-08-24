using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Guarda y restaura posición + yaw del jugador. Yaw tomado del transform local Y.
    /// </summary>
    [AddComponentMenu("Spookie/Save/Participant Player")]
    public class SaveParticipantPlayer : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private Transform playerRoot; // raíz física
        [SerializeField] private bool showDebugLogs = false;

        private void Reset()
        {
            if (playerRoot == null) playerRoot = transform;
        }

        private void OnEnable()
        {
            SaveGameManager.Instance?.Register(this);
        }

        private void OnDisable()
        {
            SaveGameManager.Instance?.Unregister(this);
        }

        public void WriteTo(SaveData data)
        {
            if (data == null || playerRoot == null) return;
            data.playerPosition = playerRoot.position;
            data.playerYaw = playerRoot.eulerAngles.y;
        }

        public void ReadFrom(SaveData data)
        {
            if (data == null || playerRoot == null) return;
            // Restaurar sólo si misma escena (SaveGameManager ya se ocupa de cargar escena adecuada)
            playerRoot.position = data.playerPosition;
            var euler = playerRoot.eulerAngles;
            euler.y = data.playerYaw;
            playerRoot.rotation = Quaternion.Euler(euler);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showDebugLogs) Debug.Log("[SaveParticipantPlayer] Pos restored");
#endif
        }
    }
}

// ScriptRole: Persistencia de posición y orientación del jugador.
// RelatedScripts: SaveGameManager
// UsesSO: No
// ReceivesFrom: SaveGameManager (callbacks)
// SendsTo: SaveData (playerPosition, playerYaw)
// Adjuntar: Al GameObject raíz del jugador.