using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FogEffect : MonoBehaviour
{
    [Header("Fog Settings")]
    public Material fogMaterial;
    public Color dayFogColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
    public Color nightFogColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
    public float fogDistance = 10f;
    
    [Header("References")]
    public DayNightCycle dayNightCycle;
    
    private Camera cam;
    private RenderTexture renderTexture;
    private Material fogMat;

    private void Start()
    {
        Debug.Log("FogEffect: Initializing");
        
        cam = GetComponent<Camera>();
        
        // Find day/night cycle if not assigned
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        }
        
        // Create fog material if not assigned
        if (fogMaterial != null)
        {
            fogMat = new Material(fogMaterial);
        }
        else
        {
            Debug.LogWarning("FogEffect: No fog material assigned!");
        }
        
        // Initialize fog
        UpdateFogSettings();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fogMat != null)
        {
            // Update fog settings based on day/night cycle
            UpdateFogSettings();
            
            // Apply fog effect
            Graphics.Blit(source, destination, fogMat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    private void UpdateFogSettings()
    {
        if (fogMat == null) return;
        
        // Get current fog color based on day/night cycle
        Color currentFogColor = dayFogColor;
        float fogIntensity = 0f;
        
        if (dayNightCycle != null)
        {
            if (dayNightCycle.IsNightTime())
            {
                currentFogColor = nightFogColor;
                fogIntensity = dayNightCycle.GetSleepPressure();
            }
        }
        
        // Apply settings to material
        fogMat.SetColor("_FogColor", currentFogColor);
        fogMat.SetFloat("_FogIntensity", fogIntensity);
        fogMat.SetFloat("_FogDistance", fogDistance);
    }

    public void SetFogIntensity(float intensity)
    {
        if (fogMat != null)
        {
            fogMat.SetFloat("_FogIntensity", Mathf.Clamp01(intensity));
        }
    }

    public void SetFogColor(Color color)
    {
        if (fogMat != null)
        {
            fogMat.SetColor("_FogColor", color);
        }
    }

    public void SetFogDistance(float distance)
    {
        fogDistance = distance;
        if (fogMat != null)
        {
            fogMat.SetFloat("_FogDistance", distance);
        }
    }

    private void OnDestroy()
    {
        if (fogMat != null)
        {
            DestroyImmediate(fogMat);
        }
    }
}

// ScriptRole: Applies fog effect to camera using custom shader
// Dependencies: Camera, Material
// UsesSO: None
// ReceivesFrom: DayNightCycle
// SendsTo: None (applies visual effect) 