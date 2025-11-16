# Diseño Técnico

Este documento amplía la arquitectura y detalla los principales diagramas, especificaciones técnicas y contratos de interfaz del proyecto.

---

## 1. Diagramas de Arquitectura

### 1.1 Diagrama de Despliegue

El siguiente diagrama muestra cómo se despliegan los componentes del sistema:

```mermaid
graph TB
    subgraph Internet["☁️ Internet / Red Local"]
        LB[Load Balancer / Reverse Proxy<br/>nginx o similar<br/>OPCIONAL]
    end
    
    subgraph AppServer["🖥️ Servidor de Aplicación"]
        Web[ASP.NET Core + SignalR<br/>Puerto 5000/5001<br/>GameTribunal.Web]
        AppSvc[Servicios de Aplicación<br/>GameTribunal.Application]
        Domain[Motor de Juego<br/>GameTribunal.Domain]
    end
    
    subgraph DataLayer["💾 Capa de Datos"]
        DB[(Base de Datos<br/>SQL Server / PostgreSQL)]
    end
    
    subgraph ExternalServices["🤖 Servicios Externos"]
        AI[API de IA Generativa<br/>OpenAI / Azure OpenAI]
    end
    
    subgraph Clients["👥 Clientes Web"]
        Host[Navegador Host<br/>PC / Smart TV<br/>Vista Principal]
        P1[Navegador Jugador 1<br/>Smartphone]
        P2[Navegador Jugador 2<br/>Smartphone]
        Pn[... hasta 16 jugadores]
    end
    
    LB --> Web
    Host --> LB
    P1 --> LB
    P2 --> LB
    Pn --> LB
    
    Web --> AppSvc
    AppSvc --> Domain
    AppSvc --> DB
    AppSvc --> AI
    
    style Web fill:#e1f5ff
    style Domain fill:#e8f5e9
    style DB fill:#f3e5f5
    style AI fill:#fff4e1
```

**Notas de despliegue:**
- En desarrollo, todo corre localmente (web app + BD local)
- En producción, se puede desplegar en Azure App Service, AWS, o servidor propio
- El load balancer es opcional pero recomendado para entornos de producción
- HTTPS es obligatorio para WebSockets en navegadores modernos

### 1.2 Diagrama de Secuencia - Inicio de Partida

```mermaid
sequenceDiagram
    participant Host as Host (Navegador)
    participant Hub as SignalR Hub
    participant RoomSvc as RoomService
    participant Engine as GameEngine
    participant DB as Base de Datos
    
    Host->>Hub: CreateRoom(gameMode)
    Hub->>RoomSvc: CreateRoom(gameMode)
    RoomSvc->>Engine: InitializeRoom(gameMode)
    Engine-->>RoomSvc: Room creada (estado Lobby)
    RoomSvc->>DB: Guardar Room
    RoomSvc-->>Hub: RoomDto + RoomCode
    Hub-->>Host: RoomCreated(roomCode, qrCode)
    
    Note over Host: Muestra QR en pantalla
    
    loop Por cada jugador que se une
        participant P as Jugador (Móvil)
        P->>Hub: JoinRoom(roomCode, alias)
        Hub->>RoomSvc: AddPlayer(roomCode, alias)
        RoomSvc->>DB: Guardar Player
        RoomSvc-->>Hub: PlayerDto
        Hub-->>Host: PlayerJoined(player)
        Hub-->>P: JoinedSuccessfully
    end
    
    Host->>Hub: StartGame(roomCode)
    Hub->>RoomSvc: StartGame(roomCode)
    RoomSvc->>Engine: StartGame(room)
    Engine->>Engine: Validar ≥ 4 jugadores
    Engine->>Engine: Transición Lobby → CaseVoting
    Engine->>Engine: Seleccionar primer CaseDefinition
    Engine-->>RoomSvc: Round iniciada
    RoomSvc->>DB: Guardar Round
    RoomSvc-->>Hub: RoundDto
    Hub-->>Host: RoundStarted(round, case)
    Hub-->>P: RoundStarted(round, case)
```

### 1.3 Diagrama de Secuencia - Flujo de Acusación y Defensa

```mermaid
sequenceDiagram
    participant P as Jugadores (Móvil)
    participant Hub as SignalR Hub
    participant RoundSvc as RoundService
    participant Engine as GameEngine
    participant AI as CommentaryService
    participant Host as Host (Navegador)
    
    Note over P: Fase CaseVoting
    
    P->>Hub: SendAccusation(accused1, accused2)
    Hub->>RoundSvc: RegisterAccusation(playerId, accusations)
    RoundSvc->>Engine: AddAccusation(round, accusations)
    Engine-->>RoundSvc: OK
    
    P->>Hub: SendPrediction(predictedAccused)
    Hub->>RoundSvc: RegisterPrediction(playerId, prediction)
    RoundSvc->>Engine: AddPrediction(round, prediction)
    Engine-->>RoundSvc: OK
    
    Note over Engine: Cuando todos votaron
    
    Engine->>Engine: CalculateInitialAccused()
    Engine->>Engine: Transición CaseVoting → Defense
    RoundSvc-->>Hub: AccusedDetermined(initialAccused)
    Hub-->>Host: ShowAccused(player)
    Hub-->>P: AccusedDetermined(player)
    
    Note over P: Fase Defense
    
    P->>Hub: SendDefense(defenseType, altAccused?)
    Hub->>RoundSvc: RegisterDefense(defense)
    RoundSvc->>Engine: ProcessDefense(round, defense)
    
    alt Defensa es Desviar
        Engine->>Engine: Transición Defense → DefenseVoting
        RoundSvc-->>Hub: DefenseVotingStarted
        Hub-->>P: VoteOnDefense(defense)
        
        P->>Hub: VoteDefense(accept/reject)
        Hub->>RoundSvc: RegisterDefenseVote(vote)
        RoundSvc->>Engine: AddDefenseVote(vote)
        
        Note over Engine: Cuando todos votaron
        Engine->>Engine: CalculateFinalAccused()
    else Defensa es Admitir/Negar
        Engine->>Engine: FinalAccused = InitialAccused
    end
    
    Engine->>Engine: Transición → Scoring
    Engine->>Engine: CalculateScores()
    Engine->>Engine: AssignTitle()
    
    RoundSvc->>AI: GenerateCommentary(roundSummary)
    AI-->>RoundSvc: Commentary text
    
    RoundSvc-->>Hub: RoundCompleted(results, commentary)
    Hub-->>Host: ShowResults(results, commentary)
    Hub-->>P: ShowResults(results)
```

### 1.4 Modelo de Datos (ERD)

```mermaid
erDiagram
    ROOM ||--o{ PLAYER : contains
    ROOM ||--o{ ROUND : has
    ROOM {
        guid Id PK
        string RoomCode UK
        int GameMode
        int CurrentPhase
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    PLAYER {
        guid Id PK
        guid RoomId FK
        string Alias
        int Score
        datetime JoinedAt
    }
    
    ROUND ||--o{ ACCUSATION : receives
    ROUND ||--o{ PREDICTION : receives
    ROUND ||--o{ DEFENSE_VOTE : receives
    ROUND ||--|| CASE_DEFINITION : uses
    ROUND ||--|| PLAYER : "initial accused"
    ROUND ||--o| PLAYER : "alt accused"
    ROUND ||--|| PLAYER : "final accused"
    ROUND {
        guid Id PK
        guid RoomId FK
        guid CaseDefinitionId FK
        guid InitialAccusedId FK
        guid AlternativeAccusedId FK
        guid FinalAccusedId FK
        int DefenseType
        int RoundNumber
        datetime StartedAt
        datetime CompletedAt
    }
    
    CASE_DEFINITION {
        guid Id PK
        string Text
        int GameMode
        string SocialTitle
    }
    
    ACCUSATION {
        guid Id PK
        guid RoundId FK
        guid AccuserId FK
        guid AccusedId FK
        datetime CreatedAt
    }
    
    PREDICTION {
        guid Id PK
        guid RoundId FK
        guid PlayerId FK
        guid PredictedAccusedId FK
        datetime CreatedAt
    }
    
    DEFENSE_VOTE {
        guid Id PK
        guid RoundId FK
        guid VoterId FK
        bool Accept
        datetime CreatedAt
    }
    
    TITLE_ASSIGNMENT ||--|| PLAYER : "assigned to"
    TITLE_ASSIGNMENT ||--|| ROUND : "awarded in"
    TITLE_ASSIGNMENT {
        guid Id PK
        guid PlayerId FK
        guid RoundId FK
        string TitleName
        datetime AwardedAt
    }
    
    PLAYER ||--o{ ACCUSATION : makes
    PLAYER ||--o{ ACCUSATION : "accused by"
    PLAYER ||--o{ PREDICTION : makes
    PLAYER ||--o{ DEFENSE_VOTE : casts
    PLAYER ||--o{ TITLE_ASSIGNMENT : receives
```

---

## 2. Especificación de Interfaces

### 2.1 SignalR Hub - Contrato de Métodos

El `GameHub` expone los siguientes métodos que los clientes pueden invocar:

#### Métodos del Hub (servidor)

```csharp
public interface IGameHub
{
    // Gestión de salas
    Task<RoomDto> CreateRoom(GameMode gameMode);
    Task<PlayerDto> JoinRoom(string roomCode, string alias);
    Task LeaveRoom(string roomCode);
    
    // Control de partida
    Task StartGame(string roomCode);
    Task<RoundDto> GetCurrentRound(string roomCode);
    
    // Acciones de jugadores durante la partida
    Task SendAccusation(string roomCode, List<Guid> accusedPlayerIds);
    Task SendPrediction(string roomCode, Guid predictedAccusedId);
    Task SendDefense(string roomCode, DefenseType defenseType, Guid? alternativeAccusedId);
    Task VoteDefense(string roomCode, bool accept);
    
    // Consultas
    Task<RoomStateDto> GetRoomState(string roomCode);
    Task<List<PlayerDto>> GetPlayers(string roomCode);
    Task<ScoreboardDto> GetScoreboard(string roomCode);
}
```

#### Eventos del Hub (notificaciones a clientes)

```csharp
public interface IGameHubClient
{
    // Eventos de sala
    Task RoomCreated(RoomDto room, string qrCodeDataUrl);
    Task RoomUpdated(RoomStateDto state);
    
    // Eventos de jugadores
    Task PlayerJoined(PlayerDto player);
    Task PlayerLeft(Guid playerId);
    
    // Eventos de partida
    Task GameStarted();
    Task RoundStarted(RoundDto round, CaseDto caseDefinition);
    Task PhaseChanged(GamePhase newPhase);
    
    // Eventos de votación
    Task AccusationReceived(Guid playerId);  // Notificación genérica sin revelar el voto
    Task PredictionReceived(Guid playerId);
    Task AllVotesReceived();
    
    // Eventos de defensa
    Task AccusedDetermined(Guid accusedPlayerId, int accusationCount);
    Task DefenseReceived(DefenseType defenseType, Guid? alternativeAccusedId);
    Task DefenseVotingStarted(Guid originalAccused, Guid alternativeAccused);
    Task DefenseVoteReceived(Guid voterId);
    
    // Eventos de resultados
    Task RoundCompleted(RoundResultDto result, string? commentary);
    Task ScoreboardUpdated(ScoreboardDto scoreboard);
    Task GameFinished(GameSummaryDto summary);
    
    // Eventos de error
    Task ErrorOccurred(string message);
}
```

### 2.2 DTOs Principales

#### RoomDto
```csharp
public record RoomDto(
    Guid Id,
    string RoomCode,
    GameMode GameMode,
    GamePhase CurrentPhase,
    int PlayerCount,
    DateTime CreatedAt
);
```

#### PlayerDto
```csharp
public record PlayerDto(
    Guid Id,
    string Alias,
    int Score,
    List<string> Titles,
    bool IsConnected
);
```

#### RoundDto
```csharp
public record RoundDto(
    Guid Id,
    int RoundNumber,
    CaseDto Case,
    GamePhase CurrentPhase,
    Guid? InitialAccusedId,
    Guid? FinalAccusedId
);
```

#### CaseDto
```csharp
public record CaseDto(
    Guid Id,
    string Text,
    string SocialTitle,
    GameMode GameMode
);
```

#### RoundResultDto
```csharp
public record RoundResultDto(
    Guid RoundId,
    Guid FinalAccusedId,
    string AssignedTitle,
    Dictionary<Guid, int> PointsAwarded,  // PlayerId -> Puntos ganados esta ronda
    List<Guid> CorrectAccusers,
    List<Guid> CorrectPredictors,
    bool DefenseSuccessful
);
```

#### ScoreboardDto
```csharp
public record ScoreboardDto(
    List<PlayerScoreDto> Rankings,  // Ordenado por puntuación descendente
    int CurrentRound,
    int TotalRounds
);

public record PlayerScoreDto(
    Guid PlayerId,
    string Alias,
    int Score,
    List<string> Titles,
    int Position
);
```

#### GameSummaryDto
```csharp
public record GameSummaryDto(
    ScoreboardDto FinalScoreboard,
    Dictionary<string, int> TitleFrequency,  // Título -> Veces asignado
    TimeSpan GameDuration,
    int TotalRounds
);
```

#### RoomStateDto
```csharp
public record RoomStateDto(
    RoomDto Room,
    List<PlayerDto> Players,
    RoundDto? CurrentRound,
    GamePhase CurrentPhase
);
```

### 2.3 Enumeraciones

#### GameMode
```csharp
public enum GameMode
{
    Suave = 0,    // Casos suaves, para grupos que se conocen poco
    Normal = 1,   // Casos estándar
    Spicy = 2     // Casos más atrevidos, para grupos de confianza
}
```

#### GamePhase
```csharp
public enum GamePhase
{
    Lobby = 0,           // Esperando jugadores
    CaseVoting = 1,      // Votando acusaciones y predicciones
    Defense = 2,         // Acusado defendiéndose
    DefenseVoting = 3,   // Votando si se acepta la defensa de desvío
    Scoring = 4,         // Calculando y mostrando resultados
    Finished = 5         // Partida terminada
}
```

#### DefenseType
```csharp
public enum DefenseType
{
    Admitir = 0,   // "Lo admito"
    Negar = 1,     // "No encaja conmigo"
    Desviar = 2    // "Creo que es más bien [otro jugador]"
}
```

---

## 3. Reglas de Validación

### 3.1 Validaciones de entrada

| Campo | Reglas |
|-------|--------|
| Alias de jugador | - Longitud: 3-20 caracteres<br/>- Caracteres alfanuméricos y espacios<br/>- Único dentro de la sala |
| RoomCode | - Formato: 6 caracteres alfanuméricos uppercase<br/>- Único globalmente |
| Acusaciones | - Mínimo 1, máximo 2 acusados<br/>- No se puede acusar a sí mismo<br/>- IDs deben ser jugadores válidos de la sala |
| Predicción | - Exactamente 1 predicción<br/>- Debe ser un jugador válido de la sala |
| Defensa de desvío | - Debe especificar alternativeAccusedId<br/>- No puede ser el acusado original<br/>- Debe ser jugador válido |

### 3.2 Validaciones de estado

| Acción | Precondiciones |
|--------|----------------|
| Iniciar partida | - Fase = Lobby<br/>- Mínimo 4 jugadores conectados |
| Enviar acusación | - Fase = CaseVoting<br/>- Jugador no ha votado aún |
| Enviar defensa | - Fase = Defense<br/>- Solo el acusado inicial puede defenderse |
| Votar defensa | - Fase = DefenseVoting<br/>- Jugador no es el acusado original |

---

## 4. Gestión de Errores

### 4.1 Códigos de error

| Código | Descripción | Acción del cliente |
|--------|-------------|-------------------|
| ROOM_NOT_FOUND | Sala no existe | Mostrar error, volver a lobby |
| ROOM_FULL | Sala llena (16 jugadores) | Mostrar mensaje, no permitir unirse |
| ALIAS_TAKEN | Alias ya en uso | Solicitar alias diferente |
| INVALID_PHASE | Acción no válida en fase actual | Sincronizar estado con servidor |
| PLAYER_NOT_FOUND | Jugador no existe | Error interno, reconectar |
| INVALID_VOTE | Voto inválido | Mostrar error, permitir reintentar |

### 4.2 Manejo de desconexiones

- **Desconexión temporal (<30s):** El jugador se marca como desconectado pero no se elimina. Puede reconectarse automáticamente.
- **Desconexión prolongada (>30s):** 
  - En Lobby: El jugador se elimina de la sala
  - En partida: El jugador se marca como inactivo, sus votos pendientes se omiten
- **Reconexión:** El cliente recibe el estado completo actual mediante `GetRoomState()`

---

## 5. Consideraciones de Seguridad

### 5.1 Validación en servidor

- ✅ Todas las acciones se validan en el servidor (nunca confiar en el cliente)
- ✅ RoomCode actúa como token de acceso simple
- ✅ Límite de rate limiting por IP (prevenir spam)
- ✅ Validación de longitud de strings (prevenir ataques de memoria)
- ✅ Sanitización de alias (prevenir XSS en nombres)

### 5.2 Datos sensibles

- ⚠️ No se almacenan datos personales reales (solo pseudónimos temporales)
- ⚠️ Las salas son efímeras (se limpian tras inactividad)
- ⚠️ No hay sistema de cuentas ni passwords

---

## 6. Próximos Pasos

Este documento se completará con:

- [ ] Especificación detallada de la API de IA (prompts, formato de respuesta)
- [ ] Diagramas de componentes Blazor
- [ ] Wireframes de interfaz (host y jugador)
- [ ] Especificación de configuración (appsettings)
- [ ] Política de limpieza de salas inactivas
