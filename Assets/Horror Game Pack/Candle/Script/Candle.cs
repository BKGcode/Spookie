using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Candle : MonoBehaviour
{
    #region Parameters
    [SerializeField] KeyCode toggleKey = KeyCode.L;

    #endregion

    #region References 

    [SerializeField] AudioClip onSound = null;
    [SerializeField] AudioClip offSound = null;

    [SerializeField] GameObject candle = null;

    #endregion

    #region Statistics

    [HideInInspector]
    [SerializeField]
    public bool usingCandle = false;

    #endregion

    #region Private and Properties

    AudioSource _source = null;
    AudioSource Source
    {
        get
        {
            if (_source == null)
            {
                _source = GetComponent<AudioSource>();
                if (_source == null) { _source = gameObject.AddComponent<AudioSource>(); }
                _source.playOnAwake = false;
            }
            return _source;
        }
    }

    #endregion 

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleCandle(!usingCandle, true);
        }
    }

    public void ToggleCandle(bool state, bool playSound)
    {
        usingCandle = state;
        candle.SetActive(state);

        ToggleObject(state);

        if (playSound)
        {
            PlaySFX(usingCandle ? onSound : offSound);
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) { return; }

        Source.clip = clip;
        Source.Play();
    }

    void ToggleObject(bool state)
    {
        
    }
}

