using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class Menu : MonoBehaviour
{
    [SerializeField] private Animator myDoor = null;
    [SerializeField] private string dooranim = "DoorAnim";
    public GameObject MainMenu;
    public GameObject OptionsMenu;
    public GameObject AudioMenu;
    public GameObject GraphicsMenu;
    public GameObject LoadingScreen;
    public GameObject QuestionMarkScreen;
    public Image LoadingBarFill;
    public TMPro.TMP_Dropdown resulutionDropdown;

    [Tooltip ("This changes the volume of every sound in the whole game.")]
    [SerializeField] Slider VolumeSlider;
    [Tooltip ("This changes the volume of the in-game music.")]
    [SerializeField] Slider MusicSlider;
    public AudioSource Music;
    Resolution[] resulutions;

    public bool InGameisProcess;
    public GameObject PostVolume;
    public PostProcessVolume InGamePostVolume;

    #region MasterVolume and Resolution and fullscreen
    void Start()
    {
        #region Resolution
        resulutions = Screen.resolutions;
        resulutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resulutions.Length; i++)
        {
            string option = resulutions[i].width + " x " + resulutions[i].height + " @ " + resulutions[i].refreshRate + "hz";
            options.Add(option);

            if (resulutions[i].width == Screen.currentResolution.width && resulutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resulutionDropdown.AddOptions(options);
        resulutionDropdown.value = currentResolutionIndex;
        resulutionDropdown.RefreshShownValue();
        #endregion
        Music.enabled = false;
        DontDestroyOnLoad(Music);
        DontDestroyOnLoad(InGamePostVolume);
        DontDestroyOnLoad(PostVolume);
        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1);
            Load();
        }

        else
        {
            Load();
        }

        if (!PlayerPrefs.HasKey("musicSlider"))
        {
            PlayerPrefs.SetFloat("musicSlider", 1);
            LoadMusic();
        }

        else
        {
            LoadMusic();
        }
    }

    public void ChangeVolume()
    {
        AudioListener.volume = VolumeSlider.value;
        Save();
    }

    public void ChangeMusicVol()
    {
        Music.volume = MusicSlider.value;
        SaveMusic();
    }

    private void Load()
    {
        VolumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
    }
    private void LoadMusic()
    {
        MusicSlider.value = PlayerPrefs.GetFloat("musicSlider");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", VolumeSlider.value);
    }
    private void SaveMusic()
    {
        PlayerPrefs.SetFloat("musicSlider", MusicSlider.value);
    } 
    #endregion

    public void Play(int sceneId)
    {
        myDoor.Play(dooranim, 0, 0.0f);
        StartCoroutine(Wait(sceneId));
    }
    IEnumerator Wait(int sceneId)
    {
        yield return new WaitForSecondsRealtime(0.71f);
        Music.enabled = true;
        Destroy(PostVolume);
        if (InGameisProcess == true)
        {
            InGamePostVolume.enabled = true;
        }
        else
        {
            InGamePostVolume.enabled = false;
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        LoadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            LoadingBarFill.fillAmount = progressValue;

            yield return null;
        }
    }

    public void Quit()
    {
        StartCoroutine(Wait2());
    }

    IEnumerator Wait2()
    {
        yield return new WaitForSecondsRealtime(0.4f);
        Application.Quit();
        Debug.Log("QUIT!");
    }
    public void OptionMenu()
    {
        StartCoroutine(Wait3());
    }
    IEnumerator Wait3()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OptionsMenu.SetActive(true);
        MainMenu.SetActive(false);
    }
    public void Back()
    {
        StartCoroutine(Wait4());
    }
    IEnumerator Wait4()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OptionsMenu.SetActive(false);
        MainMenu.SetActive(true);
    }
    public void Audio()
    {
        StartCoroutine(Wait5());
    }
    IEnumerator Wait5()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OptionsMenu.SetActive(false);
        AudioMenu.SetActive(true);
    }
    public void Back2()
    {
        StartCoroutine(Wait6());
    }
    IEnumerator Wait6()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OptionsMenu.SetActive(true);
        AudioMenu.SetActive(false);
    }
    public void Graphics()
    {
        StartCoroutine(Wait7());
    }
    IEnumerator Wait7()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        GraphicsMenu.SetActive(true);
        OptionsMenu.SetActive(false);
    }
    public void Back3()
    {
        StartCoroutine(Wait8());
    }
    IEnumerator Wait8()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        GraphicsMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resulutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetPostProcessing(bool isProcess)
    {
        if (isProcess == true)
        {
            InGameisProcess = true;
            PostVolume.SetActive(true);
        }
        if (isProcess == false)
        {
            InGameisProcess = false;
            PostVolume.SetActive(false);
        }
    }
    public void QuestionMark()
    {
        MainMenu.SetActive(false);
        QuestionMarkScreen.SetActive(true);
    }
    public void Back4()
    {
        StartCoroutine(Wait9());
    }
    IEnumerator Wait9()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        MainMenu.SetActive(true);
        QuestionMarkScreen.SetActive(false);
    }
}
