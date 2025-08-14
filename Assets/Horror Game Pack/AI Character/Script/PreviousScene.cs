using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreviousScene : MonoBehaviour
{
    public int sceneId;
    public GameObject LoadingScreen;
    public Image LoadingBarFill;
    AudioSource Music;
    private GameObject Volume;

    [Tooltip("The amount of time to wait before loading the scene")]
    public AnimationClip JumpScareAnimation;

    void Start()
    {
        Music = GameObject.FindWithTag("Music").GetComponent<AudioSource>();
        Volume = GameObject.FindWithTag("Volume");
        StartCoroutine(Load());
    }
    IEnumerator Load()
    {
        yield return new WaitForSeconds(JumpScareAnimation.length);
        Destroy(Music.gameObject);
        Destroy(GameObject.FindWithTag("Volume"));

        LoadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            LoadingBarFill.fillAmount = progressValue;

            yield return null;
        }
        Cursor.lockState = CursorLockMode.None;
    }
}
