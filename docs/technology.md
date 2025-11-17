# Stack tecnológico y decisiones de diseño

Este documento describe las tecnologías utilizadas en **Tribunal Social**, justificando las decisiones desde una perspectiva de ingeniería y de objetivos del proyecto.

---

## 1. Plataforma base

### .NET 10 y C#

Se utiliza **.NET 10** (versión LTS más reciente al inicio del proyecto) con **C#** como lenguaje principal para toda la lógica del proyecto:

- **Ventajas:**
  - Ecosistema unificado para backend, tiempo real y UI (Blazor).
  - Amplio soporte en herramientas y bibliotecas (ASP.NET Core, SignalR, EF Core).
  - Lenguaje fuertemente tipado, orientado a objetos y con buen soporte para programación asíncrona.

- **Impacto en el diseño:**
  - Permite mantener una única base tecnológica para todas las capas (dominio, aplicación, web, tests).
  - Simplifica el mantenimiento y reduce el “context switching” entre lenguajes.

---

## 2. Entorno de desarrollo y orquestación

### Visual Studio 2026 Community

- IDE principal utilizado para:
  - gestión de soluciones y proyectos,
  - depuración,
  - diseño de interfaces Blazor,
  - ejecución de pruebas unitarias e integración.

La elección de Visual Studio responde a:

- nivel de integración con .NET,
- herramientas para refactorización y navegación por código,
- soporte para las capacidades de .NET 10 y Blazor.

### Aspire

- Se utiliza **Aspire** para definir un **AppHost** (`GameTribunal.AppHost`) que declara:
  - la aplicación web,
  - la base de datos,
  - el servicio de IA.

Beneficios:

- ejecución unificada de toda la solución en desarrollo,
- configuración centralizada de recursos,
- punto de partida claro para despliegues orquestados (por ejemplo, contenedores).

---

## 3. Backend y comunicación en tiempo real

### ASP.NET Core

ASP.NET Core es el framework web base de la aplicación:

- Proporciona el pipeline HTTP para servir la aplicación Blazor.
- Gestiona la inyección de dependencias:
  - servicios de dominio y aplicación,
  - repositorios,
  - DbContext,
  - servicios de IA.

### SignalR

La comunicación en tiempo real entre servidor y clientes (pantalla principal y móviles) se realiza con **SignalR**:

- Justificación:
  - abstracción sobre WebSockets / SSE / long polling,
  - modelo basado en **Hubs** muy adecuado para salas de juego con grupos de conexiones,
  - integración directa con .NET y con Blazor Server.

- Usos en el proyecto:
  - notificar a todos los jugadores del estado de la sala,
  - propagar inicio de rondas, casos y resultados,
  - recibir acciones de los jugadores (votos, predicciones, defensas).

---

## 4. Interfaz de usuario

### Blazor

La UI se implementa con **Blazor (Server)**:

- Pantalla principal:
  - vista que representa el “tablero” del juego (casos, resultados, puntuaciones, títulos).
- Pantalla de jugador:
  - vista adaptativa para móviles/tablets,
  - interfaz de votación, defensa y confirmación.

Justificación para usar Blazor:

- Permite utilizar **C# también en el frontend**, homogeneizando el stack.
- Comunicación natural con SignalR (Blazor Server ya se basa internamente en SignalR).
- Menor complejidad conceptual que mantener un SPA en JavaScript/TypeScript separado.

---

## 5. Persistencia de datos

### Entity Framework Core y base de datos relacional

Se utiliza **Entity Framework Core** como capa de acceso a datos:

- base de datos relacional (por ejemplo, SQL Server o PostgreSQL),
- modelo de datos centrado en:
  - salas (`Rooms`),
  - jugadores (`Players`),
  - casos (`Cases`),
  - títulos (`TitleAssignments`).

Motivación:

- facilidad de mapeo entre entidades de dominio y tablas,
- soporte para migraciones,
- uso de LINQ para consultas.

La elección de un modelo relacional se debe a:

- necesidad de integridad referencial (players ↔ rooms, rounds ↔ cases),
- consultas futuras para análisis (estadísticas, histórico de títulos, etc.).

---

## 6. Control de versiones e integración continua

### Git y GitHub

- **Git** se utiliza para el control de versiones local.
- **GitHub** como repositorio remoto:
  - almacenamiento del código,
  - gestión de issues,
  - revisiones de cambios (pull requests),
  - futura integración con GitHub Actions para CI (ejecución de tests).

Ventajas:

- estándar de facto en el desarrollo colaborativo,
- facilita la trazabilidad de cambios y la revisión por parte de tutores/colaboradores.

---

## 7. IA generativa para el comentarista

El proyecto integra un **módulo opcional de IA generativa** como comentarista:

- exposición vía interfaz `ICommentaryService` en la capa de aplicación,
- implementación concreta en infraestructura (`CommentaryService`) contra el proveedor seleccionado.

Criterios de selección de tecnología de IA:

- **Facilidad de integración** con .NET (SDK o REST estable).
- **Latencia** razonable para comentarios en tiempo real o casi en tiempo real.
- **Control de estilo y seguridad**:
  - capacidad de modular el tono (humor, ironía suave),
  - filtros para lenguaje inapropiado.

Rol limitado del módulo de IA:

- solo genera texto descriptivo y humorístico sobre el estado del juego,
- no decide casos, votos ni resultados,
- no altera la lógica determinista del motor de juego.

---

## 8. Estrategia de pruebas y validación

### 8.1 Validación técnica

La validación técnica se apoya en:

- **Pruebas unitarias (.NET)** sobre:
  - `GameEngine` (cálculo de acusado, defensas, puntuación, títulos),
  - servicios de aplicación sin acceso a red,
  - utilizando frameworks como **xUnit** y ejecutadas mediante `dotnet test`.

- **Pruebas de integración** con ASP.NET Core + SignalR:
  - simulación de clientes que se conectan al Hub usando `WebApplicationFactory` / `TestServer`,
  - verificación de flujo de partida (unirse, votar, defender, puntuar).

- **Pruebas de componente y end-to-end**:
  - tests de componentes Blazor (por ejemplo, con **bUnit**) para verificar el comportamiento de la UI ante cambios de estado,
  - pruebas end-to-end con **Playwright for .NET** (opcional) para automatizar un flujo de partida básico (creación de sala, unión de varios jugadores simulados, juego de una o varias rondas).

- **Pruebas de carga y concurrencia**:
  - simulación de salas con 4, 8, 12 y 16 jugadores (por ejemplo, mediante scripts en .NET contra el Hub o herramientas externas como k6),
  - medidas de tiempo de respuesta y estabilidad,
  - uso de logging estructurado y health checks para monitorizar el comportamiento.

### 8.2 Validación de experiencia de usuario

Se complementa con:

- sesiones de juego reales con grupos de amigos,
- evaluación de:
  - facilidad de acceso (**zero-install**: QR + navegador),
  - claridad de la UI en pantalla principal y en móviles,
  - comprensión rápida de las reglas,
  - nivel de diversión e interacción social,
  - percepción del comentarista IA como capa adicional de entretenimiento.

El conjunto de validaciones permite valorar en qué medida la solución cumple su objetivo:  
ofrecer un **party game web de baja fricción y alto componente social**, apropiado para noches de amigos en casa.

---

## 9. Dispositivos objetivo, navegadores y QR

El diseño tecnológico del proyecto está orientado explícitamente a un escenario **multi-dispositivo sin instalación**:

- **Pantalla principal (host)**  
  - Navegador moderno ejecutándose en:
    - un ordenador conectado a una TV, o
    - una Smart TV con navegador integrado.
  - El contenido se puede proyectar mediante el protocolo de Google Cast, sin dependencia de hardware propietario.

- **Dispositivos de los jugadores**  
  - Smartphones y tablets con navegador moderno (Chrome, Edge, Safari, Firefox, etc.).
  - La interfaz está pensada como **web responsiva**, optimizada para pantalla vertical y entrada táctil.

Para facilitar la incorporación de jugadores, la aplicación genera un **código QR** en la pantalla principal con la URL de la sala (incluyendo el código de sala en la query).  
La lectura del QR con la cámara del dispositivo abre directamente la vista de jugador en el navegador, eliminando pasos intermedios (búsqueda de URL, login, instalación de apps).  

Este enfoque aprovecha tecnologías ya presentes en el entorno doméstico (navegador + cámara), alineándose con el objetivo de ofrecer un **party game web de baja fricción de entrada**.

---

## 10. Configuración, entornos y seguridad básica

La configuración de la aplicación se gestionará mediante los mecanismos estándar de ASP.NET Core:

- **Configuración por entorno**  
  - Ficheros `appsettings.json` y `appsettings.{Environment}.json` para:
    - cadenas de conexión de base de datos,
    - parámetros de SignalR (tiempos de keep-alive, etc.),
    - configuración del proveedor de IA generativa.
  - Uso de variables de entorno para credenciales y secretos sensibles.

- **Gestión de secretos**  
  - En desarrollo, se puede emplear el **Secret Manager** de .NET para almacenar claves de API y credenciales sin incluirlas en el control de versiones.
  - En entornos desplegados, se recomienda integrar un sistema de gestión de secretos (vault) o variables de entorno protegidas.

- **Seguridad básica**  
  - Las salas de juego se identifican mediante un código de sala aleatorio que actúa como token de acceso sencillo.
  - El modelo está orientado a partidas efímeras y pseudónimos, por lo que no se contempla autenticación pesada ni almacenamiento de datos personales reales.
  - Se aplicarán validaciones de entrada en el servidor (longitud de alias, contenido de mensajes, etc.) para evitar entradas malformadas o potencialmente problemáticas.

Aunque el proyecto no tiene requisitos de seguridad críticos (no maneja datos sensibles ni económicos), esta configuración permite una base razonable de robustez y buenas prácticas mínima.

## 11. Logging y observabilidad

Para apoyar las pruebas técnicas y el diagnóstico de errores se configurará:

- **Logging estructurado** en ASP.NET Core:
  - registro de eventos clave del juego (creación de sala, inicio de ronda, recepción de votos, cálculo de resultados),
  - almacenamiento de logs en consola durante el desarrollo y, opcionalmente, en ficheros o un sistema de agregación.

- **Health checks**:
  - endpoint de salud para comprobar la conectividad con la base de datos y el estado básico del servicio.
  - útil tanto en desarrollo como en despliegues automatizados.

Esto facilita analizar el comportamiento del sistema bajo carga y documentar métricas básicas (tiempos de respuesta, errores, etc.) en la memoria del TFG.