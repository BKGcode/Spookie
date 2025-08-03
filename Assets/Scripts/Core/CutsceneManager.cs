using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CutsceneData
{
    public string cutsceneName;
    public AudioClip voiceClip;
    public string subtitleText;
    public float duration = 3f;
    public bool disablePlayerMovement = true;
    public bool fadeInOut = true;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public bool useCustomCamera = false;
}

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene UI")]
    public Canvas cutsceneCanvas;
    public TextMeshProUGUI subtitleText;
    public Image fadeImage;
    public AudioSource cutsceneAudioSource;
    
    [Header("Cutscenes")]
    public CutsceneData[] cutscenes;
    
    [Header("Settings")]
    public float fadeSpeed = 1f;
    public Color fadeColor = Color.black;
    
    [Header("References")]
    public FeedbackMessagesSO feedbackMessages;
    public PlayerFPSController playerController;
    public Camera playerCamera;
    
    private static CutsceneManager instance;
    public static CutsceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CutsceneManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("CutsceneManager");
                    instance = go.AddComponent<CutsceneManager>();
                }
            }
            return instance;
        }
    }
    
    private Dictionary<string, CutsceneData> cutsceneDictionary;
    private bool isPlayingCutscene = false;
    private Vector3 originalCameraPosition;
    private Vector3 originalCameraRotation;
    private bool originalPlayerMovement;

    private void Awake()
    {
        Debug.Log("CutsceneManager: Initializing");
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCutsceneDictionary();
        }
        else if (instance != this)
        {
            Debug.LogWarning("CutsceneManager: Duplicate instance found, destroying");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("CutsceneManager: Starting cutscene manager");
        
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(false);
        }
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
    }

    private void InitializeCutsceneDictionary()
    {
        cutsceneDictionary = new Dictionary<string, CutsceneData>();
        
        foreach (var cutscene in cutscenes)
        {
            if (!string.IsNullOrEmpty(cutscene.cutsceneName))
            {
                cutsceneDictionary[cutscene.cutsceneName] = cutscene;
            }
        }
        
        Debug.Log($"CutsceneManager: Initialized {cutsceneDictionary.Count} cutscenes");
    }

    public void PlayCutscene(string cutsceneName)
    {
        if (cutsceneDictionary.ContainsKey(cutsceneName))
        {
            Debug.Log($"CutsceneManager: Playing cutscene {cutsceneName}");
            StartCoroutine(PlayCutsceneCoroutine(cutsceneDictionary[cutsceneName]));
        }
        else
        {
            Debug.LogWarning($"CutsceneManager: Cutscene '{cutsceneName}' not found");
        }
    }

    private IEnumerator PlayCutsceneCoroutine(CutsceneData cutscene)
    {
        isPlayingCutscene = true;
        
        // Store original camera and player state
        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.position;
            originalCameraRotation = playerCamera.transform.eulerAngles;
        }
        
        if (playerController != null)
        {
            originalPlayerMovement = playerController.enabled;
        }
        
        // Disable player movement if needed
        if (cutscene.disablePlayerMovement && playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
        
        // Fade in if needed
        if (cutscene.fadeInOut)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        // Set up camera if custom position is specified
        if (cutscene.useCustomCamera && playerCamera != null)
        {
            playerCamera.transform.position = cutscene.cameraPosition;
            playerCamera.transform.eulerAngles = cutscene.cameraRotation;
        }
        
        // Show cutscene UI
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(true);
        }
        
        // Play voice clip
        if (cutscene.voiceClip != null && cutsceneAudioSource != null)
        {
            cutsceneAudioSource.clip = cutscene.voiceClip;
            cutsceneAudioSource.Play();
            Debug.Log($"CutsceneManager: Playing voice clip {cutscene.voiceClip.name}");
        }
        
        // Show subtitle
        if (subtitleText != null && !string.IsNullOrEmpty(cutscene.subtitleText))
        {
            subtitleText.text = cutscene.subtitleText;
            Debug.Log($"CutsceneManager: Showing subtitle: {cutscene.subtitleText}");
        }
        
        // Wait for cutscene duration
        yield return new WaitForSeconds(cutscene.duration);
        
        // Hide cutscene UI
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(false);
        }
        
        if (subtitleText != null)
        {
            subtitleText.text = "";
        }
        
        // Stop audio
        if (cutsceneAudioSource != null)
        {
            cutsceneAudioSource.Stop();
        }
        
        // Restore camera position
        if (cutscene.useCustomCamera && playerCamera != null)
        {
            playerCamera.transform.position = originalCameraPosition;
            playerCamera.transform.eulerAngles = originalCameraRotation;
        }
        
        // Fade out if needed
        if (cutscene.fadeInOut)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // Restore player movement
        if (cutscene.disablePlayerMovement && playerController != null)
        {
            playerController.SetMovementEnabled(originalPlayerMovement);
        }
        
        isPlayingCutscene = false;
        Debug.Log($"CutsceneManager: Finished cutscene {cutscene.cutsceneName}");
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        
        float alpha = 0f;
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
        
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        
        float alpha = 1f;
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
        
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
    }

    public void PlayCutsceneWithMessage(string cutsceneName, string messageKey)
    {
        if (feedbackMessages != null)
        {
            string message = feedbackMessages.GetMessage(messageKey);
            if (!string.IsNullOrEmpty(message))
            {
                // Create temporary cutscene data
                CutsceneData tempCutscene = new CutsceneData
                {
                    cutsceneName = cutsceneName,
                    subtitleText = message,
                    duration = 3f,
                    disablePlayerMovement = true,
                    fadeInOut = true
                };
                
                StartCoroutine(PlayCutsceneCoroutine(tempCutscene));
            }
        }
    }

    public void SkipCurrentCutscene()
    {
        if (isPlayingCutscene)
        {
            Debug.Log("CutsceneManager: Skipping current cutscene");
            StopAllCoroutines();
            
            // Restore player state
            if (playerController != null)
            {
                playerController.SetMovementEnabled(originalPlayerMovement);
            }
            
            if (playerCamera != null)
            {
                playerCamera.transform.position = originalCameraPosition;
                playerCamera.transform.eulerAngles = originalCameraRotation;
            }
            
            // Hide UI
            if (cutsceneCanvas != null)
            {
                cutsceneCanvas.gameObject.SetActive(false);
            }
            
            if (subtitleText != null)
            {
                subtitleText.text = "";
            }
            
            if (cutsceneAudioSource != null)
            {
                cutsceneAudioSource.Stop();
            }
            
            isPlayingCutscene = false;
        }
    }

    public bool IsPlayingCutscene()
    {
        return isPlayingCutscene;
    }

    public void SetPlayerController(PlayerFPSController controller)
    {
        playerController = controller;
    }

    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }
}

// ScriptRole: Manages cutscenes, camera control and narrative sequences
// Dependencies: Canvas, TextMeshProUGUI, Image, AudioSource
// UsesSO: FeedbackMessagesSO
// ReceivesFrom: GameManager, SceneManager, PlayerFPSController
// SendsTo: PlayerFPSController (disables movement during cutscenes) 