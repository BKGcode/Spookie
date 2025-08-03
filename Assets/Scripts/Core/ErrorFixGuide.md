# Guía de Solución de Errores - Spookie

## **🐛 Errores Encontrados y Soluciones**

### **1. Error: DontDestroyOnLoad solo funciona con GameObjects raíz**

**Problema:**
```
DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
```

**Causa:** Los managers están siendo creados como hijos de otros GameObjects.

**Solución:**
1. **Crear managers como GameObjects raíz:**
   - GameManager debe estar en la raíz de la escena
   - AudioManager debe estar en la raíz de la escena
   - CutsceneManager debe estar en la raíz de la escena

2. **Modificar PrefabCreator para crear managers como raíz:**
   ```csharp
   // En lugar de:
   gameManager.transform.SetParent(managers.transform);
   
   // Usar:
   // gameManager se queda como GameObject raíz
   ```

### **2. Error: NullReferenceException en AudioManager y CutsceneManager**

**Problema:**
```
NullReferenceException: Object reference not set to an instance of an object
AudioManager.InitializeSoundDictionary()
```

**Causa:** Los managers están intentando inicializar diccionarios sin FeedbackMessagesSO asignado.

**Solución:**
1. **Asignar FeedbackMessagesSO antes de inicializar:**
   ```csharp
   // En AudioManager y CutsceneManager:
   if (feedbackMessages == null)
   {
       Debug.LogWarning("AudioManager: No FeedbackMessagesSO assigned!");
       return;
   }
   ```

2. **Usar SceneInitializer para configurar managers correctamente**

### **3. Error: Tags no definidos**

**Problema:**
```
Tag: WinZone is not defined.
Tag: DeathZone is not defined.
```

**Solución:**
1. **Crear tags en Unity:**
   - Edit → Project Settings → Tags and Layers
   - Añadir tags: "Player", "Hazard", "Enemy", "WinZone", "DeathZone"

2. **O modificar PlayerFPSController para manejar tags faltantes:**
   ```csharp
   private void OnTriggerEnter(Collider other)
   {
       if (other.CompareTag("WinZone"))
       {
           // Handle win
       }
       else if (other.CompareTag("DeathZone"))
       {
           // Handle death
       }
       // Add null checks
   }
   ```

### **4. Error: FogMaterial no asignado**

**Problema:**
```
FogEffect: No fog material assigned!
```

**Solución:**
1. **Crear FogMaterial:**
   - Click derecho en Project → Create → Material
   - Nombrar como "FogMaterial"
   - En Shader, seleccionar "Custom/FogShader"

2. **Asignar a FogEffect:**
   - Seleccionar la cámara del jugador
   - En FogEffect, arrastrar FogMaterial al campo `fogMaterial`

## **🔧 Configuración Correcta de la Escena**

### **Paso 1: Crear Tags**

1. **Edit → Project Settings → Tags and Layers**
2. **Añadir tags:**
   - Player
   - Hazard
   - Enemy
   - WinZone
   - DeathZone

### **Paso 2: Crear FogMaterial**

1. **Click derecho en Project → Create → Material**
2. **Nombrar como "FogMaterial"**
3. **En Shader, seleccionar "Custom/FogShader"**
4. **Configurar propiedades:**
   - Fog Color: (0.1f, 0.1f, 0.2f, 0.8f)
   - Fog Intensity: 0f
   - Fog Distance: 10f

### **Paso 3: Configurar Managers como GameObjects Raíz**

1. **Crear GameManager como GameObject raíz:**
   - GameObject vacío llamado "GameManager"
   - Añadir componente `GameManager`
   - Asignar FeedbackMessagesSO

2. **Crear AudioManager como GameObject raíz:**
   - GameObject vacío llamado "AudioManager"
   - Añadir componente `AudioManager`
   - Asignar FeedbackMessagesSO

3. **Crear CutsceneManager como GameObject raíz:**
   - GameObject vacío llamado "CutsceneManager"
   - Añadir componente `CutsceneManager`
   - Asignar FeedbackMessagesSO

### **Paso 4: Usar SceneInitializer**

1. **Crear GameObject vacío llamado "SceneInitializer"**
2. **Añadir componente `SceneInitializer`**
3. **Asignar FeedbackMessagesSO**
4. **Marcar `setupOnStart = true`**

## **🎯 Configuración Rápida**

### **Script de Configuración Automática:**

```csharp
// Crear este script temporal para configurar todo automáticamente
public class QuickSetup : MonoBehaviour
{
    public FeedbackMessagesSO feedbackMessages;
    
    private void Start()
    {
        SetupScene();
    }
    
    private void SetupScene()
    {
        // Crear managers como raíz
        CreateManager("GameManager", typeof(GameManager));
        CreateManager("AudioManager", typeof(AudioManager));
        CreateManager("CutsceneManager", typeof(CutsceneManager));
        
        // Asignar FeedbackMessagesSO
        AssignFeedbackMessages();
        
        // Crear FogMaterial
        CreateFogMaterial();
    }
    
    private void CreateManager(string name, System.Type componentType)
    {
        GameObject obj = new GameObject(name);
        obj.AddComponent(componentType);
    }
    
    private void AssignFeedbackMessages()
    {
        // Asignar a todos los managers
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.feedbackMessages = feedbackMessages;
        
        AudioManager am = FindObjectOfType<AudioManager>();
        if (am != null) am.feedbackMessages = feedbackMessages;
        
        CutsceneManager cm = FindObjectOfType<CutsceneManager>();
        if (cm != null) cm.feedbackMessages = feedbackMessages;
    }
    
    private void CreateFogMaterial()
    {
        // Crear material de niebla
        Material fogMaterial = new Material(Shader.Find("Custom/FogShader"));
        fogMaterial.SetColor("_FogColor", new Color(0.1f, 0.1f, 0.2f, 0.8f));
        fogMaterial.SetFloat("_FogIntensity", 0f);
        fogMaterial.SetFloat("_FogDistance", 10f);
        
        // Asignar a DayNightCycle
        DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
        if (dnc != null) dnc.fogMaterial = fogMaterial;
    }
}
```

## **🐛 Verificación de Errores**

### **Checklist de Solución:**

- [ ] **Tags creados:** Player, Hazard, Enemy, WinZone, DeathZone
- [ ] **FogMaterial creado:** Con shader Custom/FogShader
- [ ] **Managers como raíz:** GameManager, AudioManager, CutsceneManager
- [ ] **FeedbackMessagesSO asignado:** En todos los managers
- [ ] **FogMaterial asignado:** En DayNightCycle
- [ ] **Referencias UI:** DayNightCycle y PlayerController en GameUI

### **Comandos de Verificación:**

```csharp
// Verificar managers
Debug.Log($"GameManager: {FindObjectOfType<GameManager>() != null}");
Debug.Log($"AudioManager: {FindObjectOfType<AudioManager>() != null}");
Debug.Log($"CutsceneManager: {FindObjectOfType<CutsceneManager>() != null}");

// Verificar FeedbackMessagesSO
GameManager gm = FindObjectOfType<GameManager>();
Debug.Log($"GameManager FeedbackMessagesSO: {gm?.feedbackMessages != null}");

// Verificar FogMaterial
DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
Debug.Log($"DayNightCycle FogMaterial: {dnc?.fogMaterial != null}");
```

## **🎮 Configuración Final**

### **Orden de Configuración:**

1. **Crear FeedbackMessagesSO y configurar mensajes**
2. **Crear tags en Project Settings**
3. **Crear FogMaterial**
4. **Crear managers como GameObjects raíz**
5. **Asignar FeedbackMessagesSO a todos los managers**
6. **Asignar FogMaterial a DayNightCycle**
7. **Configurar referencias UI**
8. **Probar funcionalidades**

### **Configuración de Prueba:**

```csharp
// En DayNightCycle para pruebas rápidas:
dayDuration = 60f;           // 1 minuto por día
sleepPressureStartTime = 10f; // Presión en 10 segundos
maxSlowdownFactor = 0.3f;    // Velocidad mínima 30%
fogIntensity = 1f;           // Niebla máxima
```

---

**¡Con estas correcciones, la escena debería funcionar correctamente sin errores!** 