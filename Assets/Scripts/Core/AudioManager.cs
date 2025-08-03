using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;
        public AudioSource source;
    }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    public AudioSource voiceSource;
    
    [Header("Sound Effects")]
    public SoundEffect[] soundEffects;
    
    [Header("Ambient Sounds")]
    public AudioClip[] dayAmbientSounds;
    public AudioClip[] nightAmbientSounds;
    public AudioClip[] horrorAmbientSounds;
    
    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip[] levelMusic;
    public AudioClip[] horrorMusic;
    
    [Header("Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;
    public float ambientVolume = 0.5f;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }
    
    private Dictionary<string, SoundEffect> soundDictionary;
    private AudioClip currentAmbientClip;
    private AudioClip currentMusicClip;

    private void Awake()
    {
        Debug.Log("AudioManager: Initializing");
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            InitializeSoundDictionary();
        }
        else if (instance != this)
        {
            Debug.LogWarning("AudioManager: Duplicate instance found, destroying");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("AudioManager: Starting audio manager");
        PlayMainMenuMusic();
    }

    private void InitializeAudioSources()
    {
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("MusicSource");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }
        
        if (ambientSource == null)
        {
            GameObject ambientGO = new GameObject("AmbientSource");
            ambientGO.transform.SetParent(transform);
            ambientSource = ambientGO.AddComponent<AudioSource>();
            ambientSource.loop = true;
        }
        
        if (voiceSource == null)
        {
            GameObject voiceGO = new GameObject("VoiceSource");
            voiceGO.transform.SetParent(transform);
            voiceSource = voiceGO.AddComponent<AudioSource>();
        }
    }

    private void InitializeSoundDictionary()
    {
        soundDictionary = new Dictionary<string, SoundEffect>();
        
        foreach (var sound in soundEffects)
        {
            if (!string.IsNullOrEmpty(sound.name))
            {
                soundDictionary[sound.name] = sound;
            }
        }
        
        Debug.Log($"AudioManager: Initialized {soundDictionary.Count} sound effects");
    }

    public void PlaySound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            SoundEffect sound = soundDictionary[soundName];
            if (sound.clip != null)
            {
                sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume);
                Debug.Log($"AudioManager: Playing sound {soundName}");
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Sound '{soundName}' not found");
        }
    }

    public void PlaySoundAtPosition(string soundName, Vector3 position)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            SoundEffect sound = soundDictionary[soundName];
            if (sound.clip != null)
            {
                AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume);
                Debug.Log($"AudioManager: Playing sound {soundName} at position {position}");
            }
        }
    }

    public void PlayMusic(AudioClip musicClip, bool fadeIn = true)
    {
        if (musicClip != null && musicClip != currentMusicClip)
        {
            currentMusicClip = musicClip;
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
            
            if (fadeIn)
            {
                StartCoroutine(FadeInMusic());
            }
            
            Debug.Log($"AudioManager: Playing music {musicClip.name}");
        }
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            PlayMusic(mainMenuMusic);
        }
    }

    public void PlayLevelMusic(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelMusic.Length)
        {
            PlayMusic(levelMusic[levelIndex]);
        }
    }

    public void PlayHorrorMusic(int index = 0)
    {
        if (index >= 0 && index < horrorMusic.Length)
        {
            PlayMusic(horrorMusic[index]);
        }
    }

    public void PlayAmbientSound(AudioClip ambientClip, bool fadeIn = true)
    {
        if (ambientClip != null && ambientClip != currentAmbientClip)
        {
            currentAmbientClip = ambientClip;
            ambientSource.clip = ambientClip;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
            
            if (fadeIn)
            {
                StartCoroutine(FadeInAmbient());
            }
            
            Debug.Log($"AudioManager: Playing ambient sound {ambientClip.name}");
        }
    }

    public void PlayDayAmbient()
    {
        if (dayAmbientSounds.Length > 0)
        {
            AudioClip randomDaySound = dayAmbientSounds[Random.Range(0, dayAmbientSounds.Length)];
            PlayAmbientSound(randomDaySound);
        }
    }

    public void PlayNightAmbient()
    {
        if (nightAmbientSounds.Length > 0)
        {
            AudioClip randomNightSound = nightAmbientSounds[Random.Range(0, nightAmbientSounds.Length)];
            PlayAmbientSound(randomNightSound);
        }
    }

    public void PlayHorrorAmbient()
    {
        if (horrorAmbientSounds.Length > 0)
        {
            AudioClip randomHorrorSound = horrorAmbientSounds[Random.Range(0, horrorAmbientSounds.Length)];
            PlayAmbientSound(randomHorrorSound);
        }
    }

    public void PlayVoice(AudioClip voiceClip)
    {
        if (voiceClip != null)
        {
            voiceSource.clip = voiceClip;
            voiceSource.volume = sfxVolume * masterVolume;
            voiceSource.Play();
            Debug.Log($"AudioManager: Playing voice clip {voiceClip.name}");
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentMusicClip = null;
        Debug.Log("AudioManager: Stopped music");
    }

    public void StopAmbient()
    {
        ambientSource.Stop();
        currentAmbientClip = null;
        Debug.Log("AudioManager: Stopped ambient sound");
    }

    public void StopAllAudio()
    {
        musicSource.Stop();
        sfxSource.Stop();
        ambientSource.Stop();
        voiceSource.Stop();
        Debug.Log("AudioManager: Stopped all audio");
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        Debug.Log($"AudioManager: Master volume set to {masterVolume}");
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
        Debug.Log($"AudioManager: Music volume set to {musicVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        Debug.Log($"AudioManager: SFX volume set to {sfxVolume}");
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ambientSource.volume = ambientVolume * masterVolume;
        Debug.Log($"AudioManager: Ambient volume set to {ambientVolume}");
    }

    private void UpdateAllVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;
        ambientSource.volume = ambientVolume * masterVolume;
        voiceSource.volume = sfxVolume * masterVolume;
    }

    private System.Collections.IEnumerator FadeInMusic()
    {
        float targetVolume = musicVolume * masterVolume;
        musicSource.volume = 0f;
        
        while (musicSource.volume < targetVolume)
        {
            musicSource.volume += Time.deltaTime * 0.5f;
            yield return null;
        }
        
        musicSource.volume = targetVolume;
    }

    private System.Collections.IEnumerator FadeInAmbient()
    {
        float targetVolume = ambientVolume * masterVolume;
        ambientSource.volume = 0f;
        
        while (ambientSource.volume < targetVolume)
        {
            ambientSource.volume += Time.deltaTime * 0.3f;
            yield return null;
        }
        
        ambientSource.volume = targetVolume;
    }
}

// ScriptRole: Manages all audio including music, SFX, ambient sounds and voice
// Dependencies: AudioSource components
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: GameManager, SceneManager, PlayerFPSController
// SendsTo: None (plays audio directly) 