using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class ResolutionMenu : MonoBehaviour
{
    public TMPro.TMP_Dropdown resulutionDropdown;

    Resolution[] resulutions;

    #region resolution
    void Start()
    {
        #region Resulution
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
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resulutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    #endregion
}