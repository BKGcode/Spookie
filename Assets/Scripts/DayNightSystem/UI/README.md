# DayNightSystem UI - Sistema Modular

## Descripción General

El sistema de UI del DayNightSystem ha sido refactorizado en módulos más pequeños y especializados, siguiendo los principios de responsabilidad única y mantenibilidad.

## Estructura de Carpetas

```
DayNightSystem/UI/
├── TimeDisplay/
│   └── TimeDisplayController.cs
├── StatusDisplay/
│   └── StatusDisplayController.cs
├── WarningSystem/
│   └── WarningSystemController.cs
├── ProximitySystem/
│   └── ProximitySystemController.cs
├── DayNightUIManager.cs
└── README.md
```

## Componentes del Sistema

### 1. TimeDisplayController
**Responsabilidad**: Maneja la visualización del tiempo del día/noche.

**Funcionalidades**:
- Formateo de tiempo (12/24 horas, con/sin segundos)
- Actualización automática del display
- Configuración de formato de tiempo
- Eventos de cambio de tiempo

**Referencias del Inspector**:
- `timeDisplay`: TextMeshProUGUI para mostrar el tiempo
- `timeFormat`: Formato de tiempo (HH:mm)
- `showSeconds`: Mostrar segundos
- `use24HourFormat`: Usar formato 24 horas

### 2. StatusDisplayController
**Responsabilidad**: Maneja la visualización del estado del día/noche y penalizaciones.

**Funcionalidades**:
- Mostrar estado actual (Día, Noche, Exhausto, etc.)
- Información de penalizaciones activas
- Actualización automática del estado
- Configuración de detalle del estado

**Referencias del Inspector**:
- `statusDisplay`: TextMeshProUGUI para mostrar el estado
- `feedbackMessages`: ScriptableObject con mensajes
- `showDetailedStatus`: Mostrar estado detallado
- `showPenaltyInfo`: Mostrar información de penalizaciones

### 3. WarningSystemController
**Responsabilidad**: Maneja las advertencias visuales y sonoras.

**Funcionalidades**:
- Iconos de advertencia parpadeantes
- Sonidos de advertencia
- Diferentes tipos de advertencias (exhaustión, desmayo, etc.)
- Configuración de parpadeo

**Referencias del Inspector**:
- `warningIcon`: Image para el icono de advertencia
- `feedbackMessages`: ScriptableObject con mensajes
- `audioManager`: Referencia al AudioManager
- `warningBlinkRate`: Velocidad de parpadeo

### 4. ProximitySystemController
**Responsabilidad**: Maneja la visualización de proximidad al spawn point.

**Funcionalidades**:
- Indicadores de proximidad al spawn
- Zonas de advertencia y peligro
- Colores dinámicos según distancia
- Actualización automática de distancia

**Referencias del Inspector**:
- `spawnProximityIcon`: Image para el icono de proximidad
- `feedbackMessages`: ScriptableObject con mensajes
- `showSpawnProximity`: Mostrar indicador de proximidad
- `warningDistance`: Distancia de advertencia
- `dangerDistance`: Distancia de peligro

### 5. DayNightUIManager
**Responsabilidad**: Coordinador principal del sistema de UI.

**Funcionalidades**:
- Inicialización automática de todos los componentes
- Delegación de eventos
- Métodos públicos unificados
- Validación de referencias

**Referencias del Inspector**:
- `feedbackMessages`: ScriptableObject con mensajes
- `audioManager`: Referencia al AudioManager

## Beneficios de la Modularización

### 1. Mantenibilidad
- Cada componente tiene una responsabilidad específica
- Cambios en un módulo no afectan a otros
- Código más fácil de entender y modificar

### 2. Reutilización
- Los módulos pueden ser reutilizados en otros proyectos
- Fácil intercambio de componentes
- Configuración independiente por módulo

### 3. Testing
- Cada módulo puede ser testeado independientemente
- Menor complejidad por componente
- Mejor cobertura de pruebas

### 4. Escalabilidad
- Fácil agregar nuevos tipos de UI
- Configuración granular por módulo
- Arquitectura extensible

## Migración desde DayNightUI Original

### Uso del MigrationHelper
El `MigrationHelper` incluye funcionalidad para migrar automáticamente desde el `DayNightUI` original:

1. **Migración Automática**: Se ejecuta al iniciar si `autoMigrate` está habilitado
2. **Preservación de Referencias**: Migra automáticamente las referencias de UI
3. **Creación de Sistema Modular**: Crea el nuevo sistema con todos los componentes
4. **Configuración Automática**: Asigna referencias usando reflection

### Proceso de Migración
```csharp
// El MigrationHelper automáticamente:
// 1. Crea el nuevo DayNightUIManager
// 2. Añade todos los componentes modulares
// 3. Migra las referencias de UI
// 4. Configura las dependencias
```

## Configuración en Unity

### 1. Configuración Manual
1. Crear un GameObject vacío
2. Añadir `DayNightUIManager`
3. Asignar referencias en el Inspector
4. Los componentes modulares se añaden automáticamente

### 2. Configuración de Referencias
- **FeedbackMessagesSO**: ScriptableObject con mensajes
- **AudioManager**: Referencia al sistema de audio
- **UI Elements**: TextMeshPro e Image components

### 3. Configuración de ScriptableObjects
Crear en `Assets > Create > Spookie > Feedback Messages`:
- `status_day`: "Día"
- `status_night`: "Noche"
- `status_exhausted`: "Exhausto"
- `warning_exhaustion`: "¡Advertencia de agotamiento!"
- `proximity_safe_night`: "Seguro en la noche"

## Eventos del Sistema

### TimeDisplayController
- `OnTimeDisplayUpdated(string time)`: Cuando se actualiza el tiempo

### StatusDisplayController
- `OnStatusUpdated(string status)`: Cuando cambia el estado

### WarningSystemController
- `OnWarningStateChanged(bool active)`: Cuando cambia el estado de advertencia
- `OnWarningStarted()`: Cuando inicia una advertencia
- `OnWarningStopped()`: Cuando se detiene una advertencia

### ProximitySystemController
- `OnProximityStateChanged(bool active)`: Cuando cambia el estado de proximidad
- `OnWarningZoneChanged(bool inZone)`: Cuando entra/sale de zona de advertencia
- `OnDangerZoneChanged(bool inZone)`: Cuando entra/sale de zona de peligro
- `OnDistanceChanged(float distance)`: Cuando cambia la distancia

## Métodos Públicos Principales

### DayNightUIManager
```csharp
// Actualización
public void ForceUpdateTimeDisplay()
public void ForceUpdateStatusDisplay()
public void ForceUpdateAllUI()

// Advertencias
public void ForceWarning(string message)
public void StopCurrentWarning()

// Proximidad
public void ForceProximityCheck()

// Configuración
public void SetTimeFormat(string format)
public void SetShowSeconds(bool show)
public void SetShowDetailedStatus(bool show)
public void SetShowWarningIcon(bool show)
public void SetShowSpawnProximity(bool show)

// Estado
public string GetCurrentTimeString()
public string GetCurrentStatusText()
public bool IsWarningActive()
public bool IsProximityActive()
```

## Context Menu Options

Cada componente incluye opciones de Context Menu para testing y debugging:

- **Force Update**: Actualizar manualmente el componente
- **Show Status**: Mostrar estado actual del componente
- **Test Warning**: Probar sistema de advertencias
- **Toggle Features**: Activar/desactivar características
- **Validate Components**: Validar referencias

## Próximos Pasos

1. **Refactorizar TransitionHandler**: Aplicar el mismo patrón modular
2. **Optimizar Comunicación**: Mejorar eventos entre módulos
3. **Añadir Nuevos Módulos**: Expandir funcionalidad según necesidades
4. **Documentación Adicional**: Crear guías específicas por módulo

## Troubleshooting

### Problemas Comunes

1. **Referencias Faltantes**: Verificar asignación en Inspector
2. **Eventos No Funcionando**: Verificar suscripción en OnEnable/OnDisable
3. **UI No Actualizada**: Verificar que los componentes estén activos
4. **Migración Fallida**: Usar Context Menu "Validate Components"

### Debugging

- Habilitar `showDebugLogs` en cada componente
- Usar Context Menu options para testing
- Verificar logs en Console de Unity
- Validar referencias con "Validate Components"
