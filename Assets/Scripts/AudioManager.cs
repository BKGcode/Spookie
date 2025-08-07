using UnityEngine;

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
        private DayNightManager dayNightManager;
        private GameStateManager gameStateManager;
        private bool isTransitioning = false;
        
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
            dayNightManager = FindObjectOfType<DayNightManager>();
            gameStateManager = FindObjectOfType<GameStateManager>();
            
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
                Debug.LogError("[AudioManager] Music AudioSource is missing!");
            }
            
            if (sfxSource == null)
            {
                Debug.LogError("[AudioManager] SFX AudioSource is missing!");
            }
            
            if (ambientSource == null)
            {
                Debug.LogError("[AudioManager] Ambient AudioSource is missing!");
            }
        }
        
        private void OnDayStart()
        {
            if (showDebugLogs)
                Debug.Log("[AudioManager] Day started - transitioning to day audio");
            
            StartCoroutine(TransitionToDayAudio());
        }
        
        private void OnNightStart()
        {
            if (showDebugLogs)
                Debug.Log("[AudioManager] Night started - transitioning to night audio");
            
            StartCoroutine(TransitionToNightAudio());
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
            switch (newState)
            {
                case GameState.NightBlocked:
                    // Night is blocked - ensure night audio is playing
                    if (showDebugLogs)
                        Debug.Log("[AudioManager] Night blocked state - ensuring night audio");
                    break;
                    
                case GameState.Playing:
                    // Normal gameplay - audio should match current day/night state
                    if (showDebugLogs)
                        Debug.Log("[AudioManager] Playing state - audio should match day/night");
                    break;
            }
        }
        
        private System.Collections.IEnumerator TransitionToDayAudio()
        {
            if (isTransitioning) yield break;
            
            isTransitioning = true;
            
            if (showDebugLogs)
                Debug.Log("[AudioManager] Starting transition to day audio");
            
            // Play transition sound
            if (nightToDayTransition != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(nightToDayTransition);
            }
            
            // Fade out current music
            yield return StartCoroutine(FadeAudioSource(musicSource, 0f, fadeDuration * 0.5f));
            
            // Change music to day
            if (dayMusic != null && musicSource != null)
            {
                musicSource.clip = dayMusic;
                musicSource.Play();
            }
            
            // Fade in day music
            yield return StartCoroutine(FadeAudioSource(musicSource, musicVolume, fadeDuration * 0.5f));
            
            // Change ambient to day
            if (dayAmbient != null && ambientSource != null)
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
            if (isTransitioning) yield break;
            
            isTransitioning = true;
            
            if (showDebugLogs)
                Debug.Log("[AudioManager] Starting transition to night audio");
            
            // Play transition sound
            if (dayToNightTransition != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(dayToNightTransition);
            }
            
            // Fade out current music
            yield return StartCoroutine(FadeAudioSource(musicSource, 0f, fadeDuration * 0.5f));
            
            // Change music to night
            if (nightMusic != null && musicSource != null)
            {
                musicSource.clip = nightMusic;
                musicSource.Play();
            }
            
            // Fade in night music
            yield return StartCoroutine(FadeAudioSource(musicSource, musicVolume, fadeDuration * 0.5f));
            
            // Change ambient to night
            if (nightAmbient != null && ambientSource != null)
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
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        public void PlaySleepSound()
        {
            if (sleepSound != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(sleepSound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Playing sleep sound");
            }
        }
        
        public void PlayFaintSound()
        {
            if (faintSound != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(faintSound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Playing faint sound");
            }
        }
        
        public void PlayMessageNotification()
        {
            if (messageNotification != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(messageNotification);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Playing message notification");
            }
        }
        
        public void PlayWarningSound()
        {
            if (warningSound != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(warningSound);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Playing warning sound");
            }
        }
        
        public void PlayPenaltySound()
        {
            if (sfxSource != null && penaltySound != null)
            {
                sfxSource.PlayOneShot(penaltySound, sfxVolume);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Playing penalty sound");
            }
        }
        
        public void PlayWakeUpSound()
        {
            if (sfxSource != null && wakeUpSound != null)
            {
                sfxSource.PlayOneShot(wakeUpSound, sfxVolume);
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Playing wake up sound");
            }
        }
        
        public void PlayMorningAmbient()
        {
            if (ambientSource != null && morningAmbient != null)
            {
                ambientSource.clip = morningAmbient;
                ambientSource.volume = ambientVolume;
                ambientSource.Play();
                
                if (showDebugLogs)
                    Debug.Log("[AudioManager] Playing morning ambient sound");
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
        
        // Testing methods
        [ContextMenu("Test Day Audio")]
        public void TestDayAudio()
        {
            StartCoroutine(TransitionToDayAudio());
        }
        
        [ContextMenu("Test Night Audio")]
        public void TestNightAudio()
        {
            StartCoroutine(TransitionToNightAudio());
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

// ScriptRole: Manages all audio for the day/night system including music, ambient, SFX, and wake up sounds
// RelatedScripts: DayNightManager, GameStateManager, TransitionHandler
// UsesSO: None
// ReceivesFrom: DayNightManager events, GameStateManager events
// SendsTo: AudioSource components
