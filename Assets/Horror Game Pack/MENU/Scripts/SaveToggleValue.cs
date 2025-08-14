using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveToggleValue : MonoBehaviour
{
    public Toggle Toggle;

    private void Start()
    {
        
        if ((PlayerPrefs.GetInt("ToggleBool") == 1))
        {
            Toggle.isOn = true;
        }
        else
        {
            Toggle.isOn = false;
        }
    }
    private void Update()
    {
        if (Toggle.isOn == true)
        {
            PlayerPrefs.SetInt("ToggleBool", 1);
        }
        else
        {
            PlayerPrefs.SetInt("ToggleBool", 0);
        }
    }
}
