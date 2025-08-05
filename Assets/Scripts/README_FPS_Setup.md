# Sistema FPS - Configuración Completa

## Scripts Creados

### Core Scripts
- **FirstPersonController.cs**: Controlador principal del jugador
- **IInteractable.cs**: Interfaz para objetos interactuables
- **InteractableObject.cs**: Ejemplo de objeto interactuable
- **FeedbackMessagesSO.cs**: ScriptableObject para mensajes
- **PlayerSettingsSO.cs**: Configuración del jugador
- **InputManager.cs**: Gestión del Input System
- **GameManager.cs**: Estado global del juego
- **CursorManager.cs**: Gestión del cursor
- **InteractionUI.cs**: UI de interacción
- **InputSetupHelper.cs**: Utilidades para Input System

## Configuración Paso a Paso

### 1. Crear ScriptableObjects

#### FeedbackMessagesSO
1. Click derecho en Project > Create > Spookie > Feedback Messages
2. Nombrar como "FeedbackMessages"
3. Agregar mensajes:
   - Key: "press_to_interact", Message: "Presiona E para interactuar"
   - Key: "interaction_success", Message: "¡Interacción exitosa!"

#### PlayerSettingsSO
1. Click derecho en Project > Create > Spookie > Player Settings
2. Nombrar como "PlayerSettings"
3. Configurar valores según preferencia

### 2. Configurar Input System

#### Crear Input Actions Asset
1. Click derecho en Project > Create > Input Actions
2. Nombrar como "PlayerInput"
3. Agregar Action Map "Player"
4. Agregar acciones:
   - **Move** (Value, 2D Vector): WASD/Arrow Keys
   - **Look** (Value, 2D Vector): Mouse Delta
   - **Jump** (Button): Space
   - **Run** (Button): Left Shift
   - **Interact** (Button): E
   - **ToggleCursor** (Button): Tab

#### Generar C# Class
1. Seleccionar PlayerInput asset
2. En Inspector, marcar "Generate C# Class"
3. Click "Apply"

### 3. Configurar Player GameObject

#### Estructura del Player
```
Player (GameObject)
├── CharacterController
├── FirstPersonController
├── PlayerInput
├── InputManager
├── CursorManager
└── Camera (Child)
    └── Main Camera
```

#### Configuración de Componentes

**FirstPersonController:**
- Player Settings: Asignar PlayerSettingsSO
- Camera Transform: Asignar Main Camera
- Feedback Messages: Asignar FeedbackMessagesSO
- Interactable Layer: Configurar layer para objetos interactuables

**PlayerInput:**
- Actions: Asignar PlayerInput asset
- Behavior: Invoke Unity Events
- Connect events:
  - Move → FirstPersonController.OnMove
  - Look → FirstPersonController.OnLook
  - Jump → FirstPersonController.OnJump
  - Run → FirstPersonController.OnRun
  - Interact → FirstPersonController.OnInteract
  - ToggleCursor → CursorManager.OnToggleCursor

**InputManager:**
- Input Actions: Asignar PlayerInput asset

### 4. Configurar UI

#### Canvas Structure
```
Canvas (Screen Space - Overlay)
├── GameUI
│   └── InteractionPanel
│       └── InteractionText (TextMeshPro)
└── PauseMenu
    └── PausePanel
```

#### InteractionUI
- Interaction Text: Asignar TextMeshProUGUI
- Interaction Panel: Asignar GameObject del panel
- Feedback Messages: Asignar FeedbackMessagesSO

### 5. Configurar Objetos Interactuables

#### Crear Layer
1. Edit > Project Settings > Tags and Layers
2. Crear layer "Interactable"

#### Configurar Objeto Interactuable
1. Seleccionar objeto
2. Asignar layer "Interactable"
3. Agregar componente InteractableObject
4. Configurar:
   - Normal Material: Material normal
   - Highlight Material: Material de resaltado
   - Object Renderer: Renderer del objeto
   - Interaction Text: TextMeshProUGUI de UI
   - Feedback Messages: FeedbackMessagesSO

### 6. Configurar GameManager

#### GameManager Setup
1. Crear GameObject "GameManager"
2. Agregar componente GameManager
3. Configurar:
   - Feedback Messages: FeedbackMessagesSO
   - Input Manager: Asignar InputManager
   - Connect UnityEvents según necesidad

### 7. Configurar CursorManager

#### CursorManager Setup
1. Agregar componente CursorManager al Player
2. Configurar:
   - Pause Menu: GameObject del menú de pausa
   - Game UI: GameObject de la UI del juego

## Controles

- **WASD**: Movimiento
- **Mouse**: Rotación de cámara
- **Space**: Saltar
- **Left Shift**: Correr
- **E**: Interactuar
- **Tab**: Alternar cursor

## Características Implementadas

✅ Movimiento en primera persona
✅ Rotación de cámara con mouse
✅ Sistema de salto
✅ Sistema de correr
✅ Sistema de interacción con objetos
✅ Feedback visual (resaltado de objetos)
✅ UI de interacción
✅ Sistema de mensajes configurable
✅ Configuración modular con ScriptableObjects
✅ Gestión de cursor
✅ Sistema de pausa
✅ Input System integrado

## Troubleshooting

### Problemas Comunes

1. **El jugador no se mueve:**
   - Verificar que PlayerInput esté configurado correctamente
   - Revisar que los eventos estén conectados

2. **La cámara no rota:**
   - Verificar que Camera Transform esté asignado
   - Revisar configuración de Mouse Sensitivity

3. **No se detectan objetos interactuables:**
   - Verificar que tengan el layer correcto
   - Revisar Interaction Range en PlayerSettings

4. **Input System no funciona:**
   - Verificar que Input Actions Asset esté asignado
   - Revisar que esté habilitado en InputManager

### Debug

Todos los scripts incluyen Debug.Log para facilitar la depuración. Revisar la consola para información detallada. 