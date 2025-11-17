# Requisitos y Casos de Uso

Este documento recoge los requisitos funcionales, no funcionales y los casos de uso asociados al juego "Pandorium". Todos los requisitos funcionales se han extraído o derivado del documento `docs/game-logic.md` para cubrir exhaustivamente la casuística descrita.

---

## Requisitos Funcionales

### Gestión de sala y lobby

- RF-001: El sistema debe permitir crear una sala con un código único y aleatorio (`RoomCode`).
- RF-002: Debe poder configurarse el modo de juego (`GameMode`): `Suave`, `Normal` o `Spicy`.
- RF-003: La sala debe admitir entre 4 y 16 jugadores simultáneos; mientras haya menos de 4, el inicio de partida no será posible.
- RF-004: El host debe ver y compartir un código QR que incluya la URL y el código de sala para facilitar la incorporación de jugadores.
- RF-005: Debe existir una fase de lobby donde los jugadores introduzcan su alias y se unan antes de iniciar la partida.
- RF-006: El sistema debe impedir alias duplicados dentro de una misma sala.

### Gestión de jugadores y conexiones

- RF-010: Los jugadores acceden mediante un navegador y se identifican con un alias sin autenticación adicional.
- RF-011: El sistema debe sincronizar el estado de la sala en tiempo real entre todos los dispositivos (host y jugadores) mediante SignalR.
- RF-012: Si un jugador pierde la conexión, debe poder reconectarse y recuperar su estado siempre que la partida siga activa.
- RF-013: El sistema debe mostrar el estado de cada jugador como `Conectado`, `Inactivo` o `Desconectado` dentro de la sala.
- RF-014: El estado `Inactivo` se aplicará automáticamente tras 30 segundos sin actividad y se revertirá al detectar interacción del jugador.
- RF-015: El estado `Desconectado` se establecerá tras 5 minutos de inactividad continuada y permanecerá hasta que el jugador se reincorpore.
- RF-016: Los cambios de estado deben propagarse en tiempo real a todos los clientes conectados mediante SignalR.

### Flujo de partida y fases

- RF-020: Una vez iniciada la partida, el sistema debe gestionar las fases en el orden determinista descrito: `Lobby` → `CaseVoting` → `Defense` (opcional) → `DefenseVoting` (condicional) → `Scoring` → (`CaseVoting` | `Finished`).
- RF-021: En la fase `CaseVoting`, el motor debe seleccionar un caso (`CaseDefinition`) compatible con el modo de juego de la sala.
- RF-022: Cada jugador debe poder emitir hasta dos acusaciones por ronda hacia otros jugadores, nunca hacia sí mismo.
- RF-023: Cada jugador debe realizar una única predicción indicando quién cree que será el acusado final.
- RF-024: El sistema debe calcular el acusado inicial como el jugador con mayor número de acusaciones recibidas.
- RF-025: En caso de empate en acusaciones, debe aplicarse una regla determinista definida por diseño (por ejemplo, orden interno de jugadores o títulos previos) para elegir al acusado inicial.
- RF-026: El acusado inicial debe poder escoger entre las opciones de defensa configuradas (`Admitir`, `Negar`, `Desviar`).
- RF-027: Si el acusado selecciona `Desviar`, el sistema debe permitirle elegir a otro jugador como acusado alternativo (`altAccused`).
- RF-028: Tras una defensa de desvío, el resto de jugadores (excluyendo al acusado original) debe votar `Aceptar` o `Rechazar` la defensa.
- RF-029: El acusado final debe determinarse en función del resultado de la votación de defensa: si la mayoría acepta, el acusado alternativo pasa a ser el acusado final; en caso contrario o empate, se mantiene el acusado inicial.
- RF-030: Si la defensa no es de desvío (`Admitir` o `Negar`), la fase de votación de defensa debe omitirse y el acusado final será el acusado inicial.
- RF-031: El sistema debe permitir configurar el número total de rondas o casos que se jugarán antes de finalizar la partida.

### Puntuación y títulos

- RF-040: Al finalizar cada ronda, el sistema debe asignar puntos conforme a las reglas de puntuación: acusaciones acertadas (+2), predicciones acertadas (+1), defensas exitosas de desvío para el acusado original (+2) o la configuración que se defina.
- RF-041: Las reglas de puntuación deben permitir ajustes o parametrización futura sin romper la lógica central.
- RF-042: El acusado final debe recibir el título social asociado al `CaseDefinition` jugado en esa ronda.
- RF-043: Los títulos deben persistir asociados al jugador durante toda la partida y mostrarse en los resultados finales.

### Comentarios generados por IA

- RF-050: Tras cada ronda o evento relevante, el sistema debe invocar al servicio de comentarista IA (`ICommentaryService`) proporcionando un resumen determinista del estado de juego.
- RF-051: El comentario generado debe presentarse en la interfaz principal sin alterar el estado ni las decisiones del motor de juego.
- RF-052: El sistema debe permitir operar sin el servicio de IA (modo fallback) usando mensajes predefinidos si la integración falla.

### Persistencia y auditoría

- RF-060: Todos los elementos de la partida (salas, jugadores, rondas, acusaciones, predicciones, defensas, resultados y títulos) deben persistirse mediante la capa de infraestructura (EF Core).
- RF-061: El sistema debe registrar la secuencia de eventos clave para posibilitar seguimiento y auditoría básica (creación de sala, inicio de ronda, votos, defensas, puntuaciones).
- RF-062: Debe ser posible consultar el histórico de partidas y rondas una vez finalizadas (opcional según alcance).

### Reglas de negocio transversales

- RF-070: El motor de juego debe ser determinista y reproducible, de forma que dados los mismos inputs se obtenga el mismo resultado.
- RF-071: El sistema debe impedir acciones fuera de fase (por ejemplo, enviar acusaciones durante `Defense`).
- RF-072: Deben validarse las entradas de los jugadores (alias, acusaciones, predicciones, defensas) para garantizar que cumplen las reglas (longitud, jugador válido, fase correcta).
- RF-073: El sistema debe gestionar de forma clara el final de partida, mostrando ranking, títulos acumulados y un resumen final.
- RF-074: La lógica de juego no debe depender de la IA generativa; si el módulo IA no está disponible, la partida debe continuar con normalidad.

---

## Requisitos No Funcionales

- RNF-001: La aplicación debe funcionar en navegadores modernos con soporte para WebSockets: Chrome, Edge, Safari y Firefox en sus versiones actuales.
- RNF-002: El tiempo de respuesta en acciones críticas (envío de acusaciones, defensas, votaciones) debe mantenerse por debajo de 500 ms en condiciones nominales.
- RNF-003: La solución debe soportar sesiones con entre 4 y 16 jugadores concurrentes conectados desde dispositivos BYOD.
- RNF-004: El motor de juego y la comunicación deben tolerar latencias de red irregulares sin comprometer la consistencia del estado.
- RNF-005: La arquitectura debe ser desplegable tanto en entornos Linux como Windows, en servidores propios o servicios cloud comunes.
- RNF-006: La persistencia debe garantizar integridad referencial y transaccional en operaciones que afecten al estado de la partida.
- RNF-007: Deben existir mecanismos básicos de logging y monitorización para diagnosticar incidencias.
- RNF-008: La interfaz debe ser responsiva y usable en dispositivos móviles (pantalla vertical) y en pantallas de gran formato.

---

## Casos de Uso Principales

### CU-01 Crear sala de juego
- Actor: Host.
- Precondición: Aplicación disponible.
- Flujo: El host selecciona modo de juego y crea la sala; el sistema genera `RoomCode` y muestra QR.
- Resultado: Sala en estado `Lobby` lista para recibir jugadores.

### CU-02 Unirse a sala como jugador
- Actor: Jugador.
- Precondición: Sala existente en `Lobby`.
- Flujo: El jugador escanea el QR o introduce el código, define alias y se une; el sistema valida alias y sincroniza estado.
- Resultado: Jugador añadido a la sala y visible para todos.

### CU-03 Emitir acusaciones y predicciones
- Actor: Jugador.
- Precondición: Sala en fase `CaseVoting`; caso activo.
- Flujo: El jugador selecciona hasta dos acusados válidos y una predicción; el sistema valida que no se acuse a sí mismo y registra la acción.
- Resultado: Acusaciones y predicción almacenadas; se notifica al resto si procede.

### CU-04 Defenderse o desviar acusación
- Actor: Acusado inicial.
- Precondición: Sala en fase `Defense`; acusado inicial determinado.
- Flujo: El acusado elige opción (`Admitir`, `Negar`, `Desviar`); si es desvío, selecciona acusado alternativo; el sistema valida y avanza de fase.
- Resultado: Defensa registrada; si procede, se inicia `DefenseVoting`.

### CU-05 Votar defensa
- Actor: Resto de jugadores.
- Precondición: Defensa de tipo `Desviar` activa; sala en `DefenseVoting`.
- Flujo: Cada jugador emite voto binario `Aceptar`/`Rechazar`; el sistema recoge votos y calcula mayoría.
- Resultado: Determinación de acusado final según resultado de la votación.

### CU-06 Calcular puntuaciones y asignar títulos
- Actor: Motor de juego.
- Precondición: Finalizada fase de defensa.
- Flujo: El sistema calcula puntuaciones según reglas, asigna título al acusado final y acumula resultados.
- Resultado: Puntuaciones y títulos actualizados; se notifica al host y jugadores.

### CU-07 Mostrar resultados y comentar partida
- Actor: Host / Jugadores.
- Precondición: Tras `Scoring` o fin de partida.
- Flujo: El sistema actualiza ranking y títulos en pantalla, invoca al comentarista IA (si disponible) y muestra el comentario generado.
- Resultado: Interfaz sincronizada con resumen de la ronda o fin de partida.

### CU-08 Finalizar partida y presentar resumen
- Actor: Host / Sistema.
- Precondición: Todas las rondas configuradas completadas.
- Flujo: El sistema cambia a fase `Finished`, muestra ranking final, títulos por jugador y resumen global.
- Resultado: Partida cerrada; jugadores pueden abandonar la sala.

---

Este documento debe revisarse y ampliarse cuando se introduzcan nuevas reglas, modos de juego o variaciones en la lógica descrita en `docs/game-logic.md`.
