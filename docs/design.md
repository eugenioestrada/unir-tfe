# Diseño Técnico

Este documento amplía la arquitectura y detalla los principales diagramas y especificaciones técnicas del proyecto.

---

## Diagramas

- **Diagrama de despliegue:**
  - Servidor ASP.NET Core + SignalR
  - Base de datos relacional (SQL Server/PostgreSQL)
  - Clientes web (pantalla principal y móviles)

- **Diagrama de secuencia (ejemplo):**
  - Inicio de partida
  - Flujo de acusación y defensa
  - Asignación de títulos

- **Modelo de datos (ERD):**
  - Entidades: Room, Player, Round, CaseDefinition, TitleAssignment

---

## Especificación de Interfaces

- **SignalR Hub:**
  - Métodos: JoinRoom, StartGame, SendAccusation, SendPrediction, SendDefense, VoteDefense, GetResults
  - Eventos: RoomUpdated, RoundStarted, AccusationReceived, DefenseVoted, ResultsAvailable

- **DTOs principales:**
  - RoomDto, PlayerDto, RoundDto, CaseDto, TitleDto

---

(Completar con diagramas y detalles según avance)
