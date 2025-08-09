using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public enum PauseReason
    {
        UI = 0,
        Oniric = 1,
        Transition = 2,
        Cutscene = 3,
        System = 4,
    }

    /// <summary>
    /// Minimal, idempotent pause manager. Disables player control and Day/Night while paused.
    /// KISS: no Time.timeScale changes in this slice. Inspector-first wiring.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseManager : MonoBehaviour
    {
        [Header("Targets (assign in Inspector)")]
        [Tooltip("Player movement component to disable while paused.")]
        [SerializeField] private PlayerController.PlayerMovement playerMovement;
        [Tooltip("Mouse look component to disable while paused.")]
        [SerializeField] private PlayerController.MouseLook mouseLook;
        [Tooltip("Player interaction component to disable while paused.")]
        [SerializeField] private PlayerController.PlayerInteraction playerInteraction;
        [Tooltip("Day/Night manager to disable while paused.")]
        [SerializeField] private Game.DayNight.DayNightManager dayNightManager;

        [Header("Behavior")]
        [Tooltip("Keep this manager across scene loads.")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // State
        private readonly HashSet<PauseReason> _reasons = new HashSet<PauseReason>();
        public bool IsPaused => _reasons.Count > 0;

        public event Action<bool, IReadOnlyCollection<PauseReason>> OnPauseChanged;

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Ensure initial unpaused state is applied to targets
            ApplyPauseState(false);
        }

        private void OnDisable()
        {
            // Safety net: if this manager gets disabled while paused, make best effort to restore controls
            if (IsPaused)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[PauseManager] Manager disabled while paused. Restoring targets.");
                }
                ApplyPauseState(false);
                _reasons.Clear();
                RaisePauseChanged(false);
            }
        }

        private void OnValidate()
        {
            // Light warnings only; no heavy work
            if (playerMovement == null)
            {
                Debug.LogWarning("[PauseManager] PlayerMovement not assigned.");
            }
            if (mouseLook == null)
            {
                Debug.LogWarning("[PauseManager] MouseLook not assigned.");
            }
            if (playerInteraction == null)
            {
                Debug.LogWarning("[PauseManager] PlayerInteraction not assigned.");
            }
            if (dayNightManager == null)
            {
                Debug.LogWarning("[PauseManager] DayNightManager not assigned.");
            }
        }

        // Public API --------------------------------------------------------

        public void RequestPause(PauseReason reason)
        {
            if (_reasons.Add(reason))
            {
                if (_reasons.Count == 1)
                {
                    // Transition to paused
                    ApplyPauseState(true);
                    if (showDebugLogs) Debug.Log($"[PauseManager] Paused (reason: {reason}).");
                }
                RaisePauseChanged(true);
            }
        }

        public void ReleasePause(PauseReason reason)
        {
            if (_reasons.Remove(reason))
            {
                if (_reasons.Count == 0)
                {
                    // Transition to unpaused
                    ApplyPauseState(false);
                    if (showDebugLogs) Debug.Log($"[PauseManager] Unpaused (released: {reason}).");
                    RaisePauseChanged(false);
                }
                else
                {
                    // Still paused by other reasons; notify for observability
                    RaisePauseChanged(true);
                }
            }
        }

        public IReadOnlyCollection<PauseReason> GetActiveReasons() => _reasons;

        // Internal ----------------------------------------------------------

        private void ApplyPauseState(bool paused)
        {
            // Player controls
            if (playerMovement != null) playerMovement.enabled = !paused;
            if (mouseLook != null) mouseLook.enabled = !paused;
            if (playerInteraction != null) playerInteraction.enabled = !paused;

            // Day/Night system
            if (dayNightManager != null) dayNightManager.enabled = !paused;
        }

        private void RaisePauseChanged(bool paused)
        {
            try
            {
                OnPauseChanged?.Invoke(paused, _reasons);
            }
            catch (Exception)
            {
                // Swallow observer exceptions to keep gameplay resilient
            }
        }

        // Debug helpers (call via context menu or UI buttons) --------------
        [ContextMenu("Debug/Pause (UI)")]
        private void DebugPauseUI()
        {
            RequestPause(PauseReason.UI);
        }

        [ContextMenu("Debug/Resume (UI)")]
        private void DebugResumeUI()
        {
            ReleasePause(PauseReason.UI);
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Pause manager idempotente. Deshabilita control de jugador y Day/Night según razones de pausa.
// RelatedScripts: PlayerController.PlayerMovement, PlayerController.MouseLook, PlayerController.PlayerInteraction, Game.DayNight.DayNightManager
// UsesSO: No
// ReceivesFrom: UI (menús), Pensamientos Oníricos, Transiciones, Cutscenes (RequestPause/ReleasePause)
// SendsTo: Habilita/deshabilita componentes (player y Day/Night)
// Adjuntar a: GameObject en escena raíz ("PauseManager"). Marcar DontDestroyOnLoad si procede. Asignar referencias por Inspector.
