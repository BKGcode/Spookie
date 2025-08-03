# Guía de Prefabs Modulares - Spookie

## **🎮 Sistema de Prefabs Modulares**

### **📋 Prefabs Disponibles**

#### **🏃‍♂️ Player.prefab**
- **Ubicación:** `Assets/Prefabs/Player/Player.prefab`
- **Contenido:**
  - Player GameObject con CharacterController
  - PlayerFPSController script
  - PlayerCamera con Camera y FogEffect
  - Configurado para movimiento FPS
- **Uso:** Arrastrar a la escena para añadir jugador

#### **🛏️ Bed.prefab**
- **Ubicación:** `Assets/Prefabs/Environment/Bed.prefab`
- **Contenido:**
  - BedBase y Mattress (Cubes)
  - BedInteractable script
  - BoxCollider para interacción
  - Posiciones de sueño configuradas
- **Uso:** Arrastrar a la escena para añadir cama interactiva

#### **🏠 House.prefab**
- **Ubicación:** `Assets/Prefabs/Environment/House.prefab`
- **Contenido:**
  - 4 paredes (Walls)
  - Techo (Roof)
  - Puerta (Door)
  - Estructura básica de casa
- **Uso:** Arrastrar a la escena para añadir casa

#### **🌍 Ground.prefab**
- **Ubicación:** `Assets/Prefabs/Environment/Ground.prefab`
- **Contenido:**
  - Plane escalado (20x20)
  - BoxCollider para física
  - Superficie de juego
- **Uso:** Arrastrar a la escena para añadir suelo

#### **🎛️ GameManagers.prefab**
- **Ubicación:** `Assets/Prefabs/Managers/GameManagers.prefab`
- **Contenido:**
  - GameManager
  - DayNightCycle (configurado para pruebas)
  - RespawnSystem
  - AudioManager con 4 AudioSources
  - CutsceneManager
- **Uso:** Arrastrar a la escena para añadir todos los managers

#### **🖥️ GameUI.prefab**
- **Ubicación:** `Assets/Prefabs/UI/GameUI.prefab`
- **Contenido:**
  - Canvas con CanvasScaler
  - UIManager
  - GameUI script
  - 4 TextMeshPro elementos (Día/Noche, Presión, Tiempo, Interacción)
- **Uso:** Arrastrar a la escena para añadir UI completa

#### **🔊 AudioSources.prefab**
- **Ubicación:** `Assets/Prefabs/Audio/AudioSources.prefab`
- **Contenido:**
  - 4 AudioSources (Music, SFX, Ambient, Voice)
  - Configurados con volúmenes y loops
- **Uso:** Arrastrar a la escena para añadir sistema de audio

#### **💡 Lighting.prefab**
- **Ubicación:** `Assets/Prefabs/Lighting/Lighting.prefab`
- **Contenido:**
  - DirectionalLight configurada
  - Posición y rotación optimizadas
  - Sombras suaves
- **Uso:** Arrastrar a la escena para añadir iluminación

---

## **🚀 Configuración Rápida**

### **Paso 1: Generar Prefabs**
1. **Crear GameObject vacío llamado "PrefabGenerator"**
2. **Añadir componente `PrefabGenerator`**
3. **Asignar FeedbackMessagesSO**
4. **Click derecho → Generate All Prefabs**

### **Paso 2: Crear Escena**
1. **File → New Scene**
2. **Guardar como "TestScene"**

### **Paso 3: Añadir Prefabs**
1. **Arrastrar `GameManagers.prefab`** (contiene todos los managers)
2. **Arrastrar `Lighting.prefab`** (iluminación)
3. **Arrastrar `Ground.prefab`** (suelo)
4. **Arrastrar `House.prefab`** (casa)
5. **Arrastrar `Bed.prefab`** (cama)
6. **Arrastrar `Player.prefab`** (jugador)
7. **Arrastrar `GameUI.prefab`** (interfaz)
8. **Arrastrar `AudioSources.prefab`** (audio)

### **Paso 4: Configurar Referencias**
1. **En DayNightCycle:**
   - Asignar `directionalLight` (del Lighting prefab)
   - Asignar `fogMaterial` (crear FogMaterial)
2. **En GameUI:**
   - Asignar `dayNightCycle` (del GameManagers)
   - Asignar `playerController` (del Player)
3. **En AudioManager:**
   - Asignar los 4 AudioSources (del AudioSources prefab)

---

## **🎯 Configuración Detallada**

### **Posiciones Recomendadas**
```
Ground: Position (0, 0, 0)
House: Position (0, 0, 0)
Bed: Position (0, 0, 0) - dentro de la casa
Player: Position (0, 1, 0) - fuera de la casa
GameManagers: Position (0, 0, 0) - invisible
Lighting: Position (0, 10, 0)
GameUI: Position (0, 0, 0) - UI overlay
AudioSources: Position (0, 0, 0) - invisible
```

### **Configuración de Materiales**
1. **Crear FogMaterial:**
   - Shader: Custom/FogShader
   - Fog Color: (0.1f, 0.1f, 0.2f, 0.8f)
   - Fog Intensity: 0f
   - Fog Distance: 10f

2. **Materiales de la casa:**
   - Paredes: Color gris
   - Techo: Color marrón oscuro
   - Puerta: Color marrón

3. **Material del suelo:**
   - Color marrón

---

## **🔧 Configuración de Referencias**

### **Referencias Automáticas**
Los prefabs ya incluyen:
- ✅ FeedbackMessagesSO asignado
- ✅ Componentes configurados
- ✅ Jerarquías correctas
- ✅ Tags asignados

### **Referencias Manuales**
Solo necesitas asignar:
1. **DayNightCycle:**
   - `directionalLight` → DirectionalLight del Lighting prefab
   - `fogMaterial` → FogMaterial creado

2. **GameUI:**
   - `dayNightCycle` → DayNightCycle del GameManagers
   - `playerController` → PlayerFPSController del Player

3. **AudioManager:**
   - `musicSource` → MusicSource del AudioSources
   - `sfxSource` → SFXSource del AudioSources
   - `ambientSource` → AmbientSource del AudioSources
   - `voiceSource` → VoiceSource del AudioSources

---

## **🎮 Configuración de Prueba**

### **Configuración Rápida (1 minuto)**
```csharp
// En DayNightCycle:
dayDuration = 60f;           // 1 minuto por día
sleepPressureStartTime = 10f; // Presión en 10 segundos
maxSlowdownFactor = 0.3f;    // Velocidad mínima 30%
fogIntensity = 1f;           // Niebla máxima
```

### **Configuración Normal (5 minutos)**
```csharp
// En DayNightCycle:
dayDuration = 300f;          // 5 minutos por día
sleepPressureStartTime = 240f; // Presión en 4 minutos
maxSlowdownFactor = 0.5f;    // Velocidad mínima 50%
fogIntensity = 0.8f;         // Niebla moderada
```

---

## **📊 Verificación de Setup**

### **Comandos de Verificación**
```csharp
// Verificar prefabs generados
FindObjectOfType<PrefabGenerator>().VerifyPrefabs();

// Verificar managers
Debug.Log($"GameManager: {FindObjectOfType<GameManager>() != null}");
Debug.Log($"AudioManager: {FindObjectOfType<AudioManager>() != null}");
Debug.Log($"CutsceneManager: {FindObjectOfType<CutsceneManager>() != null}");

// Verificar referencias
DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
Debug.Log($"DayNightCycle FogMaterial: {dnc?.fogMaterial != null}");
Debug.Log($"DayNightCycle DirectionalLight: {dnc?.directionalLight != null}");
```

### **Checklist de Verificación**
- [ ] **Prefabs generados** en Assets/Prefabs/
- [ ] **GameManagers** en la escena
- [ ] **Player** con CharacterController
- [ ] **House** con paredes y techo
- [ ] **Bed** con BedInteractable
- [ ] **Ground** con Plane
- [ ] **Lighting** con DirectionalLight
- [ ] **GameUI** con Canvas y TextMeshPro
- [ ] **AudioSources** con 4 AudioSources
- [ ] **Referencias asignadas** (DayNightCycle, GameUI)

---

## **🎯 Ventajas del Sistema de Prefabs**

### **✅ Beneficios**
- **Configuración rápida:** Arrastrar y soltar
- **Modularidad:** Cada prefab es independiente
- **Reutilización:** Usar en múltiples escenas
- **Mantenimiento:** Cambios centralizados
- **Consistencia:** Misma configuración siempre

### **🔧 Personalización**
- **Assets 3D:** Reemplazar Cubes con modelos 3D
- **Materiales:** Aplicar texturas y shaders
- **Posiciones:** Ajustar según necesidades
- **Configuración:** Modificar parámetros en Inspector

---

## **🐛 Solución de Problemas**

### **Error: "Prefab not found"**
- **Solución:** Ejecutar PrefabGenerator primero

### **Error: "Missing references"**
- **Solución:** Asignar referencias manualmente

### **Error: "Component missing"**
- **Solución:** Verificar que todos los scripts están en Scripts/

### **Error: "Tags not defined"**
- **Solución:** Crear tags en Project Settings

---

## **🎮 Uso Final**

### **Orden de Implementación:**
1. **Generar prefabs** con PrefabGenerator
2. **Crear escena vacía**
3. **Arrastrar prefabs** en orden recomendado
4. **Asignar referencias** faltantes
5. **Configurar materiales** y assets 3D
6. **Probar funcionalidades**

### **Comandos de Prueba:**
```csharp
// Saltar a la noche
FindObjectOfType<DayNightCycle>().SkipToNight();

// Cambiar duración del día
FindObjectOfType<DayNightCycle>().dayDuration = 30f;

// Verificar estado
Debug.Log($"GameManager: {FindObjectOfType<GameManager>() != null}");
```

---

**¡Con este sistema de prefabs modulares, puedes crear escenas completas en minutos!** 