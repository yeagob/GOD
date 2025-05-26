# Sistema de Pruebas Firebase para GOD

## Descripción

Sistema completo de pruebas para Firebase siguiendo la arquitectura MVP del proyecto Game Of Duck (GOD). Permite probar todas las funcionalidades de comunicación con Firebase de manera manual y automatizada.

## Estructura del Sistema

### Arquitectura MVP

```
Network/
├── Data/                    # Estructuras de datos POCO
│   ├── MatchData.cs
│   ├── PlayerMatchData.cs
│   └── GameEventData.cs
├── Services/                # Servicios de comunicación
│   └── FirebaseService.cs
├── Repositories/            # Acceso a datos
│   ├── MatchRepository.cs
│   ├── PlayerMatchRepository.cs
│   └── GameEventRepository.cs
├── Models/                  # Lógica de negocio
│   ├── MatchModel.cs
│   ├── PlayerMatchModel.cs
│   └── GameEventModel.cs
├── Presenters/              # Controladores MVP
│   ├── MatchPresenter.cs
│   ├── PlayerMatchPresenter.cs
│   └── GameEventPresenter.cs
├── Testing/                 # Sistema de pruebas
│   ├── FirebaseTestController.cs
│   ├── AutomatedFirebaseTest.cs
│   ├── FirebaseTestUI.cs
│   ├── FirebaseTestData.cs
│   └── FirebaseTestSceneController.cs
└── NetworkInstaller.cs      # Instalador de dependencias
```

## Componentes Principales

### 1. Estructuras de Datos (Data Layer)

- **MatchData**: Información de partidas
- **PlayerMatchData**: Datos de jugadores en partidas
- **GameEventData**: Eventos del juego

### 2. Servicios

- **FirebaseService**: Comunicación directa con Firebase
  - Inicialización automática
  - Prueba de conexión
  - Operaciones CRUD genéricas
  - Manejo de errores

### 3. Repositorios

- **MatchRepository**: Gestión de partidas
- **PlayerMatchRepository**: Gestión de jugadores
- **GameEventRepository**: Gestión de eventos

### 4. Modelos

- **MatchModel**: Lógica de partidas
- **PlayerMatchModel**: Lógica de jugadores
- **GameEventModel**: Lógica de eventos

### 5. Presenters

- **MatchPresenter**: Controlador de partidas
- **PlayerMatchPresenter**: Controlador de jugadores
- **GameEventPresenter**: Controlador de eventos

## Sistema de Pruebas

### Pruebas Manuales

**FirebaseTestController** proporciona:

- Prueba de conexión a Firebase
- Creación de partidas
- Unión a partidas
- Movimiento de jugadores
- Tirada de dados
- Finalización de partidas
- Log detallado de todas las operaciones

### Pruebas Automatizadas

**AutomatedFirebaseTest** permite:

- Ejecución de escenarios predefinidos
- Secuencias automáticas de acciones
- Configuración mediante ScriptableObjects
- Reportes de éxito/fallo

### UI de Pruebas

**FirebaseTestUI** incluye:

- Panel principal con controles
- Log en tiempo real
- Información de partidas y jugadores
- Export de logs
- Controles de pruebas automatizadas

## Configuración

### 1. Dependencias de Firebase

Asegúrate de tener instalado:
- Firebase SDK para Unity
- Firebase Realtime Database

### 2. Configuración de Firebase

1. Crear proyecto en Firebase Console
2. Configurar Realtime Database
3. Descargar `google-services.json`
4. Colocar en `Assets/StreamingAssets/`

### 3. Configuración de Escena

#### Opción A: Usar Escena Preconfigurada

1. Abrir `Assets/Scenes/FirebaseTest.unity`
2. Configurar Firebase
3. Ejecutar

#### Opción B: Configuración Manual

1. Crear GameObject vacío
2. Agregar `FirebaseTestController`
3. Configurar referencias de UI
4. Opcional: Agregar `AutomatedFirebaseTest` y `FirebaseTestUI`

## Uso

### Pruebas Manuales

1. **Test Connection**: Verificar conectividad
2. **Create Match**: Crear nueva partida
3. **Join Match**: Unirse a partida (usar ID generado)
4. **Move Player**: Simular movimiento
5. **Roll Dice**: Simular tirada de dados
6. **End Match**: Finalizar partida

### Pruebas Automatizadas

1. Crear `FirebaseTestData` ScriptableObject
2. Configurar escenarios de prueba
3. Asignar al `AutomatedFirebaseTest`
4. Ejecutar con "Run Automated Test"

### Ejemplo de Configuración de Test Data

```csharp
// Crear Assets/TestData/BasicFirebaseTest.asset
TestScenario basicTest = new TestScenario
{
    scenarioName = "Basic Flow Test",
    description = "Test complete game flow",
    actions = new TestAction[]
    {
        new TestAction { actionType = TestConnection, delay = 2f },
        new TestAction { actionType = CreateMatch, delay = 3f },
        new TestAction { actionType = JoinMatch, delay = 2f },
        new TestAction { actionType = RollDice, delay = 1f },
        new TestAction { actionType = MovePlayer, delay = 2f },
        new TestAction { actionType = EndMatch, delay = 2f }
    }
};
```

## Integración con URL Parameters

El sistema detecta automáticamente:
- `firebase-test`: Mantiene modo de prueba
- `multiplayer` o `board`: Carga escena principal del juego

## Logs y Debugging

- Logs detallados en consola Unity
- UI con timestamps
- Export de logs a archivo
- Indicadores visuales de éxito/error

## Estructura de Datos en Firebase

```json
{
  "matches": {
    "match_id": {
      "MatchId": "string",
      "StartTime": "datetime",
      "Status": "waiting|active|finished",
      "MaxPlayers": "int",
      "CurrentPlayers": "int",
      "BoardType": "string"
    }
  },
  "playerMatches": {
    "match_id": {
      "player_id": {
        "PlayerId": "string",
        "PlayerName": "string",
        "Position": "int",
        "Score": "int",
        "IsActive": "bool",
        "IsWinner": "bool"
      }
    }
  },
  "gameEvents": {
    "event_id": {
      "EventId": "string",
      "MatchId": "string",
      "PlayerId": "string",
      "EventType": "string",
      "EventData": "json",
      "Timestamp": "datetime",
      "Turn": "int"
    }
  }
}
```

## Extensión del Sistema

### Agregar Nuevos Tipos de Datos

1. Crear struct en `Network/Data/`
2. Crear Repository en `Network/Repositories/`
3. Crear Model en `Network/Models/`
4. Crear Presenter en `Network/Presenters/`
5. Actualizar `NetworkInstaller`
6. Agregar pruebas específicas

### Agregar Nuevas Pruebas

1. Extender `FirebaseTestData.TestActionType`
2. Implementar lógica en `AutomatedFirebaseTest`
3. Actualizar UI si es necesario

## Principios SOLID Aplicados

- **S**: Cada clase tiene una responsabilidad específica
- **O**: Sistema extensible sin modificar código existente
- **L**: Los repositorios y modelos son intercambiables
- **I**: Interfaces segregadas por responsabilidad
- **D**: Dependencias inyectadas via constructor

## Características Técnicas

- ✅ Arquitectura MVP consistente
- ✅ Estructuras POCO para datos
- ✅ Manejo asíncrono con async/await
- ✅ Inyección de dependencias
- ✅ Principios SOLID
- ✅ Filosofía Scout (mejora continua)
- ✅ Sin comentarios en código
- ✅ Llaves siempre presentes
- ✅ Manejo robusto de errores
- ✅ Logs detallados
- ✅ Configuración via ScriptableObjects
- ✅ UI responsive y informativa

## Integración con GOD

Este sistema se integra perfectamente con el juego principal:

1. **Desarrollo**: Usar para probar comunicación Firebase
2. **Staging**: Validar funcionalidad antes de release
3. **Production**: Monitoreo y debugging en vivo
4. **Mantenimiento**: Pruebas regulares de conectividad

## Próximos Pasos

1. Configurar Firebase en el proyecto
2. Ejecutar pruebas básicas
3. Crear escenarios específicos para tu juego
4. Integrar con el flujo principal del juego
5. Automatizar pruebas en CI/CD

---

**Nota**: Este sistema respeta completamente la arquitectura y principios del proyecto Game Of Duck, mantiene la consistencia con el patrón MVP y facilita el testing y debugging de la comunicación con Firebase.
