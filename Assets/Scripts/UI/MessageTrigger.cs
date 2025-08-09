using UnityEngine;

namespace Game.UI
{
	/// <summary>
	/// Simple trigger to enqueue a LowerMessage by localization key. One-shot runtime with optional PlayerPrefs persistence and cooldown.
	/// </summary>
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("Spookie/Message Trigger")]
	public class MessageTrigger : MonoBehaviour
	{
		[Header("Target")]
		[SerializeField] private LowerMessageController lower;

		[Header("Content")]
		[Tooltip("Localization key for the message text")]
		[SerializeField] private string key;
		[SerializeField] private AudioClip audio;

		[Header("Rules")]
		[Tooltip("Unique id to persist as seen (PlayerPrefs). Leave empty for session-only one-shot.")]
		[SerializeField] private string persistentId;
		[SerializeField] private bool oneShotRuntime = true;
		[Tooltip("Cooldown in seconds to prevent refire")]
		[SerializeField] private float cooldownSeconds = 0f;
		[Tooltip("If true, bypass global cooldown as repeatable")]
		[SerializeField] private bool repeatable = false;
		[Tooltip("<=0 uses controller default")]
		[SerializeField] private float autoCloseSeconds = -1f;

		[Header("Trigger")]
		[SerializeField] private string requiredTag = "Player";

		[Header("Debug")]
		[SerializeField] private bool showDebugLogs = true;

		private bool _firedRuntime;
		private float _lastFireTime = -999f;
		private const string PP_Prefix = "Spookie.MsgTrig.";

		private void Reset()
		{
			var col = GetComponent<Collider>();
			if (col != null) col.isTrigger = true;
		}

		private void OnValidate()
		{
			if (lower == null) lower = FindObjectOfType<LowerMessageController>();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
			TryFire();
		}

		public void TryFire()
		{
			if (lower == null || string.IsNullOrEmpty(key)) return;
			if (oneShotRuntime && _firedRuntime) return;
			if (!string.IsNullOrEmpty(persistentId) && PlayerPrefs.GetInt(PP_Prefix + persistentId, 0) == 1) return;
			if (cooldownSeconds > 0f && (Time.time - _lastFireTime) < cooldownSeconds) return;

			lower.EnqueueKey(key, audio, autoCloseSeconds, repeatable);
			_firedRuntime = true;
			_lastFireTime = Time.time;
			if (!string.IsNullOrEmpty(persistentId))
			{
				PlayerPrefs.SetInt(PP_Prefix + persistentId, 1);
				PlayerPrefs.Save();
			}

			#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (showDebugLogs) Debug.Log($"[MessageTrigger] Fired key '{key}' on {name}");
			#endif
		}
	}
}

