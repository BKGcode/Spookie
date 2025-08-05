# Sistema FPS - Configuración Automática

## 🚀 Configuración Rápida (Recomendado)

### Opción 1: Setup Wizard (Más Fácil)
1. **Abrir el Wizard:**
   - Menú: `Tools > FPS System > Setup Wizard`
   - Sigue los pasos guiados del wizard

### Opción 2: Herramienta Completa
1. **Abrir la Herramienta:**
   - Menú: `Tools > FPS System > Generate Complete FPS System`
   - Configura las opciones y genera todo de una vez

## 🔧 Herramientas Disponibles

### FPSSystemTools
**Ubicación:** `Tools > FPS System > Generate Complete FPS System`

**Características:**
- ✅ Genera Input Actions Asset automáticamente
- ✅ Crea prefab del Player completo
- ✅ Crea prefab de objetos interactuables
- ✅ Genera materiales por defecto
- ✅ Configura la escena completa
- ✅ Asigna referencias automáticamente

**Uso:**
1. Abrir la herramienta desde el menú
2. Asignar ScriptableObjects (PlayerSettingsSO, FeedbackMessagesSO)
3. Configurar opciones de generación
4. Hacer click en "Generate Complete FPS System"

### FPSSetupWizard
**Ubicación:** `Tools > FPS System > Setup Wizard`

**Características:**
- ✅ Wizard paso a paso
- ✅ Barra de progreso visual
- ✅ Validación en cada paso
- ✅ Creación automática de assets
- ✅ Guía completa del proceso

**Pasos del Wizard:**
1. **Bienvenida:** Información del sistema
2. **ScriptableObjects:** Crear/assignar PlayerSettingsSO y FeedbackMessagesSO
3. **Input System:** Crear Input Actions Asset
4. **Materials:** Crear materiales para objetos interactuables
5. **Prefabs:** Generar Player_FPS e InteractableObject prefabs
6. **Scene Setup:** Configurar UI de la escena

## 📁 Estructura de Archivos Generados

```
Assets/
├── PlayerSettings.asset              # Configuración del jugador
├── FeedbackMessages.asset            # Mensajes de UI
├── PlayerInput.inputactions          # Input System configurado
├── Materials/
│   ├── NormalMaterial.mat           # Material normal
│   └── HighlightMaterial.mat        # Material de resaltado
└── Prefabs/
    ├── Player_FPS.prefab            # Player completo
    └── InteractableObject.prefab    # Objeto interactuable
```

## 🎮 Características del Sistema

### Player Prefab Incluye:
- ✅ **CharacterController** configurado
- ✅ **Camera** en posición correcta
- ✅ **FirstPersonController** con todos los scripts
- ✅ **PlayerInput** configurado
- ✅ **InputManager** para gestión de input
- ✅ **CursorManager** para gestión de cursor
- ✅ **InteractionUI** para UI de interacción

### Input System Configurado:
- ✅ **Move:** WASD + Arrow Keys
- ✅ **Look:** Mouse Delta
- ✅ **Jump:** Space
- ✅ **Run:** Left Shift
- ✅ **Interact:** E
- ✅ **ToggleCursor:** Tab

### Objetos Interactuables:
- ✅ **Layer "Interactable"** automático
- ✅ **Material de resaltado** configurado
- ✅ **Collider** apropiado
- ✅ **InteractableObject** script configurado

## 🛠️ Configuración Manual

### Paso 1: Crear ScriptableObjects
```csharp
// PlayerSettingsSO
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Spookie/Player Settings")]

// FeedbackMessagesSO  
[CreateAssetMenu(fileName = "FeedbackMessages", menuName = "Spookie/Feedback Messages")]
```

### Paso 2: Configurar Input System
- Crear Input Actions Asset
- Configurar Action Map "Player"
- Agregar acciones: Move, Look, Jump, Run, Interact, ToggleCursor

### Paso 3: Crear Prefabs
- Player_FPS con todos los componentes
- InteractableObject con materiales y scripts

### Paso 4: Configurar UI
- Canvas con GameUI y PauseMenu
- InteractionPanel con TextMeshPro
- Asignar referencias al Player

## 🐛 Troubleshooting

### Problemas Comunes:

1. **"Input Actions Asset not found":**
   - Usar la herramienta para generar automáticamente
   - Verificar que se creó el archivo PlayerInput.inputactions

2. **"Interactable layer not found":**
   - La herramienta crea automáticamente el layer
   - Si falla, crear manualmente en Edit > Project Settings > Tags and Layers

3. **"PlayerSettings not assigned":**
   - Usar el wizard para crear automáticamente
   - Verificar que el SO existe y está configurado

4. **"FeedbackMessages not assigned":**
   - Usar el wizard para crear automáticamente
   - Verificar que tiene los mensajes necesarios

### Debug:
- Todas las herramientas incluyen Debug.Log detallado
- Revisar la consola para información de generación
- Los errores se muestran claramente en la consola

## 🎯 Próximos Pasos

1. **Usar las herramientas** para configuración rápida
2. **Personalizar materiales** según tu estilo visual
3. **Ajustar configuraciones** en PlayerSettingsSO
4. **Agregar más objetos** interactuables usando el prefab
5. **Crear UI personalizada** para tu juego

## 📝 Notas Importantes

- **Solo funciona en Editor:** Las herramientas solo funcionan en modo Editor
- **Backup recomendado:** Hacer backup antes de generar
- **URP requerido:** El sistema está configurado para URP
- **TextMeshPro:** Se requiere para la UI de interacción
- **Tools en lugar de scripts:** Siguiendo las @unityrules.mdc, usamos herramientas de Editor

## 🎮 Controles Finales

- **WASD:** Movimiento
- **Mouse:** Rotación de cámara
- **Space:** Saltar
- **Left Shift:** Correr
- **E:** Interactuar
- **Tab:** Alternar cursor

¡El sistema está listo para usar! 🎉 