# Tribunal Social – Juego social web multi-dispositivo

**Tribunal Social** es un juego social de salón pensado para grupos de **4 a 16 jugadores** que comparten una misma sala física.

Cada partida se juega con:
- una **pantalla principal** (ordenador o Smart TV con navegador), y  
- los **móviles/tablets** de los jugadores, sin instalación de apps (solo navegador + QR).

El juego plantea **casos o situaciones sociales** en los que los jugadores:
- acusan en secreto a quien creen que encaja mejor en cada caso,
- realizan apuestas sobre qué decidirá la mayoría,
- y resuelven defensas y contraacusaciones.

El sistema asigna **títulos sociales** (insignias) a los jugadores según las decisiones del grupo.  
Un **comentarista automático** basado en IA generativa narra lo que ocurre (acusados, resultados, títulos) sin intervenir en las reglas ni en el resultado del juego.

---

## Características principales

- Party game web **zero-install** (solo navegador y código QR).
- Admite **4–16 jugadores** en la misma sala física.
- Mecánicas pensadas para:
  - generar risas y pequeñas fricciones amistosas,
  - crear “títulos sociales” y running gags para el grupo.
- Arquitectura **.NET 10** con:
  - ASP.NET Core, SignalR, Blazor, EF Core, Aspire.
- Motor de juego **determinista**, desacoplado de la IA.
- Módulo de comentariado con **IA generativa** como capa extra de ambientación.

---

## Estructura de la documentación

- [`docs/architecture.md`](docs/architecture.md)  
  Arquitectura de la solución, proyectos, capas y comunicación entre componentes.

- [`docs/game-logic.md`](docs/game-logic.md)  
  Modelo de dominio, fases del juego, reglas de votación, defensa y puntuación.

- [`docs/technology.md`](docs/technology.md)  
  Stack tecnológico, decisiones de diseño, pruebas y validación.

---

## Estado del proyecto

- [ ] Modelo de dominio (entidades y motor de juego).  
- [ ] Persistencia con EF Core y migraciones iniciales.  
- [ ] Hubs de SignalR y contrato de mensajes.  
- [ ] Interfaz Blazor (pantalla principal y jugadores).  
- [ ] Integración con IA generativa para el comentarista.  
- [ ] Pruebas técnicas (unitarias, integración, carga) y pruebas con usuarios.

---

## Licencia

_(Pendiente de definir: MIT, Apache 2.0, etc.)_
