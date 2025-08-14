using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]

public class SaveDropDownvalue : MonoBehaviour
{
    const string PrefName = "optionvalue";

    private TMPro.TMP_Dropdown _dropdown;

    void Awake()
    {
        _dropdown = GetComponent<TMPro.TMP_Dropdown>();

        _dropdown.onValueChanged.AddListener(new UnityAction<int>(index =>
        {
            PlayerPrefs.SetInt(PrefName, _dropdown.value);
            PlayerPrefs.Save();
        }));
    }

    void Start()
    {
        _dropdown.value = PlayerPrefs.GetInt(PrefName, 0);
    }
}

