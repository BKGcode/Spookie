using UnityEngine;
using DayNightSystem.Core;
using DayNightSystem;

namespace TransitionSystem.Audio
{
    public class AudioTransitionController : MonoBehaviour
    {
        [Header("Audio References")]
        [SerializeField] private AudioManager audioManager;
        
        [Header("Audio Settings")]
        [SerializeField] private bool enableAudioTransitions = true;
        [SerializeField] private float audioFadeDuration = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Events
        public System.Action OnSleepSoundPlayed;
        public System.Action OnWakeUpSoundPlayed;
        public System.Action OnFaintSoundPlayed;
        public System.Action OnMorningAmbientPlayed;
        
        private void Awake()
        {
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] Initialized");
        }
        
        private void ValidateReferences()
        {
            if (audioManager == null)
            {
                Debug.LogWarning("[AudioTransitionController] AudioManager reference is missing!");
            }
        }
        
        public void PlaySleepSound()
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            audioManager.PlaySleepSound();
            OnSleepSoundPlayed?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] Sleep sound played");
        }
        
        public void PlayWakeUpSound()
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            audioManager.PlayWakeUpSound();
            OnWakeUpSoundPlayed?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] Wake up sound played");
        }
        
        public void PlayFaintSound()
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            audioManager.PlayFaintSound();
            OnFaintSoundPlayed?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] Faint sound played");
        }
        
        public void PlayMorningAmbient()
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            audioManager.PlayMorningAmbient();
            OnMorningAmbientPlayed?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] Morning ambient sound played");
        }
        
        public void PlayDayToNightSound()
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            // Play transition sound for day to night
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] Day to night sound played");
        }
        
        public void PlayNightToDaySound()
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            // Play transition sound for night to day
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] Night to day sound played");
        }
        
        public void FadeOutAudio(float duration = -1f)
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            float fadeDuration = duration > 0 ? duration : audioFadeDuration;
            
            if (showDebugLogs)
                Debug.Log($"[AudioTransitionController] Fading out audio over {fadeDuration}s");
        }
        
        public void FadeInAudio(float duration = -1f)
        {
            if (!enableAudioTransitions || audioManager == null) return;
            
            float fadeDuration = duration > 0 ? duration : audioFadeDuration;
            
            if (showDebugLogs)
                Debug.Log($"[AudioTransitionController] Fading in audio over {fadeDuration}s");
        }
        
        public void SetAudioManager(AudioManager manager)
        {
            audioManager = manager;
            
            if (showDebugLogs)
                Debug.Log("[AudioTransitionController] AudioManager reference set");
        }
        
        public void SetEnableAudioTransitions(bool enable)
        {
            enableAudioTransitions = enable;
            
            if (showDebugLogs)
                Debug.Log($"[AudioTransitionController] Audio transitions {(enable ? "enabled" : "disabled")}");
        }
        
        public void SetAudioFadeDuration(float duration)
        {
            audioFadeDuration = Mathf.Max(0.1f, duration);
        }
        
        public bool IsAudioEnabled()
        {
            return enableAudioTransitions && audioManager != null;
        }
        
        [ContextMenu("Test Sleep Sound")]
        public void TestSleepSound()
        {
            PlaySleepSound();
        }
        
        [ContextMenu("Test Wake Up Sound")]
        public void TestWakeUpSound()
        {
            PlayWakeUpSound();
        }
        
        [ContextMenu("Test Faint Sound")]
        public void TestFaintSound()
        {
            PlayFaintSound();
        }
        
        [ContextMenu("Test Morning Ambient")]
        public void TestMorningAmbient()
        {
            PlayMorningAmbient();
        }
        
        [ContextMenu("Test Audio Fade Out")]
        public void TestAudioFadeOut()
        {
            FadeOutAudio();
        }
        
        [ContextMenu("Test Audio Fade In")]
        public void TestAudioFadeIn()
        {
            FadeInAudio();
        }
        
        [ContextMenu("Toggle Audio Transitions")]
        public void ToggleAudioTransitions()
        {
            SetEnableAudioTransitions(!enableAudioTransitions);
        }
        
        // ScriptRole: Manages audio transitions for day/night cycles and player states
        // RelatedScripts: AudioManager, VisualTransitionController, SleepTransitionController, FaintTransitionController
        // UsesSO: None
        // ReceivesFrom: TransitionManager, TransitionEventController
        // SendsTo: AudioManager
    }
}
