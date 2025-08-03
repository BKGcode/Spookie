# Guía de Configuración de Escena - Spookie

## **Paso 1: Crear FeedbackMessagesSO**

1. **Crear ScriptableObject:**
   - Click derecho en Project → Create → Spookie → Feedback Messages
   - Nombrar como "FeedbackMessages"

2. **Configurar mensajes:**
   - Seleccionar el asset creado
   - En Inspector, añadir mensajes:
     - `go_to_bed_message`: "Es hora de ir a la cama. Te sientes cansado..."
     - `sleeping_message`: "Duermes profundamente..."
     - `sleep_pressure_message`: "Te sientes cada vez más cansado. Necesitas descansar."
     - `sleep_prompt`: "Presiona E para dormir"
     - `not_sleep_time_message`: "No es hora de dormir aún"
     - `respawning_message`: "Respawneando..."

## **Paso 2: Crear Material de Niebla**

1. **Crear material:**
   - Click derecho en Project → Create → Material
   - Nombrar como "FogMaterial"
   - En Shader, seleccionar "Custom/FogShader"

2. **Configurar propiedades:**
   - Fog Color: (0.1, 0.1, 0.2, 0.8) para noche
   - Fog Intensity: 0 (se ajusta automáticamente)
   - Fog Distance: 10

## **Paso 3: Configurar Escena**

### **Opción A: Configuración Automática**

1. **Crear GameObject vacío:**
   - Nombrar como "SceneSetup"
   - Añadir componente `SceneSetup`

2. **Configurar SceneSetup:**
   - Asignar `FeedbackMessagesSO` creado
   - Asignar `FogMaterial` creado
   - Marcar `autoSetup = true`

3. **Ejecutar escena:**
   - La escena se configurará automáticamente

### **Opción B: Configuración Manual**

#### **3.1 Crear Terreno**
- Crear GameObject → 3D Object → Plane
- Escalar a (20, 1, 20)
- Añadir material de suelo

#### **3.2 Crear Casa**
- Crear GameObject vacío "House"
- Crear paredes con primitivos Cube
- Posicionar en (0, 0, 0)
- Añadir puerta en (0, 1, 5)

#### **3.3 Crear Cama**
- Crear GameObject "Bed" en (0, 0.5, 2)
- Añadir componentes:
  - `BedInteractable`
  - `BoxCollider` (isTrigger = true)
- Configurar:
  - `sleepPosition`: Transform de la cama
  - `wakePosition`: Transform de la cama
  - `feedbackMessages`: Referencia al SO

#### **3.4 Crear Jugador**
- Crear GameObject "Player"
- Añadir `CharacterController`
- Añadir `PlayerFPSController`
- Crear cámara hija "PlayerCamera"
- Añadir `FogEffect` a la cámara
- Configurar:
  - `cameraTransform`: Referencia a la cámara
  - `feedbackMessages`: Referencia al SO

#### **3.5 Crear Managers**
- **GameManager:**
  - Crear GameObject "GameManager"
  - Añadir componente `GameManager`
  - Asignar `feedbackMessages`

- **DayNightCycle:**
  - Crear GameObject "DayNightCycle"
  - Añadir componente `DayNightCycle`
  - Configurar:
    - `directionalLight`: Referencia a la luz
    - `fogMaterial`: Referencia al material de niebla
    - `feedbackMessages`: Referencia al SO

- **RespawnSystem:**
  - Crear GameObject "RespawnSystem"
  - Añadir componente `RespawnSystem`
  - Configurar:
    - `fallThreshold`: -5 (ajustar según terreno)
    - `feedbackMessages`: Referencia al SO

- **AudioManager:**
  - Crear GameObject "AudioManager"
  - Añadir componente `AudioManager`
  - Configurar `feedbackMessages`

- **CutsceneManager:**
  - Crear GameObject "CutsceneManager"
  - Añadir componente `CutsceneManager`
  - Configurar `feedbackMessages`

#### **3.6 Crear UI**
- Crear Canvas
- Añadir `UIManager`
- Crear elementos UI:
  - TextMeshPro para día/noche
  - TextMeshPro para interacciones
  - TextMeshPro para presión de sueño
  - TextMeshPro para tiempo

#### **3.7 Crear Iluminación**
- Crear GameObject "DirectionalLight"
- Añadir componente `Light`
- Configurar:
  - Type: Directional
  - Intensity: 1
  - Color: White
  - Rotation: (45, 45, 0)

## **Paso 4: Configurar Input**

1. **Verificar Input Manager:**
   - Edit → Project Settings → Input Manager
   - Asegurar que existen:
     - Horizontal (WASD)
     - Vertical (WASD)
     - Jump (Space)
     - Mouse X
     - Mouse Y

## **Paso 5: Configurar Tags**

1. **Crear tags necesarios:**
   - Edit → Project Settings → Tags and Layers
   - Añadir tags:
     - "Player"
     - "Hazard"
     - "Enemy"
     - "WinZone"
     - "DeathZone"

## **Paso 6: Configurar Layers**

1. **Crear layers necesarios:**
   - Edit → Project Settings → Tags and Layers
   - Añadir layers:
     - "Ground"
     - "Player"
     - "Interactable"

## **Paso 7: Configurar Audio**

1. **Configurar AudioManager:**
   - Asignar AudioSources para:
     - Música
     - SFX
     - Ambiente
     - Voz

2. **Crear clips de audio:**
   - Sonidos de pasos
   - Ambiente de día/noche
   - Sonidos de dormir/despertar

## **Paso 8: Probar Escena**

### **Controles:**
- **WASD:** Movimiento
- **Mouse:** Mirar alrededor
- **Space:** Saltar
- **Shift:** Correr
- **E:** Interactuar

### **Funcionalidades a Probar:**
1. **Movimiento del jugador**
2. **Ciclo día/noche** (5 minutos día, 1 minuto noche)
3. **Presión de sueño** (ralentización gradual)
4. **Niebla atmosférica** (aparece en la noche)
5. **Interacción con la cama**
6. **Sistema de respawn** (caer por debajo de Y=-5)
7. **UI en tiempo real**

### **Configuración de Prueba:**
- **Día rápido:** Cambiar `dayDuration = 60f` en DayNightCycle
- **Presión rápida:** Cambiar `sleepPressureStartTime = 10f`
- **Niebla intensa:** Cambiar `fogIntensity = 1f`

## **Paso 9: Optimización**

1. **Configurar Quality Settings:**
   - Edit → Project Settings → Quality
   - Ajustar según necesidades

2. **Configurar Graphics:**
   - Edit → Project Settings → Graphics
   - Asignar pipeline de renderizado

3. **Configurar Physics:**
   - Edit → Project Settings → Physics
   - Ajustar Layer Collision Matrix

## **Solución de Problemas**

### **Problema: Jugador no se mueve**
- Verificar que `CharacterController` está presente
- Verificar que `PlayerFPSController` está configurado
- Verificar que `GameManager.IsGamePlaying()` es true

### **Problema: Niebla no aparece**
- Verificar que `FogEffect` está en la cámara
- Verificar que `FogMaterial` está asignado
- Verificar que `DayNightCycle` está configurado

### **Problema: No se puede interactuar con la cama**
- Verificar que `BedInteractable` tiene `BoxCollider`
- Verificar que `isTrigger = true`
- Verificar que `DayNightCycle.IsCycleComplete()` es true

### **Problema: UI no se actualiza**
- Verificar que `GameUI` está configurado
- Verificar que `TextMeshPro` componentes están asignados
- Verificar que `FeedbackMessagesSO` está configurado

## **Configuración Avanzada**

### **Timeline para Transiciones:**
1. Crear Timeline Asset
2. Añadir tracks de iluminación
3. Asignar a `DayNightCycle.dayNightTimeline`

### **Sistema de Guardado:**
1. Crear `SaveSystem` script
2. Implementar `ISaveable` interface
3. Configurar puntos de guardado

### **Sistema de Partículas:**
1. Crear prefabs de partículas
2. Añadir `ParticleSystem` components
3. Configurar triggers de eventos

---

**¡La escena está lista para probar el sistema de amanecer-anochecer con presión de sueño atmosférica!** 