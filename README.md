# Tribunal Social – Plataforma web de juego social multi-dispositivo

Tribunal Social es un juego social de salón pensado para grupos de **4 a 16 jugadores** que comparten una misma sala física.  
Cada partida se juega con:

- una **pantalla principal** (ordenador o Smart TV con navegador), y  
- los **móviles/tablets** de los jugadores, sin instalación de apps (solo navegador + QR).

El objetivo del proyecto es proporcionar un **party game web zero-install** que genere votaciones, acusaciones y “títulos sociales” entre amigos, con un **comentarista automático** basado en IA generativa que narra lo que ocurre sin intervenir en las reglas del juego.

---

## Objetivos del proyecto

- Diseñar e implementar un **juego social** con mecánicas simples pero ricas en interacción:
  - casos/situaciones sociales,
  - acusaciones secretas,
  - defensas y contraataques,
  - títulos sociales persistentes.
- Ofrecer una experiencia **multi-dispositivo** sin instalación:
  - host: pantalla principal en web,
  - jugadores: acceso mediante **código QR** con sus móviles.
- Construir una arquitectura **.NET** clara y testable:
  - dominio independiente,
  - servicios de aplicación,
  - SignalR para tiempo real,
  - Blazor para la interfaz.
- Integrar un módulo de **IA generativa como comentarista**:
  - solo describe y comenta,
  - no modifica reglas ni resultados.

---

## Stack tecnológico y justificación

### Plataforma y lenguaje

- **.NET 10 + C#**
  - Plataforma madura, con soporte completo para:
    - ASP.NET Core,
    - SignalR,
    - EF Core,
    - Blazor.
  - Permite escribir **toda** la solución (backend + frontend Blazor) en C#, facilitando la coherencia del código y el mantenimiento.

- **Visual Studio 2026 Community**
  - IDE principal para desarrollo .NET.
  - Integración nativa con:
    - depuración,
    - herramientas de test,
    - soporte para Aspire,
    - tooling para Blazor y SignalR.

### Orquestación

- **Aspire**
  - Se usa para gestionar un **AppHost** que levanta:
    - la aplicación web,
    - la base de datos,
    - (opcionalmente) el servicio de IA.
  - Justificación:
    - facilita un *single entry point* de desarrollo,
    - define de forma declarativa los recursos (BD, endpoints),
    - simplifica la ejecución local y la futura despliegue orquestado.

### Comunicación y backend

- **ASP.NET Core**
  - Marco de trabajo principal para la aplicación web.
  - Ofrece:
    - pipeline HTTP configurable,
    - integración con SignalR,
    - sistema de inyección de dependencias para servicios del dominio y de infraestructura.

- **SignalR**
  - Canal de comunicación **bidireccional en tiempo real** entre:
    - pantalla principal,
    - clientes móviles.
  - Justificación:
    - modelo de programación orientado a *hubs* muy adecuado para salas de juego,
    - abstracción sobre WebSockets / Server-Sent Events,
    - integración directa con .NET y Blazor.

- **Entity Framework Core + BD relacional** (por ejemplo SQL Server/PostgreSQL)
  - EF Core facilita el acceso a datos mediante modelo de objetos (.NET) con migraciones y LINQ.
  - La BD relacional se usa para:
    - salas,
    - jugadores,
    - casos,
    - resultados y títulos sociales.
  - Justificación:
    - modelo de datos estructurado,
    - consultas relacionales (estadísticas, históricos),
    - tooling maduro para migraciones y pruebas.

### Interfaz de usuario

- **Blazor (Server)**
  - La UI de la pantalla principal y de los jugadores se implementa con componentes Blazor.
  - Justificación:
    - permite reutilizar C# en la capa de UI,
    - integración directa con SignalR,
    - buena experiencia para aplicaciones interactivas multi-vista (host vs jugador),
    - evita tener que mantener un SPA separado en JavaScript.

### Control de versiones e IA

- **Git + GitHub**
  - Git para control de versiones.
  - GitHub como repositorio remoto, issues, PR, Actions (CI).
  - Justificación:
    - estándar de facto,
    - facilita trazabilidad y colaboración,
    - integración con pipelines de CI (tests automatizados).

- **IA generativa (servicio externo por determinar)**
  - Se evaluarán proveedores compatibles con .NET para el comentarista automático.
  - Este servicio:
    - recibe resúmenes de rondas y estado,
    - genera texto corto de comentario.
  - Decisión tecnológica final basada en:
    - facilidad de integración (SDK/REST),
    - coste y latencia,
    - control de estilo.

---

## Arquitectura de la solución

La solución se organiza en **cuatro proyectos principales**:

1. `GameTribunal.Domain` – Modelo de dominio y reglas de juego.  
2. `GameTribunal.Application` – Casos de uso y orquestación.  
3. `GameTribunal.Infrastructure` – Persistencia e integración con servicios externos.  
4. `GameTribunal.Web` – Aplicación ASP.NET Core + Blazor + SignalR (host + jugadores).

Opcional:

- `GameTribunal.AppHost` – Proyecto Aspire para orquestar la aplicación y la BD.

### Diagrama lógico (simplificado)

```text
+-------------------+        +-----------------------+
|   Presentación    |        |   Infraestructura     |
|  (GameTribunal.Web)        | (GameTribunal.Infra)  |
| - Blazor (host/player)     | - EF Core + DB        |
| - SignalR Hubs             | - IA (comentario)     |
+-------------+-----+        +-----------+-----------+
              |                          ^
              v                          |
       +------+------+            +------+------+
       |  Aplicación  |            |   Dominio  |
       | (Application)|----------->| (Domain)   |
       | - RoomService|            | - Entidades|
       | - RoundService           | - GameEngine
       +-------------+            +-----------+
