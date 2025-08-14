using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenSwitcher : MonoBehaviour
{
    public List<FlickeringLight> flickeringLights = new List<FlickeringLight>();
    public List<Light> lights = new List<Light>();
    public bool isGeneratorOn = false;

    void Update()
    {
        if(!isGeneratorOn)
        {
            foreach(var light in lights)
            {
                light.enabled = false;
            }
            foreach(var flickerlight in flickeringLights)
            {
                flickerlight.enabled = false;
                flickerlight.gamestarted = false;
                flickerlight.SetLightsAndMaterials(false);
            }
        }
        else
        {
            foreach (var light in lights)
            {
                light.enabled = true;
            }
            foreach (var flickerlight in flickeringLights)
            {
                flickerlight.enabled = true;
                flickerlight.gamestarted = true;
                StartCoroutine(lighton());
            }
        }
    }

    IEnumerator lighton()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        foreach (var flickerlight in flickeringLights)
            flickerlight.SetLightsAndMaterials(true);
    }
}
