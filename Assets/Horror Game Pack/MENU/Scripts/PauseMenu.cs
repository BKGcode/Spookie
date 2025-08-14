using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public MouseLook mouselook;
    public GameObject OptionsMenu;
    public GameObject AudioMenu;
    public GameObject GraphicsMenu;
    public GameObject MainMenu;
    [SerializeField] Slider VolumeSlider;
    [SerializeField] Slider MusicSlider;
    AudioSource Music;

    public bool MenuGameisProcess;
    private GameObject Volume;
    [SerializeField] KeyCode togglekey = KeyCode.Escape;

    void Start()
    {
        GameIsPaused = false;
        Music = GameObject.FindWithTag("Music").GetComponent<AudioSource>();
        Volume = GameObject.FindWithTag("Volume");
        pauseMenuUI.SetActive(false);
        
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
    public void Menu()
    {
        StartCoroutine(WaitScene());
    }
    IEnumerator WaitScene()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        Destroy(Music.gameObject);
        Destroy(GameObject.FindWithTag("Volume"));
        Time.timeScale = 1f;
        SceneManager.LoadScene("Demo");
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

    public void Update()
    {
        if (Input.GetKeyDown(togglekey))
        {
            if (GameIsPaused == true)
            {
                Resume();
            }
            else if (GameIsPaused == false)
            {
                Pause();
            }
        }
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        mouselook.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
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

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        mouselook.enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGamee()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}
