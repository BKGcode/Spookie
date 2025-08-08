using System.Collections;
using UnityEngine;

namespace AudioSystem
{
    public class MusicController : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public IEnumerator TransitionToDay(AudioSource musicSource, AudioClip dayClip, float targetVolume, float fadeDuration)
        {
            if (musicSource == null || dayClip == null) { yield break; }

            if (musicSource.isPlaying)
            {
                yield return Fade(musicSource, 0f, fadeDuration * 0.5f);
            }

            musicSource.clip = dayClip;
            musicSource.Play();
            yield return Fade(musicSource, targetVolume, fadeDuration * 0.5f);

            if (showDebugLogs) { Debug.Log("[MusicController] Transitioned to day music"); }
        }

        public IEnumerator TransitionToNight(AudioSource musicSource, AudioClip nightClip, float targetVolume, float fadeDuration)
        {
            if (musicSource == null || nightClip == null) { yield break; }

            if (musicSource.isPlaying)
            {
                yield return Fade(musicSource, 0f, fadeDuration * 0.5f);
            }

            musicSource.clip = nightClip;
            musicSource.Play();
            yield return Fade(musicSource, targetVolume, fadeDuration * 0.5f);

            if (showDebugLogs) { Debug.Log("[MusicController] Transitioned to night music"); }
        }

        private IEnumerator Fade(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) { yield break; }
            float startVolume = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                source.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }
            source.volume = targetVolume;
        }
    }
}

// ScriptRole: Handles music transitions (day/night) with fades
// RelatedScripts: AudioManager
// UsesSO: None
// ReceivesFrom: AudioManager
// SendsTo: AudioSource (music)


