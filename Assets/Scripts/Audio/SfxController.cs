using UnityEngine;

namespace AudioSystem
{
    public class SfxController : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public void PlayOneShot(AudioSource sfxSource, AudioClip clip)
        {
            if (sfxSource == null || clip == null) { return; }
            sfxSource.PlayOneShot(clip);

            if (showDebugLogs) { Debug.Log($"[SfxController] Played: {clip.name}"); }
        }
    }
}

// ScriptRole: Plays one-shot SFX (sleep, faint, warning, penalty, notifications)
// RelatedScripts: AudioManager
// UsesSO: None
// ReceivesFrom: AudioManager
// SendsTo: AudioSource (sfx)


