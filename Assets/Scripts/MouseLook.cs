using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core; // GameConfigProvider

namespace PlayerController
{
	/// <summary>
	/// Simple mouse/gamepad look: yaw on player (this.transform), pitch on camera (cameraTransform).
	/// Uses New Input System via an InputActionReference (Vector2). Sensitivity & clamp from PlayerSettingsSO.
	/// KISS: no smoothing by default, no cursor management unless enabled.
	/// </summary>
	[AddComponentMenu("Spookie/Mouse Look")]
	public class MouseLook : MonoBehaviour
	{
		[Header("References")]
		[Tooltip("Configuración compartida del jugador (sensibilidad, ángulo máximo, etc.)")]
		[SerializeField] private PlayerSettingsSO playerSettings;
		[Tooltip("Transform de la cámara (hija del Player). El pitch se aplica aquí.")]
		[SerializeField] private Transform cameraTransform;

		[Header("Input (New Input System)")]
		[Tooltip("Acción Vector2 de 'Look' (por ejemplo, Mouse delta + Gamepad right stick). Asignar por Inspector.")]
		[SerializeField] private InputActionReference lookAction;

		[Header("Tuning")]
		[Tooltip("Multiplicador adicional por-instancia (se multiplica por PlayerSettingsSO.MouseSensitivity y LookSensitivityMultiplier)")]
		[SerializeField] private float sensitivityMultiplier = 1f;

		[Header("Cursor (legacy - deshabilitado)")]
		[Tooltip("DEPRECATED: usar CursorStateController central. Este flag ya no hace nada, se mantiene para compatibilidad de prefabs.")]
		[SerializeField] private bool manageCursor = false; // mantenido pero sin efecto

		[Header("Debug")]
		[SerializeField] private bool showDebugLogs = true;

		// State
		private float yaw;       // smoothed yaw
		private float pitch;     // smoothed pitch
		private float yawTarget; // target yaw from input
		private float pitchTarget; // target pitch from input
		private bool isReady;
	// Avoid initial spike from input when locking cursor at start
	private bool suppressFirstDelta;

		private void Awake()
		{
			InitializeAnglesFromTransforms();
		}

		private void OnEnable()
		{
			ValidateReferences();

			// Eliminado: gestión directa del cursor. Ahora central via CursorStateController externo.
			if (lookAction != null)
			{
				try { lookAction.action.Enable(); } catch { /* ignore if already enabled */ }
			}

			// Ignore the first non-zero delta right after enabling/locking
			suppressFirstDelta = true;
		}

		private void OnDisable()
		{
			if (lookAction != null)
			{
				try { lookAction.action.Disable(); } catch { /* ignore */ }
			}
			// Eliminado: liberar cursor aquí (lo hará quien corresponda por refcount en CursorStateController)
		}

		private void Update()
		{
			if (!isReady) return;
			if (lookAction == null)
			{
				#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (showDebugLogs) Debug.LogWarning("[MouseLook] lookAction no asignada. Sin input.");
				#endif
				return;
			}

			Vector2 delta = lookAction.action.ReadValue<Vector2>();
			// Discard the first non-zero delta to avoid big jump at startup
			if (suppressFirstDelta)
			{
				if (delta.sqrMagnitude > 0f)
				{
					suppressFirstDelta = false;
					return;
				}
			}
			if (delta.sqrMagnitude <= 0f) return;

			// Sensibilidad normalizada 0..1 desde el SO, multiplicador global del SO y multiplicador local opcional
			float sens = Mathf.Clamp01(playerSettings.MouseSensitivity)
					   * Mathf.Max(0f, playerSettings.LookSensitivityMultiplier)
					   * Mathf.Max(0f, sensitivityMultiplier);
			float x = delta.x * sens;
			bool invertY = playerSettings != null && playerSettings.InvertVerticalLook;
			float yScale = playerSettings != null ? Mathf.Clamp01(playerSettings.MouseYScale) : 1f;
			float y = delta.y * sens * yScale * (invertY ? 1f : -1f);

			// Update targets
			yawTarget = NormalizeAngle(yawTarget + x);
			pitchTarget = NormalizeAngle(pitchTarget + y);

			// Clamp pitch target
			float maxAngle = Mathf.Clamp(playerSettings.MaxLookAngle, 0f, 89.9f);
			pitchTarget = Mathf.Clamp(pitchTarget, -maxAngle, maxAngle);
		}

		private void LateUpdate()
		{
			if (!isReady) return;
			// Easing simple usando LerpAngle/Lerp
			bool easing = playerSettings != null && playerSettings.LookEasingEnabled;
			float amount = playerSettings != null ? Mathf.Clamp01(playerSettings.LookEasingAmount) : 0.2f;
			if (easing && amount > 0f)
			{
				// Escalar por deltaTime para que sea frame-rate independent
				float t = 1f - Mathf.Pow(1f - amount, Time.deltaTime * 60f); // convierte amount@60fps a amount@dt
				yaw = Mathf.LerpAngle(yaw, yawTarget, t);
				pitch = Mathf.Lerp(pitch, pitchTarget, t);
			}
			else
			{
				// Sin easing: saltar directamente a los targets
				yaw = yawTarget;
				pitch = pitchTarget;
			}

			// Asegurar clamp del pitch cada frame, incluso si no hubo input
			float maxAngle = playerSettings != null ? Mathf.Clamp(playerSettings.MaxLookAngle, 0f, 89.9f) : 80f;
			pitch = Mathf.Clamp(pitch, -maxAngle, maxAngle);
			pitchTarget = Mathf.Clamp(pitchTarget, -maxAngle, maxAngle);

			// Aplicar rotaciones ya suavizadas
			ApplyRotations();
		}

		private void ApplyRotations()
		{
			// Apply to player (yaw)
			transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

			// Apply to camera (pitch)
			if (cameraTransform != null)
			{
				Vector3 camEuler = cameraTransform.localEulerAngles;
				camEuler.x = pitch;
				camEuler.y = 0f; // keep camera yaw zero relative to player
				camEuler.z = 0f;
				cameraTransform.localRotation = Quaternion.Euler(camEuler);
			}
		}

		private void InitializeAnglesFromTransforms()
		{
			// Initialize from transforms
			yaw = yawTarget = NormalizeAngle(transform.localEulerAngles.y);
			if (cameraTransform != null)
			{
				pitch = pitchTarget = NormalizeAngle(cameraTransform.localEulerAngles.x);
				// Clamp inicial para evitar nacer mirando demasiado arriba/abajo
				float maxAngle = playerSettings != null ? Mathf.Clamp(playerSettings.MaxLookAngle, 0f, 89.9f) : 80f;
				pitch = Mathf.Clamp(pitch, -maxAngle, maxAngle);
				pitchTarget = Mathf.Clamp(pitchTarget, -maxAngle, maxAngle);
			}
		}

		private float NormalizeAngle(float angle)
		{
			angle %= 360f;
			if (angle > 180f) angle -= 360f;
			return angle;
		}

		private void ValidateReferences()
		{
			isReady = true;

			// Optional: auto-wire from GameConfigProvider if left unassigned
			if (playerSettings == null)
			{
				var provider = FindObjectOfType<GameConfigProvider>();
				var cfg = provider != null ? provider.Config : null;
				if (cfg != null) playerSettings = cfg.PlayerSettings;
			}

			if (playerSettings == null)
			{
				Debug.LogError("[MouseLook] Falta referencia a PlayerSettingsSO.");
				isReady = false;
			}

			if (cameraTransform == null)
			{
				// Intentar obtener una cámara razonable
				var cam = GetComponentInChildren<Camera>();
				if (cam != null) cameraTransform = cam.transform;
				else if (Camera.main != null) cameraTransform = Camera.main.transform;

				if (cameraTransform == null)
				{
					Debug.LogError("[MouseLook] Falta cameraTransform. Asigna la cámara del jugador.");
					isReady = false;
				}
			}

			if (showDebugLogs)
			{
				#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.Log($"[MouseLook] Ready={isReady}, Sens={playerSettings?.MouseSensitivity}, MaxAngle={playerSettings?.MaxLookAngle}");
				#endif
			}
		}

		private void OnValidate()
		{
			// Evitar NaNs/negativos
			sensitivityMultiplier = Mathf.Max(0f, sensitivityMultiplier);
			// No recalculamos isReady aquí para evitar spam en editor; se valida en OnEnable
		}

		[ContextMenu("Validate Now")]
		private void ContextValidate()
		{
			ValidateReferences();
		}

		// Public API minimal
		public void SetYaw(float newYaw)
		{
			yaw = NormalizeAngle(newYaw);
			// Mantener sincronizados los targets para evitar "rebote" con easing activo
			yawTarget = yaw;
			ApplyRotations();
		}

		public void SetPitch(float newPitch)
		{
			float maxAngle = playerSettings != null ? Mathf.Clamp(playerSettings.MaxLookAngle, 0f, 89.9f) : 80f;
			float clamped = Mathf.Clamp(NormalizeAngle(newPitch), -maxAngle, maxAngle);
			pitch = clamped;
			// Mantener sincronizados los targets para evitar "rebote" con easing activo
			pitchTarget = clamped;
			ApplyRotations();
		}
	}
}

// ScriptRole: Control de mirada (yaw del jugador y pitch de la cámara) usando el Nuevo Input System
// RelatedScripts: PlayerMovement, PlayerInteraction
// UsesSO: PlayerSettingsSO
// ReceivesFrom: InputAction (Vector2 Look)
// SendsTo: Transform del Player (yaw), Transform de la Cámara (pitch)
//
// Nota: La gestión de cursor se ha externalizado. Usar:
//   CursorStateController.Instance?.SetLocked("Gameplay"); en el sistema que habilita control de cámara
//   CursorStateController.Instance?.SetFree("Pausa/Inventario"); al mostrar UI.
