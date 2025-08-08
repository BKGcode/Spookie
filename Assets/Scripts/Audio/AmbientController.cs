using UnityEngine;

namespace AudioSystem
{
    public class AmbientController : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public void PlayAmbient(AudioSource ambientSource, AudioClip clip, float volume)
        {
            if (ambientSource == null || clip == null) { return; }
            ambientSource.clip = clip;
            ambientSource.volume = Mathf.Clamp01(volume);
            ambientSource.loop = true;
            ambientSource.Play();

            if (showDebugLogs) { Debug.Log($"[AmbientController] Ambient playing: {clip.name}"); }
        }
    }
}

// ScriptRole: Plays ambient loops for day/night contexts
// RelatedScripts: AudioManager
// UsesSO: None
// ReceivesFrom: AudioManager
// SendsTo: AudioSource (ambient)


