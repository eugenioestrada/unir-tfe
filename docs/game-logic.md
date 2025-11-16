# Lógica de juego

Este documento describe las reglas de juego de **Tribunal Social**: el modelo conceptual, las fases de una partida y cómo se calculan acusaciones, defensas, resultados y títulos sociales.

---

## 1. Objetivo del juego

El juego está diseñado para:

- generar **situaciones sociales** basadas en casos del estilo “¿Quién es más probable que…?”
- obligar al grupo a **señalar** a miembros concretos,
- producir **pequeñas fricciones y risas** a través de acusaciones y defensas,
- asignar **títulos sociales** que se convierten en running gags del grupo.

No hay un “ganador absoluto” en términos profundos: la competición existe (puntuación), pero el foco es la dinámica social.

---

## 2. Modelo de dominio (resumen)

### Entidades principales

- `Room`
  - Código de sala (`RoomCode`).
  - Modo de juego (`GameMode`): `Suave`, `Normal`, `Spicy`.
  - Lista de `Player`.
  - Lista de `Round` jugadas.
  - Fase actual (`GamePhase`).
  - Ronda en curso.

- `Player`
  - Identificador interno.
  - Alias visible para el grupo.
  - Puntuación acumulada.
  - Lista de `TitleAssignment` (títulos sociales recibidos).

- `CaseDefinition`
  - Identificador.
  - Texto del caso (ej.: “¿Quién es más probable que llegue siempre tarde?”).
  - `GameMode` al que pertenece.
  - Título social asociado al caso (ej.: “Señor/a de la impuntualidad crónica”).

- `Round`
  - Referencia a `CaseDefinition`.
  - Colecciones de votos:
    - `Accusations` (quién acusa a quién).
    - `Predictions` (quién cree que será el acusado oficial).
    - `DefenseVotes` (votos sobre si se acepta o no la defensa).
  - Acusado inicial.
  - Acusado alternativo (en caso de defensa de desvío).
  - Acusado final (tras defensa y votación).
  - Resultado de puntuación.

- `TitleAssignment`
  - Jugador.
  - Título recibido.
  - Ronda en la que se otorgó.

```mermaid
classDiagram
    class Room {
        +String RoomCode
        +GameMode GameMode
        +GamePhase CurrentPhase
        +List~Player~ Players
        +List~Round~ Rounds
        +Round CurrentRound
    }
    
    class Player {
        +Guid Id
        +String Alias
        +int Score
        +List~TitleAssignment~ Titles
    }
    
    class CaseDefinition {
        +Guid Id
        +String Text
        +GameMode Mode
        +String SocialTitle
    }
    
    class Round {
        +CaseDefinition Case
        +List~Accusation~ Accusations
        +List~Prediction~ Predictions
        +List~DefenseVote~ DefenseVotes
        +Player InitialAccused
        +Player AlternativeAccused
        +Player FinalAccused
        +DefenseType DefenseChosen
    }
    
    class TitleAssignment {
        +Player Player
        +String Title
        +Round Round
    }
    
    class GameMode {
        <<enumeration>>
        Suave
        Normal
        Spicy
    }
    
    class GamePhase {
        <<enumeration>>
        Lobby
        CaseVoting
        Defense
        DefenseVoting
        Scoring
        Finished
    }
    
    Room "1" --> "*" Player
    Room "1" --> "*" Round
    Room --> GameMode
    Room --> GamePhase
    Round --> CaseDefinition
    Round --> Player : InitialAccused
    Round --> Player : AlternativeAccused
    Round --> Player : FinalAccused
    Player "1" --> "*" TitleAssignment
    TitleAssignment --> Round
```

---

## 3. Fases de la partida (`GamePhase`)

```mermaid
stateDiagram-v2
    [*] --> Lobby
    Lobby --> CaseVoting : Iniciar partida<br/>(mín. 4 jugadores)
    
    CaseVoting --> Defense : Todos votan
    
    Defense --> DefenseVoting : Defensa tipo<br/>"Desviar"
    Defense --> Scoring : Defensa tipo<br/>"Admitir" o "Negar"
    
    DefenseVoting --> Scoring : Votación<br/>completada
    
    Scoring --> CaseVoting : Más rondas<br/>pendientes
    Scoring --> Finished : Rondas<br/>completadas
    
    Finished --> [*]
    
    note right of Lobby
        Creación de sala
        Unión de jugadores
        Selección de GameMode
    end note
    
    note right of CaseVoting
        Caso aleatorio
        Acusaciones (1-2)
        Predicciones (1)
    end note
    
    note right of Defense
        Acusado inicial decide:
        - Admitir
        - Negar
        - Desviar
    end note
    
    note right of Scoring
        Cálculo de puntos
        Asignación de título
        Actualización ranking
    end note
```

Una partida progresa por varias fases:

1. **Lobby**
   - Se crea la sala y se elige `GameMode`.
   - Los jugadores se unen introduciendo alias (vía QR + web).
   - Cuando hay al menos 4 jugadores, se puede iniciar la partida.

2. **CaseVoting**
   - El motor selecciona un `CaseDefinition` aleatorio compatible con el `GameMode`.
   - Todos los jugadores:
     - emiten **acusaciones** (1–2 acusados, excepto a sí mismos),
     - realizan una **predicción** (quién creen que será el acusado según la mayoría).

3. **Defense**
   - Se calcula el **acusado inicial**: el jugador con más votos de acusación.
   - Este jugador elige una opción de defensa entre un conjunto discreto (configurable), por ejemplo:
     - `Admitir` (reconocimiento jocoso),
     - `Negar` (rechazo del papel),
     - `Desviar` (señalar a otro jugador como más adecuado para el caso).
   - En caso de `Desviar`, se registra un `altAccused`.

4. **DefenseVoting**
   - Si la defensa ha sido de desvío:
     - El resto de jugadores (según reglas) vota si acepta o no la defensa.
     - Voto binario: `Aceptar` / `Rechazar`.
   - Si la defensa no es de desvío, esta fase puede omitirse o simplificarse.

5. **Scoring**
   - Se decide el **acusado final**:
     - Si no hay desvío → acusado final = acusado inicial.
     - Si hay desvío → acusado final = acusado alternativo si la defensa se acepta por mayoría; en caso contrario sigue siendo el acusado inicial.
   - Se otorgan puntos según reglas (ver sección siguiente).
   - Se asigna al acusado final el **título social** asociado al `CaseDefinition`.
   - Se actualizan puntuaciones de todos los jugadores.

6. **Finished**
   - Tras N rondas/casos (configurable), la partida termina.
   - Se muestran:
     - ranking de puntuaciones,
     - títulos acumulados por jugador,
     - “perfil social” derivado de los títulos.

Toda la transición entre fases la gestiona el `GameEngine` con métodos que garantizan la coherencia del flujo de juego.

---

## 4. Reglas de votación y defensa

### 4.1 Acusaciones

- Cada jugador puede:
  - votar a 1 o 2 acusados por caso (configurable).
  - no se permite votar a uno mismo.
- El acusado inicial se calcula como el jugador con mayor número de votos de acusación.
- En caso de empate, se aplican reglas deterministas, por ejemplo:
  - desempatar por jugador con más títulos previos de la misma categoría,
  - o por orden de jugador en la lista si se quiere algo más simple.

### 4.2 Predicciones

- Cada jugador elige exactamente 1 nombre en la fase de predicción:
  - su hipótesis sobre quién será el acusado.
- Las predicciones no afectan al veredicto; sólo al sistema de puntuación.

### 4.3 Defensa

- El acusado inicial escoge una **opción discreta** de defensa. Ejemplo de opciones:

  - `Admitir`:
    - mensaje del estilo “lo admito, suena a algo que haría”.
    - no cambia el acusado, pero puede otorgar puntos de “autoaceptación” si se quiere.

  - `Negar`:
    - mensaje del estilo “no me encaja nada conmigo”.
    - no cambia el acusado.

  - `Desviar`:
    - el acusado elige a otro jugador como “culpable alternativo”.
    - activa una fase de votación de defensa.

### 4.4 Votación de defensa

- Sólo se aplica si hubo defensa de desvío.
- El resto de jugadores vota `Aceptar` / `Rechazar`.
- Si hay mayoría de "Aceptar":
  - acusado final = `altAccused`.
- Si hay mayoría de "Rechazar" o empate:
  - acusado final = acusado inicial.

```mermaid
flowchart TD
    Start[Inicio de Ronda] --> SelectCase[Seleccionar CaseDefinition]
    SelectCase --> Vote[Fase CaseVoting]
    
    Vote --> Accuse[Todos los jugadores<br/>acusan 1-2 personas]
    Accuse --> Predict[Todos los jugadores<br/>predicen quién será acusado]
    
    Predict --> CalcAccused[Calcular acusado inicial<br/>jugador con más votos]
    
    CalcAccused --> Defense{Acusado inicial<br/>elige defensa}
    
    Defense -->|Admitir| FinalAdmit[Acusado final =<br/>Acusado inicial]
    Defense -->|Negar| FinalDeny[Acusado final =<br/>Acusado inicial]
    Defense -->|Desviar| SelectAlt[Acusado selecciona<br/>jugador alternativo]
    
    SelectAlt --> DefenseVote[Resto de jugadores<br/>votan Aceptar/Rechazar]
    
    DefenseVote --> VoteResult{Mayoría<br/>acepta?}
    
    VoteResult -->|Sí| FinalAlt[Acusado final =<br/>Acusado alternativo]
    VoteResult -->|No/Empate| FinalOriginal[Acusado final =<br/>Acusado inicial]
    
    FinalAdmit --> Score[Calcular puntuación]
    FinalDeny --> Score
    FinalAlt --> Score
    FinalOriginal --> Score
    
    Score --> AssignTitle[Asignar título social<br/>al acusado final]
    AssignTitle --> UpdateScores[Actualizar puntuaciones]
    
    UpdateScores --> CheckRounds{Quedan<br/>rondas?}
    
    CheckRounds -->|Sí| Start
    CheckRounds -->|No| End[Fase Finished<br/>Ranking y títulos]
    
    style Start fill:#e1f5ff
    style Vote fill:#fff4e1
    style Defense fill:#ffe1e1
    style Score fill:#e8f5e9
    style End fill:#f3e5f5
```

---

## 5. Sistema de puntuación (ejemplo base)

```mermaid
flowchart LR
    subgraph Puntuación
        A[Acusaciones correctas] -->|+2 pts| Points[Puntos totales]
        P[Predicciones correctas] -->|+1 pt| Points
        D[Defensa exitosa<br/>desvío aceptado] -->|+2 pts al<br/>acusado original| Points
    end
    
    subgraph Títulos
        Points --> Title[Acusado final<br/>recibe título social]
        Title --> Persist[Título persistente<br/>en perfil del jugador]
    end
    
    style A fill:#e8f5e9
    style P fill:#e1f5ff
    style D fill:#fff4e1
    style Title fill:#f3e5f5
```

### Reglas de puntuación

La lógica concreta puede ajustarse, pero un esquema base podría ser:

- **Acusaciones:**
  - Cada jugador que acusó al acusado final:
    - +2 puntos.

- **Predicciones:**
  - Cada jugador cuya predicción coincide con el acusado final:
    - +1 punto.

- **Defensa:**
  - Si un acusado logra desviar la culpa (defensa aceptada):
    - +2 puntos al acusado original (por “labia”).
  - Si falla:
    - 0 puntos adicionales (ya ha tenido su momento social).

- **Títulos sociales:**
  - El acusado final recibe el título asociado al caso.
  - Los títulos no añaden puntos extra por sí mismos (o sí, si se quiere), pero sirven como “logros” sociales persistentes.

Esta lógica se centraliza en una operación de `GameEngine.ScoreRound(Room room)`.

---

## 6. Títulos sociales y memoria de la partida

Cada `CaseDefinition` tiene asociado un **título social** estático (predefinido), por ejemplo:

- Caso: “¿Quién es más probable que llegue siempre tarde?”  
  → Título: “Señor/a de la impuntualidad crónica”.

Cuando se resuelve una ronda, el título se asigna al acusado final y se registra un `TitleAssignment`.

Al final de la partida, la pantalla principal muestra, por ejemplo:

- Para cada jugador:
  - Puntos totales.
  - Lista de títulos recibidos.
- Para el grupo:
  - Ranking de títulos más repetidos,
  - “perfil social” del grupo (e.g. “Mucha impuntualidad y mucho drama”).

Este sistema de títulos está pensado para generar conversaciones posteriores (“tú eres el desaparecedor oficial en fiestas”, etc.).

---

## 7. Rol de la IA generativa en la lógica de juego

```mermaid
sequenceDiagram
    participant GE as GameEngine
    participant Room as Room State
    participant AI as ICommentaryService
    participant UI as Pantalla Principal
    
    GE->>Room: Iniciar nueva ronda
    GE->>Room: Calcular acusado final
    GE->>Room: Calcular puntuaciones
    GE->>Room: Asignar título social
    
    Note over GE,Room: Lógica determinista<br/>completamente independiente de IA
    
    GE->>AI: Solicitar comentario<br/>(resumen de estado)
    AI-->>GE: Texto generado<br/>(sin afectar estado)
    
    GE->>UI: Actualizar estado de juego
    GE->>UI: Mostrar comentario IA
    
    Note over AI: Solo genera texto<br/>NO modifica Room, Round,<br/>ni puntuaciones
```

La IA **no forma parte** de la lógica de juego. Detalles clave:

- No decide casos, votos ni resultados.
- No calcula puntuaciones ni títulos.
- No altera el estado de `Room`, `Round`, etc.

Su rol es únicamente:

1. Recibir resúmenes de:
   - caso jugado,
   - acusado final,
   - votos agregados,
   - puntuaciones,
   - títulos asignados.
2. Generar texto breve de comentario:
   - introducción a la ronda,
   - reacción al veredicto,
   - resumen final de partida.

Esto permite mantener el **motor de juego totalmente determinista**, al tiempo que se añade una capa de entretenimiento dinámico.

---