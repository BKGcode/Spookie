using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{
    public List<Light> LampLights = new List<Light>();
    public Material lampMaterial;
    public bool isGeneratorOn = false;
    public bool IsLampOn = false;
    public AudioSource lampOn;
    public AudioSource lampOff;
    public GameObject GeneratorOffMessageCanvas;
    [SerializeField] public InspectController inspectController;
    [SerializeField] public string ItemName;

    private bool isGeneratorOffMessageActive = false;

    private Material lampMaterialInstance;
    private Renderer lampRenderer;

    private void Awake()
    {
        lampMaterialInstance = new Material(lampMaterial);

        lampRenderer = GetComponent<Renderer>();
        if (lampRenderer != null)
        {
            lampRenderer.material = lampMaterialInstance;
        }
    }

    public void ShowObjectName()
    {
        inspectController.ShowName(ItemName);
    }

    public void HideObjectName()
    {
        inspectController.HideName();
    }

    private void Start()
    {
        if (IsLampOn && isGeneratorOn)
        {
            foreach (var lamplights in LampLights)
            {
                lamplights.enabled = true;
            }
            lampMaterialInstance.EnableKeyword("_EMISSION");
            IsLampOn = true;
        }
        else
        {
            foreach (var lamplights in LampLights)
            {
                lamplights.enabled = false;
            }
            lampMaterialInstance.DisableKeyword("_EMISSION");
            IsLampOn = false;
        }
    }

    private void Update()
    {
        if (isGeneratorOn && IsLampOn)
        {
            foreach (var lamplights in LampLights)
            {
                lamplights.enabled = true;
            }
            lampMaterialInstance.EnableKeyword("_EMISSION");
        }

        if (!isGeneratorOn)
        {
            foreach (var lamplights in LampLights)
            {
                lamplights.enabled = false;
            }
            lampMaterialInstance.DisableKeyword("_EMISSION");
        }
    }

    public void LampToggle()
    {
        if (!IsLampOn)
        {
            if (isGeneratorOn)
            {
                foreach (var lamplights in LampLights)
                {
                    lamplights.enabled = true;
                }
                lampMaterialInstance.EnableKeyword("_EMISSION");
            }
            IsLampOn = true;
            lampOn.Play();
        }
        else
        {
            if (isGeneratorOn)
            {
                foreach (var lamplights in LampLights)
                {
                    lamplights.enabled = false;
                }
                lampMaterialInstance.DisableKeyword("_EMISSION");
            }
            IsLampOn = false;
            lampOff.Play();
        }

        if (!isGeneratorOn && !isGeneratorOffMessageActive)
        {
            StartCoroutine(GeneratorOffMessage());
        }
    }

    IEnumerator GeneratorOffMessage()
    {
        isGeneratorOffMessageActive = true;
        GeneratorOffMessageCanvas.SetActive(true);
        yield return new WaitForSecondsRealtime(4.5f);
        GeneratorOffMessageCanvas.SetActive(false);
        isGeneratorOffMessageActive = false;
    }
}