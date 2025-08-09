using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Minimal settings menu controller. Only shows/hides the settings panel and can navigate back to a pause panel.
    /// No actual settings logic in this slice.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("Root of the Settings panel UI to show/hide.")]
        [SerializeField] private GameObject settingsPanelRoot;
        [Tooltip("Optional: Pause panel root to toggle when returning from Settings.")]
        [SerializeField] private GameObject pausePanelRoot;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
        }

        public void OpenSettings()
        {
            if (pausePanelRoot != null) pausePanelRoot.SetActive(false);
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(true);
            if (showDebugLogs) Debug.Log("[SettingsMenuController] Open Settings");
        }

        public void CloseSettings()
        {
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
            if (pausePanelRoot != null) pausePanelRoot.SetActive(true);
            if (showDebugLogs) Debug.Log("[SettingsMenuController] Close Settings");
        }

        // Convenience for a Back button
        public void GoBack()
        {
            CloseSettings();
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Control mínimo del panel de Settings (mostrar/ocultar, volver al Pause panel).
// RelatedScripts: Game.UI.PauseMenuController
// UsesSO: No
// ReceivesFrom: Botones UI
// SendsTo: GameObjects de UI (SetActive)
