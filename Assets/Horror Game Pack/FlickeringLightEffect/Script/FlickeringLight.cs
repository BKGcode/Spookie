using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlickeringLight : MonoBehaviour
{
    public List<Light> lightSources = new List<Light>();
    public AudioSource audioSource;
    public AudioClip flickerSound;

    public float minFlickerDuration = 0.05f;
    public float maxFlickerDuration = 0.2f;
    public float minDelayTime = 2.0f;
    public float maxDelayTime = 5.0f;

    public List<Renderer> lampRenderers = new List<Renderer>();
    private List<Material> lampMaterialInstances = new List<Material>();
    [HideInInspector]public bool gamestarted = true;


    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null && flickerSound != null)
        {
            audioSource.clip = flickerSound;
            audioSource.loop = true;
            audioSource.Play();
            audioSource.volume = 0;
        }

        InitializeMaterials();
        StartCoroutine(FlickerLight());
    }

    private void InitializeMaterials()
    {
        foreach (var renderer in lampRenderers)
        {
            if (renderer != null)
            {
                Material[] materials = renderer.materials;
                foreach (var material in materials)
                {
                    Material lampMaterialInstance = new Material(material);
                    lampMaterialInstances.Add(lampMaterialInstance);
                }

                renderer.materials = lampMaterialInstances.ToArray();
            }
        }
    }

    private IEnumerator FlickerLight()
    {
        if (gamestarted)
        {
            while (true)
            {
                float delayTime = Random.Range(minDelayTime, maxDelayTime);
                yield return new WaitForSeconds(delayTime);

                float flickerDuration = Random.Range(minFlickerDuration, maxFlickerDuration);

                yield return StartCoroutine(PerformFlicker(flickerDuration));
            }
        }
    }

    private IEnumerator PerformFlicker(float duration)
    {
        if (gamestarted)
        {
            float endTime = Time.time + duration;

            if (audioSource != null)
            {
                audioSource.volume = 1;
            }

            bool isFlickering = true;

            while (Time.time < endTime)
            {
                ToggleLightsAndMaterials();

                yield return new WaitForSeconds(0.05f);
            }

            SetLightsAndMaterials(true);

            if (audioSource != null)
            {
                audioSource.volume = 0;
            }
        }
    }

    private void ToggleLightsAndMaterials()
    {
        foreach (var light in lightSources)
        {
            light.enabled = !light.enabled;
        }

        foreach (var material in lampMaterialInstances)
        {
            if (material.IsKeywordEnabled("_EMISSION"))
            {
                material.DisableKeyword("_EMISSION");
            }
            else
            {
                material.EnableKeyword("_EMISSION");
            }
        }
    }

    public void SetLightsAndMaterials(bool enable)
    {
        foreach (var light in lightSources)
        {
            light.enabled = enable;
        }

        foreach (var material in lampMaterialInstances)
        {
            if (enable)
            {
                material.EnableKeyword("_EMISSION");
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
        }
    }
}