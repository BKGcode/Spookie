using UnityEngine;
using DayNightSystem.Core;

namespace DayNightSystem
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        
        [Header("Day/Night Audio")]
        [SerializeField] private AudioClip dayMusic;
        [SerializeField] private AudioClip nightMusic;
        [SerializeField] private AudioClip dayAmbient;
        [SerializeField] private AudioClip nightAmbient;
        
        [Header("Transition Audio")]
        [SerializeField] private AudioClip dayToNightTransition;
        [SerializeField] private AudioClip nightToDayTransition;
        [SerializeField] private AudioClip sleepSound;
        [SerializeField] private AudioClip faintSound;
        
        [Header("UI Audio")]
        [SerializeField] private AudioClip messageNotification;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip penaltySound;
        
        [Header("Wake Up Sounds")]
        [SerializeField] private AudioClip wakeUpSound;
        [SerializeField] private AudioClip morningAmbient;
        
        [Header("Settings")]
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float ambientVolume = 0.5f;
        [SerializeField] private float fadeDuration = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Private fields
        private DayNightSystem.Core.DayNightManager dayNightManager;
        private GameStateManager gameStateManager;
        private bool isTransitioning = false;
        [SerializeField] private AudioSystem.MusicController musicController; // Delegation (optional)
        [SerializeField] private AudioSystem.AmbientController ambientController; // Delegation (optional)
        [SerializeField] private AudioSystem.SfxController sfxController; // Delegation (optional)
        
        // Events
        public System.Action OnAudioTransitionComplete;
        
        private void Awake()
        {
            FindReferences();
            SetupAudioSources();
            ValidateReferences();
        }
        
        private void OnEnable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart += OnDayStart;
                dayNightManager.OnNightStart += OnNightStart;
                dayNightManager.OnPlayerSlept += OnPlayerSlept;
                dayNightManager.OnPlayerFainted += OnPlayerFainted;
            }
            
            if (gameStateManager != null)
            {
                gameStateManager.OnGameStateChanged += OnGameStateChanged;
            }
        }
        
        private void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.OnDayStart -= OnDayStart;
                dayNightManager.OnNightStart -= OnNightStart;
                dayNightManager.OnPlayerSlept -= OnPlayerSlept;
                dayNightManager.OnPlayerFainted -= OnPlayerFainted;
            }
            
            if (gameStateManager != null)
            {
                gameStateManager.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        
        private void FindReferences()
        {
            dayNightManager = FindObjectOfType<DayNightSystem.Core.DayNightManager>();
            gameStateManager = FindObjectOfType<GameStateManager>();
            if (musicController == null) { musicController = GetComponent<AudioSystem.MusicController>(); }
            if (ambientController == null) { ambientController = GetComponent<AudioSystem.AmbientController>(); }
            if (sfxController == null) { sfxController = GetComponent<AudioSystem.SfxController>(); }
            
            // Auto-setup audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
        }
        
        private void SetupAudioSources()
        {
            // Configure music source
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
                musicSource.spatialBlend = 0f; // 2D
            }
            
            // Configure SFX source
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
                sfxSource.spatialBlend = 0f; // 2D
            }
            
            // Configure ambient source
            if (ambientSource != null)
            {
                ambientSource.volume = ambientVolume;
                ambientSource.spatialBlend = 0f; // 2D
            }
        }
        
        private void ValidateReferences()
        {
            if (dayNightManager == null)
            {
                Debug.LogWarning("[AudioManager] No DayNightManager found in scene!");
            }
            
            if (gameStateManager == null)
            {
                Debug.LogWarning("[AudioManager] No GameStateManager found in scene!");
            }
            
            if (musicSource == null)
            {
                Debug.LogError("[AudioManager] Music source is missing!");
            }
            
            if (sfxSource == null)
            {
                Debug.LogError("[AudioManager] SFX source is missing!");
            }
            
            if (ambientSource == null)
            {
                Debug.LogError("[AudioManager] Ambient source is missing!");
            }
        }
        
        private void OnDayStart()
        {
            if (isTransitioning) return;
            
            if (musicController != null)
            {
                StartCoroutine(musicController.TransitionToDay(musicSource, dayMusic, musicVolume, fadeDuration));
                if (ambientController != null) { ambientController.PlayAmbient(ambientSource, dayAmbient, ambientVolume); }
            }
            else
            {
                StartCoroutine(TransitionToDayAudio());
            }
            
            if (showDebugLogs)
                Debug.Log("[AudioManager] Day started - transitioning to day audio");
        }
        
        private void OnNightStart()
        {
            if (isTransitioning) return;
            
            if (musicController != null)
            {
                StartCoroutine(musicController.TransitionToNight(musicSource, nightMusic, musicVolume, fadeDuration));
                if (ambientController != null) { ambientController.PlayAmbient(ambientSource, nightAmbient, ambientVolume); }
            }
            else
            {
                StartCoroutine(TransitionToNightAudio());
            }
            
            if (showDebugLogs)
                Debug.Log("[AudioManager] Night started - transitioning to night audio");
        }
        
        private void OnPlayerSlept()
        {
            PlaySleepSound();
        }
        
        private void OnPlayerFainted()
        {
            PlayFaintSound();
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            // Handle game state changes that affect audio
            switch (newState)
            {
                case GameState.Paused:
                    // Reduce volume when paused
                    SetMusicVolume(musicVolume * 0.5f);
                    SetAmbientVolume(ambientVolume * 0.5f);
                    break;
                    
                case GameState.Playing:
                    // Restore volume when resuming
                    SetMusicVolume(musicVolume);
                    SetAmbientVolume(ambientVolume);
                    break;
                    
                case GameState.NightBlocked:
                    // Special audio for night blocked state
                    if (ambientSource != null && nightAmbient != null)
                    {
                        ambientSource.clip = nightAmbient;
                        ambientSource.Play();
                    }
                    break;
            }
        }
        
        private System.Collections.IEnumerator TransitionToDayAudio()
        {
            isTransitioning = true;
            
            // Fade out current music
            if (musicSource != null && musicSource.isPlaying)
            {
                yield return StartCoroutine(FadeAudioSource(musicSource, 0f, fadeDuration * 0.5f));
            }
            
            // Switch to day music
            if (musicSource != null && dayMusic != null)
            {
                musicSource.clip = dayMusic;
                musicSource.Play();
                yield return StartCoroutine(FadeAudioSource(musicSource, musicVolume, fadeDuration * 0.5f));
            }
            
            // Switch ambient audio
            if (ambientSource != null && dayAmbient != null)
            {
                ambientSource.clip = dayAmbient;
                ambientSource.Play();
            }
            
            isTransitioning = false;
            OnAudioTransitionComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[AudioManager] Day audio transition completed");
        }
        
        private System.Collections.IEnumerator TransitionToNightAudio()
        {
            isTransitioning = true;
            
            // Fade out current music
            if (musicSource != null && musicSource.isPlaying)
            {
                yield return StartCoroutine(FadeAudioSource(musicSource, 0f, fadeDuration * 0.5f));
            }
            
            // Switch to night music
            if (musicSource != null && nightMusic != null)
            {
                musicSource.clip = nightMusic;
                musicSource.Play();
                yield return StartCoroutine(FadeAudioSource(musicSource, musicVolume, fadeDuration * 0.5f));
            }
            
            // Switch ambient audio
            if (ambientSource != null && nightAmbient != null)
            {
                ambientSource.clip = nightAmbient;
                ambientSource.Play();
            }
            
            isTransitioning = false;
            OnAudioTransitionComplete?.Invoke();
            
            if (showDebugLogs)
                Debug.Log("[AudioManager] Night audio transition completed");
        }
        
        private System.Collections.IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) yield break;
            
            float startVolume = source.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                source.volume = Mathf.Lerp(startVolume, targetVolume, progress);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        public void PlaySleepSound()
        {
            if (sfxController != null) { sfxController.PlayOneShot(sfxSource, sleepSound); }
            else if (sfxSource != null && sleepSound != null)
            {
                sfxSource.PlayOneShot(sleepSound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Sleep sound played");
            }
        }
        
        public void PlayFaintSound()
        {
            if (sfxController != null) { sfxController.PlayOneShot(sfxSource, faintSound); }
            else if (sfxSource != null && faintSound != null)
            {
                sfxSource.PlayOneShot(faintSound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Faint sound played");
            }
        }
        
        public void PlayMessageNotification()
        {
            if (sfxController != null) { sfxController.PlayOneShot(sfxSource, messageNotification); }
            else if (sfxSource != null && messageNotification != null)
            {
                sfxSource.PlayOneShot(messageNotification);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Message notification played");
            }
        }
        
        public void PlayWarningSound()
        {
            if (sfxController != null) { sfxController.PlayOneShot(sfxSource, warningSound); }
            else if (sfxSource != null && warningSound != null)
            {
                sfxSource.PlayOneShot(warningSound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Warning sound played");
            }
        }
        
        public void PlayPenaltySound()
        {
            if (sfxController != null) { sfxController.PlayOneShot(sfxSource, penaltySound); }
            else if (sfxSource != null && penaltySound != null)
            {
                sfxSource.PlayOneShot(penaltySound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Penalty sound played");
            }
        }
        
        public void PlayWakeUpSound()
        {
            if (sfxSource != null && wakeUpSound != null)
            {
                sfxSource.PlayOneShot(wakeUpSound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Wake up sound played");
            }
        }
        
        public void PlayMorningAmbient()
        {
            if (ambientSource != null && morningAmbient != null)
            {
                ambientSource.clip = morningAmbient;
                ambientSource.Play();
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Morning ambient played");
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
            
            if (showDebugLogs)
                Debug.Log($"[AudioManager] Music volume set to: {musicVolume}");
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
            
            if (showDebugLogs)
                Debug.Log($"[AudioManager] SFX volume set to: {sfxVolume}");
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            
            if (ambientSource != null)
            {
                ambientSource.volume = ambientVolume;
            }
            
            if (showDebugLogs)
                Debug.Log($"[AudioManager] Ambient volume set to: {ambientVolume}");
        }
        
        [ContextMenu("Test Day Audio")]
        public void TestDayAudio()
        {
            OnDayStart();
        }
        
        [ContextMenu("Test Night Audio")]
        public void TestNightAudio()
        {
            OnNightStart();
        }
        
        [ContextMenu("Test Sleep Sound")]
        public void TestSleepSound()
        {
            PlaySleepSound();
        }
        
        [ContextMenu("Test Faint Sound")]
        public void TestFaintSound()
        {
            PlayFaintSound();
        }
        
        [ContextMenu("Test Warning Sound")]
        public void TestWarningSound()
        {
            PlayWarningSound();
        }
        
        [ContextMenu("Test Penalty Sound")]
        public void TestPenaltySound()
        {
            PlayPenaltySound();
        }
        
        [ContextMenu("Test Wake Up Sound")]
        public void TestWakeUpSound()
        {
            PlayWakeUpSound();
        }
        
        [ContextMenu("Test Morning Ambient")]
        public void TestMorningAmbient()
        {
            PlayMorningAmbient();
        }
    }
}

// ScriptRole: Manages all audio for the game including day/night transitions and UI sounds
// RelatedScripts: DayNightManager, GameStateManager, TransitionHandler
// UsesSO: None
// ReceivesFrom: DayNightManager events, GameStateManager events
// SendsTo: AudioSource components (music, sfx, ambient)
