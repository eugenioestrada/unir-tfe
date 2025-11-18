# Requisitos y Casos de Uso

Este documento recoge los requisitos funcionales, no funcionales y los casos de uso asociados al juego "Pandorium". Todos los requisitos funcionales se han extraído o derivado del documento `docs/game-logic.md` para cubrir exhaustivamente la casuística descrita.

---

## Requisitos Funcionales

### Gestión de sala y lobby

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RF-001 | Crear sala con código único. | El sistema debe permitir crear una sala con un código único y aleatorio (`RoomCode`). | Completado |
| RF-002 | Configurar modo de juego. | Debe poder configurarse el modo de juego (`GameMode`): `Suave`, `Normal` o `Spicy`. | Completado |
| RF-003 | Limitar jugadores por sala. | La sala debe admitir entre 4 y 16 jugadores simultáneos; mientras haya menos de 4, el inicio de partida no será posible. | Completado |
| RF-004 | Mostrar QR de acceso. | El host debe ver y compartir un código QR que incluya la URL y el código de sala para facilitar la incorporación de jugadores. | Completado |
| RF-005 | Gestionar fase de lobby. | Debe existir una fase de lobby donde los jugadores introduzcan su alias y se unan antes de iniciar la partida. | Completado |
| RF-006 | Evitar alias duplicados. | El sistema debe impedir alias duplicados dentro de una misma sala. | Completado |
| RF-007 | Compartir enlace nativo. | El host debe poder compartir la URL de la sala mediante los mecanismos nativos del navegador (por ejemplo, la Web Share API) como alternativa al escaneo del código QR. | Completado |

### Gestión de jugadores y conexiones

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RF-010 | Acceso web con alias. | Los jugadores acceden mediante un navegador y se identifican con un alias sin autenticación adicional. | Completado |
| RF-011 | Sincronizar estado en tiempo real. | El sistema debe sincronizar el estado de la sala en tiempo real entre todos los dispositivos (host y jugadores) mediante SignalR. | Completado |
| RF-012 | Permitir reconexión de jugador. | Si un jugador pierde la conexión, debe poder reconectarse y recuperar su estado siempre que la partida siga activa. | En progreso |
| RF-013 | Mostrar estado de conexión. | El sistema debe mostrar el estado de cada jugador como `Conectado`, `Inactivo` o `Desconectado` dentro de la sala. | Completado |
| RF-014 | Marcar inactividad a 30s. | El estado `Inactivo` se aplicará automáticamente tras 30 segundos sin actividad y se revertirá al detectar interacción del jugador. | Completado |
| RF-015 | Marcar desconexión a 5min. | El estado `Desconectado` se establecerá tras 5 minutos de inactividad continuada y permanecerá hasta que el jugador se reincorpore. | Completado |
| RF-016 | Propagar cambios de estado. | Los cambios de estado deben propagarse en tiempo real a todos los clientes conectados mediante SignalR. | Completado |
| RF-017 | Restaurar vista tras refresco. | Tras refrescar la página, el host y los jugadores deben recuperar automáticamente su vista correspondiente (lobby, jugador o fase activa) siempre que la partida no haya finalizado. | Completado |

### Flujo de partida y fases

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RF-020 | Gestionar fases deterministas. | Una vez iniciada la partida, el sistema debe gestionar las fases en el orden determinista descrito: `Lobby` → `CaseVoting` → `Defense` (opcional) → `DefenseVoting` (condicional) → `Scoring` → (`CaseVoting` \| `Finished`). | Pendiente |
| RF-021 | Seleccionar caso por modo. | En la fase `CaseVoting`, el motor debe seleccionar un caso (`CaseDefinition`) compatible con el modo de juego de la sala. | Pendiente |
| RF-022 | Limitar acusaciones por jugador. | Cada jugador debe poder emitir hasta dos acusaciones por ronda hacia otros jugadores, nunca hacia sí mismo. | Pendiente |
| RF-023 | Registrar predicción única. | Cada jugador debe realizar una única predicción indicando quién cree que será el acusado final. | Pendiente |
| RF-024 | Calcular acusado inicial. | El sistema debe calcular el acusado inicial como el jugador con mayor número de acusaciones recibidas. | Pendiente |
| RF-025 | Resolver empates deterministas. | En caso de empate en acusaciones, debe aplicarse una regla determinista definida por diseño (por ejemplo, orden interno de jugadores o títulos previos) para elegir al acusado inicial. | Pendiente |
| RF-026 | Ofrecer opciones de defensa. | El acusado inicial debe poder escoger entre las opciones de defensa configuradas (`Admitir`, `Negar`, `Desviar`). | Pendiente |
| RF-027 | Permitir acusado alternativo. | Si el acusado selecciona `Desviar`, el sistema debe permitirle elegir a otro jugador como acusado alternativo (`altAccused`). | Pendiente |
| RF-028 | Votar defensa de desvío. | Tras una defensa de desvío, el resto de jugadores (excluyendo al acusado original) debe votar `Aceptar` o `Rechazar` la defensa. | Pendiente |
| RF-029 | Determinar acusado final. | El acusado final debe determinarse en función del resultado de la votación de defensa: si la mayoría acepta, el acusado alternativo pasa a ser el acusado final; en caso contrario o empate, se mantiene el acusado inicial. | Pendiente |
| RF-030 | Omitir votación sin desvío. | Si la defensa no es de desvío (`Admitir` o `Negar`), la fase de votación de defensa debe omitirse y el acusado final será el acusado inicial. | Pendiente |
| RF-031 | Configurar número de rondas. | El sistema debe permitir configurar el número total de rondas o casos que se jugarán antes de finalizar la partida. | Pendiente |
| RF-032 | Finalizar partida anticipadamente. | El host debe poder finalizar la partida de forma anticipada, cerrando la sala y mostrando el resumen correspondiente. | Pendiente |

### Puntuación y títulos

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RF-040 | Asignar puntos por reglas. | Al finalizar cada ronda, el sistema debe asignar puntos conforme a las reglas de puntuación: acusaciones acertadas (+2), predicciones acertadas (+1), defensas exitosas de desvío para el acusado original (+2) o la configuración que se defina. | Pendiente |
| RF-041 | Parametrizar puntuación. | Las reglas de puntuación deben permitir ajustes o parametrización futura sin romper la lógica central. | Pendiente |
| RF-042 | Asignar título al acusado. | El acusado final debe recibir el título social asociado al `CaseDefinition` jugado en esa ronda. | Pendiente |
| RF-043 | Persistir títulos por partida. | Los títulos deben persistir asociados al jugador durante toda la partida y mostrarse en los resultados finales. | Pendiente |

### Comentarios generados por IA

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RF-050 | Invocar comentarista IA. | Tras cada ronda o evento relevante, el sistema debe invocar al servicio de comentarista IA (`ICommentaryService`) proporcionando un resumen determinista del estado de juego. | Pendiente |
| RF-051 | Mostrar comentario sin efectos. | El comentario generado debe presentarse en la interfaz principal sin alterar el estado ni las decisiones del motor de juego. | Pendiente |
| RF-052 | Activar fallback sin IA. | El sistema debe permitir operar sin el servicio de IA (modo fallback) usando mensajes predefinidos si la integración falla. | Pendiente |

### Persistencia y auditoría

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RF-060 | Persistir elementos de partida. | Todos los elementos de la partida (salas, jugadores, rondas, acusaciones, predicciones, defensas, resultados y títulos) deben persistirse mediante la capa de infraestructura (EF Core). | Pendiente |
| RF-061 | Registrar eventos clave. | El sistema debe registrar la secuencia de eventos clave para posibilitar seguimiento y auditoría básica (creación de sala, inicio de ronda, votos, defensas, puntuaciones). | Pendiente |
| RF-062 | Consultar histórico de partidas. | Debe ser posible consultar el histórico de partidas y rondas una vez finalizadas (opcional según alcance). | Pendiente |

### Reglas de negocio transversales

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RF-070 | Garantizar motor determinista. | El motor de juego debe ser determinista y reproducible, de forma que dados los mismos inputs se obtenga el mismo resultado. | Pendiente |
| RF-071 | Impedir acciones fuera de fase. | El sistema debe impedir acciones fuera de fase (por ejemplo, enviar acusaciones durante `Defense`). | Pendiente |
| RF-072 | Validar entradas de jugadores. | Deben validarse las entradas de los jugadores (alias, acusaciones, predicciones, defensas) para garantizar que cumplen las reglas (longitud, jugador válido, fase correcta). | Pendiente |
| RF-073 | Gestionar fin de partida. | El sistema debe gestionar de forma clara el final de partida, mostrando ranking, títulos acumulados y un resumen final. | Pendiente |
| RF-074 | Desacoplar lógica de la IA. | La lógica de juego no debe depender de la IA generativa; si el módulo IA no está disponible, la partida debe continuar con normalidad. | Pendiente |

---

## Requisitos No Funcionales

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| RNF-001 | Soportar navegadores modernos. | La aplicación debe funcionar en navegadores modernos con soporte para WebSockets: Chrome, Edge, Safari y Firefox en sus versiones actuales. | Pendiente |
| RNF-002 | Responder en <500 ms. | El tiempo de respuesta en acciones críticas (envío de acusaciones, defensas, votaciones) debe mantenerse por debajo de 500 ms en condiciones nominales. | Pendiente |
| RNF-003 | Soportar 4-16 jugadores. | La solución debe soportar sesiones con entre 4 y 16 jugadores concurrentes conectados desde dispositivos BYOD. | Completado |
| RNF-004 | Tolerar latencias irregulares. | El motor de juego y la comunicación deben tolerar latencias de red irregulares sin comprometer la consistencia del estado. | En progreso |
| RNF-005 | Desplegar en Linux y Windows. | La arquitectura debe ser desplegable tanto en entornos Linux como Windows, en servidores propios o servicios cloud comunes. | Pendiente |
| RNF-006 | Mantener integridad transaccional. | La persistencia debe garantizar integridad referencial y transaccional en operaciones que afecten al estado de la partida. | Pendiente |
| RNF-007 | Disponer de logging básico. | Deben existir mecanismos básicos de logging y monitorización para diagnosticar incidencias. | En progreso |
| RNF-008 | Interfaz responsive. | La interfaz debe ser responsiva y usable en dispositivos móviles (pantalla vertical) y en pantallas de gran formato. | Completado |
| RNF-009 | Preservar contexto tras recarga. | La experiencia debe preservar el contexto de los usuarios ante recargas de página, restableciendo la sesión y el estado visible sin intervención manual mientras la partida siga activa. | Pendiente |
| RNF-010 | Adaptar layout a pantalla completa. | La interfaz debe ajustarse dinámicamente al viewport disponible, redistribuyendo componentes para ocupar toda la superficie útil sin requerir desplazamiento vertical u horizontal. | Pendiente |
| RNF-011 | Alinear con principios de diseño. | Toda funcionalidad y componente de UI debe alinearse con los lineamientos vigentes en `docs/ui-design-principles.md`; cualquier desviación requiere actualizar la guía antes de su implementación. | Pendiente |

---

## Casos de Uso Principales

| ID | Título | Descripción | Estado |
|----|--------|-------------|--------|
| CU-01 | Crear sala de juego. | Actor: Host.<br>Precondición: Aplicación disponible.<br>Flujo: El host selecciona modo de juego y crea la sala; el sistema genera `RoomCode` y muestra QR.<br>Resultado: Sala en estado `Lobby` lista para recibir jugadores. | Completado |
| CU-02 | Unirse a sala como jugador. | Actor: Jugador.<br>Precondición: Sala existente en `Lobby`.<br>Flujo: El jugador escanea el QR o introduce el código, define alias y se une; el sistema valida alias y sincroniza estado.<br>Resultado: Jugador añadido a la sala y visible para todos. | Completado |
| CU-03 | Emitir acusaciones y predicciones. | Actor: Jugador.<br>Precondición: Sala en fase `CaseVoting`; caso activo.<br>Flujo: El jugador selecciona hasta dos acusados válidos y una predicción; el sistema valida que no se acuse a sí mismo y registra la acción.<br>Resultado: Acusaciones y predicción almacenadas; se notifica al resto si procede. | Pendiente |
| CU-04 | Defenderse o desviar acusación. | Actor: Acusado inicial.<br>Precondición: Sala en fase `Defense`; acusado inicial determinado.<br>Flujo: El acusado elige opción (`Admitir`, `Negar`, `Desviar`); si es desvío, selecciona acusado alternativo; el sistema valida y avanza de fase.<br>Resultado: Defensa registrada; si procede, se inicia `DefenseVoting`. | Pendiente |
| CU-05 | Votar defensa. | Actor: Resto de jugadores.<br>Precondición: Defensa de tipo `Desviar` activa; sala en `DefenseVoting`.<br>Flujo: Cada jugador emite voto binario `Aceptar`/`Rechazar`; el sistema recoge votos y calcula mayoría.<br>Resultado: Determinación de acusado final según resultado de la votación. | Pendiente |
| CU-06 | Calcular puntuaciones y asignar títulos. | Actor: Motor de juego.<br>Precondición: Finalizada fase de defensa.<br>Flujo: El sistema calcula puntuaciones según reglas, asigna título al acusado final y acumula resultados.<br>Resultado: Puntuaciones y títulos actualizados; se notifica al host y jugadores. | Pendiente |
| CU-07 | Mostrar resultados y comentar partida. | Actor: Host / Jugadores.<br>Precondición: Tras `Scoring` o fin de partida.<br>Flujo: El sistema actualiza ranking y títulos en pantalla, invoca al comentarista IA (si disponible) y muestra el comentario generado.<br>Resultado: Interfaz sincronizada con resumen de la ronda o fin de partida. | Pendiente |
| CU-08 | Finalizar partida y presentar resumen. | Actor: Host / Sistema.<br>Precondición: Todas las rondas configuradas completadas.<br>Flujo: El sistema cambia a fase `Finished`, muestra ranking final, títulos por jugador y resumen global.<br>Resultado: Partida cerrada; jugadores pueden abandonar la sala. | Pendiente |

---

Este documento debe revisarse y ampliarse cuando se introduzcan nuevas reglas, modos de juego o variaciones en la lógica descrita en `docs/game-logic.md`.
