using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public AudioClip KeyTakeOut;
    public AudioClip KeyTakeBack;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void KeyTakeOutSound()
    {
        audioSource.clip = KeyTakeOut;
        audioSource.Play();
    }

    public void KeyTakeBackSound()
    {
        audioSource.clip = KeyTakeBack;
        audioSource.Play();
    }
}
