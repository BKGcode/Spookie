using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switcher : MonoBehaviour
{
    public List<Light> RoomLight = new List<Light>();
    public Material lampMaterial;

    public AudioSource TurnOnLight;
    public AudioSource TurnOffLight;

    [SerializeField] public InspectController inspectController;
    [SerializeField] public string ItemName;

    public bool IsGeneratorOn = true;
    public bool IsLampOn = false;
    public bool IsLightALightBulb = false;
    public Animator SwitcherAnimator;
    [SerializeField] private string turnOnAnimation = "SwitcherOn";
    [SerializeField] private string turnOffAnimation = "SwitcherOff";
    public GameObject GeneratorOffMessageCanvas;

    public List<GameObject> LampGameObjectsWithEmission;

    private bool isGeneratorOffMessageActive = false;

    private List<Material> lampMaterialInstances = new List<Material>();

    private void Awake()
    {
        lampMaterialInstances.Clear();

        foreach (var lampGameObject in LampGameObjectsWithEmission)
        {
            Renderer lampRenderer = lampGameObject.GetComponent<Renderer>();
            if (lampRenderer != null)
            {
                Material[] materials = lampRenderer.materials;
                Material[] newMaterials = new Material[materials.Length];

                for (int i = 0; i < materials.Length; i++)
                {
                    Material lampMaterialInstance = new Material(materials[i]);
                    newMaterials[i] = lampMaterialInstance;
                    lampMaterialInstances.Add(lampMaterialInstance);
                }

                lampRenderer.materials = newMaterials;
            }
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

    private void Update()
    {
        if (IsGeneratorOn && IsLampOn)
        {
            EnableEmission(true);
            foreach (var light in RoomLight)
            {
                light.enabled = true;
            }
        }
        else
        {
            EnableEmission(false);
            foreach (var light in RoomLight)
            {
                light.enabled = false;
            }
        }
    }

    public void ToggleSwitcher()
    {
        if (IsLampOn == false)
        {
            SwitcherAnimator.Play(turnOnAnimation);
            IsLampOn = true;
            TurnOnLight.Play();
            if (IsGeneratorOn)
            {
                EnableEmission(true);
                foreach (var light in RoomLight)
                {
                    light.enabled = true;
                }
            }
        }
        else if (IsLampOn == true)
        {
            SwitcherAnimator.Play(turnOffAnimation);
            IsLampOn = false;
            TurnOffLight.Play();
            if (IsGeneratorOn)
            {
                EnableEmission(false);
                foreach (var light in RoomLight)
                {
                    light.enabled = false;
                }
            }
        }
        if (!IsGeneratorOn && !isGeneratorOffMessageActive)
        {
            StartCoroutine(GeneratorOffMessage());
        }
    }

    private void EnableEmission(bool enable)
    {
        foreach (var lampMaterialInstance in lampMaterialInstances)
        {
            if (enable)
            {
                lampMaterialInstance.EnableKeyword("_EMISSION");
            }
            else
            {
                lampMaterialInstance.DisableKeyword("_EMISSION");
            }
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