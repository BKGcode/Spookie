using UnityEngine;

namespace GameStateController
{
    public class PlayerInputGate : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController.PlayerMovement playerMovement;
        [SerializeField] private PlayerController.PlayerInteraction playerInteraction;
        [SerializeField] private PlayerController.MouseLook mouseLook;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            // Allow assignment from Inspector; fallback to scene lookup for convenience
            if (playerMovement == null) { playerMovement = FindObjectOfType<PlayerController.PlayerMovement>(); }
            if (playerInteraction == null) { playerInteraction = FindObjectOfType<PlayerController.PlayerInteraction>(); }
            if (mouseLook == null) { mouseLook = FindObjectOfType<PlayerController.MouseLook>(); }

            if (playerMovement == null) { Debug.LogWarning("[PlayerInputGate] PlayerMovement not found"); }
            if (playerInteraction == null) { Debug.LogWarning("[PlayerInputGate] PlayerInteraction not found"); }
            if (mouseLook == null) { Debug.LogWarning("[PlayerInputGate] MouseLook not found"); }

            if (showDebugLogs)
            {
                Debug.Log("[PlayerInputGate] Initialized");
            }
        }

        public void DisableInput()
        {
            if (playerMovement != null) { playerMovement.enabled = false; }
            if (playerInteraction != null) { playerInteraction.enabled = false; }
            if (mouseLook != null) { mouseLook.enabled = false; }

            if (showDebugLogs) { Debug.Log("[PlayerInputGate] Player input disabled"); }
        }

        public void EnableInput()
        {
            if (playerMovement != null) { playerMovement.enabled = true; }
            if (playerInteraction != null) { playerInteraction.enabled = true; }
            if (mouseLook != null) { mouseLook.enabled = true; }

            if (showDebugLogs) { Debug.Log("[PlayerInputGate] Player input enabled"); }
        }

        [ContextMenu("Disable Input (Test)")]
        private void CtxDisableInput() => DisableInput();

        [ContextMenu("Enable Input (Test)")]
        private void CtxEnableInput() => EnableInput();
    }
}

// ScriptRole: Centralizes enabling/disabling of player input components
// RelatedScripts: PlayerMovement, PlayerInteraction, MouseLook, GameStateManager
// UsesSO: None
// ReceivesFrom: GameStateManager
// SendsTo: Player components (enable/disable)


