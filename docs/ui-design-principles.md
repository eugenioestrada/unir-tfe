# Principios de diseño de UI

Este documento define el sistema de diseño, los componentes disponibles y los patrones de interfaz que guiarán la implementación de Pandorium. Se alinea con los requisitos funcionales y no funcionales descritos en `docs/requirements.md`, las decisiones técnicas del stack (`docs/technology.md`) y el sistema visual implementado en `src/GameTribunal.Web/wwwroot/game-design.css`.

---

## 1. Filosofía y objetivos

- **Social y espectacular:** La UI debe reforzar el núcleo social del juego; prioriza el dramatismo visual, el humor y la claridad para facilitar las interacciones grupales.
- **Responsiva y 10-foot UI:** Cumple los requisitos RNF-008 y RNF-010 adaptando tamaños, densidad y jerarquía para móviles, tabletas, escritorio y pantallas compartidas (TV, proyector).
- **Accesible por defecto:** Mantiene contraste AA, estados de foco y tamaños mínimos de toque/lectura; atiende RNF-001 y RNF-004 asegurando navegabilidad con teclado/gamepad y tolerancia a latencia.
- **Determinista y coherente:** Las transiciones visuales reflejan el flujo determinista del motor de juego (RF-020 a RF-074). Las animaciones enfatizan hitos pero nunca ocultan información crítica.
- **Configuración mínima:** Usa tokens y utilidades (`game-stack`, `game-card`, `game-btn`) para acelerar la evolución sin duplicar estilos.

---

## 2. Sistema de diseño (design tokens)

### 2.1 Paleta cromática

| Token | Valor | Uso principal |
|-------|-------|---------------|
| `--color-primary` | `#FF6347` | Acciones principales, highlights críticos.
| `--color-secondary` | `#7B2CBF` | Acciones alternativas, pestañas activas.
| `--color-accent-1/2/3` | `#F72585`, `#4ECDC4`, `#FFD60A` | Refuerzos narrativos (títulos, IA, estados especiales).
| `--color-success` | `#00C9A7` | Confirmaciones, estados conectados (RF-013).
| `--color-warning` | `#FFB627` | Advertencias, límites de tiempo.
| `--color-danger` | `#FF5A5F` | Errores, desconexiones (RF-012, RF-015).
| `--color-dark` y escala `--color-gray-*` | Tonos profundos y neutros | Fondos, tarjetas y texto secundario.

Gradientes (`--gradient-primary`, `--gradient-secondary`, `--gradient-dark`) añaden profundidad en botones, cards clave y fondos hero.

### 2.2 Tipografía

- **Familia base:** `Inter` para texto funcional (`body`, controles, tablas).
- **Familia display:** `Poppins` para titulares, impactos y llamadas a la acción.
- **Escala:** Tokens `--font-size-xs` a `--font-size-7xl`; usar `--font-size-5xl` y superiores en titulares de host (10-foot UI) y `--font-size-lg` máximo en móviles para evitar saturación.
- **Peso:** `--font-weight-extrabold` reserva para títulos y puntuaciones; `--font-weight-medium` para texto operativo.

### 2.3 Espaciado, radios y sombras

- Espaciado incremental `--space-xs`…`--space-4xl` para definir ritmo vertical/horizontal sin valores ad-hoc.
- Bordes redondeados (`--radius-sm`…`--radius-2xl`) refuerzan el estilo futurista. `--radius-2xl` se reserva para contenedores principales (hero, modales).
- Sombras (`--shadow-sm`…`--shadow-2xl`) controlan la jerarquía; `--shadow-glow` enfatiza elementos interactivos destacados (botones inicio, código de sala).

### 2.4 Breakpoints y capas

- Breakpoints definidos en CSS: móvil ≤640 px, tablet 641–1024 px, desktop 1025–1399 px, TV ≥1400 px, TV XL ≥1920 px.
- Ajustar tipografía y densidad mediante las reglas existentes (`@media`) para cumplir RNF-010: sin scroll en host, scroll mínimo en player.
- Escala Z-index (`--z-*`) para modales, popovers, toasts; evita valores mágicos.

### 2.5 Transiciones y animaciones

- Duraciones (`--transition-fast`, `--transition-base`, `--transition-slow`) gobiernan hovers, fades y microinteracciones.
- Animaciones predefinidas (`game-animate-fadeIn`, `game-animate-slideIn`, `game-animate-pulse`) refuerzan feedback sin afectar la legibilidad.
- Efectos avanzados (glow, particles, shimmer) limitados a host y momentos celebratorios para no saturar la vista player.

---

## 3. Componentes disponibles

### 3.1 Primitivas de layout

| Clase | Descripción | Uso recomendado |
|-------|-------------|-----------------|
| `.game-container` | Contenedor principal centrado, padding adaptable. | Vista host (pantalla completa) y secciones de player.
| `.game-content-container-*` | Contenedores con anchuras máximas escalables. | Formularios y paneles dentro de player.
| `.game-stack` + modificadores | Flex vertical/horizontal con espaciado consistente. | Agrupar tarjetas, botones o listas responsivas.
| `.game-grid` / `.game-grid-2` | Grillas responsivas auto-fit. | Distribuir tarjetas de jugadores o paneles informativos.
| `.game-spacer` | Elemento flexible para distribuir huecos. | Ajustar alineaciones en host sin hacks.

### 3.2 Contenedores y tarjetas

- `.game-hero`: bloque hero translucido con fondo animado; introduce sala y casos en host.
- `.game-card` (+ variantes `-elevated`, `-interactive`, `-spotlight`): panel base para secciones (ranking, detalle de caso, lista de jugadores).
- `.game-card-header/body/footer`: estructuran contenido interno con separadores consistentes.

### 3.3 Tipos de botón

- `.game-btn` base, con variantes `-primary`, `-secondary`, `-outline`, `-lg`, `-block`, `is-loading`.
- Uso:
  - Acciones críticas (crear sala, iniciar ronda, enviar voto) → `game-btn game-btn-primary`.
  - Acciones secundarias (compartir enlace, volver al lobby) → `game-btn game-btn-secondary`.
  - Cancelaciones o navegación no destructiva → `game-btn game-btn-outline`.

### 3.4 Formularios y controles

- `.game-form-group`, `.game-label`, `.game-input`, `.game-select` cubren campos de alias, selección de modo, opciones de defensa.
- `.game-chip`, `.game-badge` representan estados (host: conectado/inactivo; player: roles especiales).
- `.game-progress-*` ilustra progreso de fases o rondas completadas.

### 3.5 Feedback e información

- `.game-alert-*` para mensajes persistentes (fallback IA, límites de tiempo).
- `.game-toast-*` para notificaciones efímeras (reconexión, voto registrado).
- `.game-spinner`, `.game-loading-container` para estados de carga (RF-011, RF-016).
- `.game-skeleton-*` para placeholders durante sincronización inicial (RNF-004).

### 3.6 Navegación y modales

- `.game-tabs`/`.game-tab` permiten alternar entre paneles (ej. listado de jugadores vs títulos).
- `.game-modal-*` gestiona diálogos (confirmar fin anticipado RF-032, ver reglas).
- `.game-qr-container`, `.game-room-code`, `.game-qr-image` soportan RF-004: QR y códigos visibles.

---

## 4. Vistas y patrones de interacción

Se definen tres contextos principales: **Host (Lobby)**, **Host (Partida)** y **Player**. Todos deben mantener coherencia visual y responder en menos de 500 ms para acciones críticas (RNF-002).

### 4.1 Host (Lobby compartido)

- **Objetivos:** presentar sala, invitar jugadores (RF-001 a RF-007).
- **Layout:**
  - Hero central (`.game-hero`) con título (`.game-title`), subtítulo y CTA `Iniciar sala`.
  - Panel QR (`.game-qr-container`) con código, enlace de compartir y badges de modo (`.game-badge`).
  - Lista de jugadores (`.game-player-list`, `.game-player-item`, `.game-player-status`).
- **Patrones:**
  - Botones prominentes (`game-btn-lg`) para iniciar partida y compartir (Web Share API).
  - Alertas informan requisitos (mínimo 4 jugadores RF-003) o alias duplicado.
  - Animaciones suaves (`game-animate-fadeIn`) al sumarse jugadores.

### 4.2 Host (Partida en curso)

- **Objetivos:** sincronizar información de fase (RF-020) y resultados (RF-040 a RF-043, RF-050 a RF-052).
- **Secciones clave:**
  - Panel del caso actual (`game-card` con `game-card-header` y texto principal en `--font-size-3xl` o superior).
  - Barra de progreso (`.game-progress-bar`, `.game-progress-steps`) indicando fase actual.
  - Mosaico de jugadores con contadores de acusaciones/predicciones (utilizar `.game-grid-2`).
  - Área de comentario IA (`.game-card game-card-elevated`); debe permitir fallback textual claro cuando el servicio no esté disponible.
- **Transiciones:**
  - Cambios de fase disparan `PhaseChanged` → aplicar `game-animate-slideIn` para paneles nuevos.
  - Al finalizar ronda, usar `.game-animate-bounceIn` o `.game-levitate` en el título social asignado.

### 4.3 Player (Dispositivo personal)

Componentes adaptativos para móvil vertical; deben priorizar legibilidad y control táctil.

1. **Incorporación:**
   - Formulario con `.game-input` para alias, `game-btn-primary` para unirse (RF-010, RF-006).
   - Mensajes de error via `.game-alert-danger`.
2. **CaseVoting:**
   - Lista de jugadores renderizada como tarjetas seleccionables (`.game-card-interactive`) con badges de estado.
   - Selección múltiple (máx. dos) visualizada con `.game-chip` y contadores.
   - Predicción en selector (`.game-select`). Desactivar botón enviar hasta cumplir reglas.
3. **Defensa:**
   - Si jugador es acusado inicial, mostrar modal (`.game-modal`) con opciones (`game-btn-outline`).
   - Para resto, mostrar estado bloqueado con `.game-alert-info`.
4. **DefenseVoting:**
   - Botones binarios grandes (`game-btn game-btn-lg`) accesibles con pulgar.
   - Temporizador opcional usando `.game-progress-fill` animado.
5. **Resultados:**
   - Mostrar puntos ganados (`.game-badge-success`), títulos obtenidos (`.game-badge-secondary`).
   - Incluir comentario IA resumido; si no hay IA, usar mensaje fallback según RF-052.

### 4.4 Patrones transversales

- **Reconexión (RF-012, RF-017):**
  - Mostrar `game-toast-info` al recuperar estado.
  - Aplicar `.game-skeleton` mientras se restaura la vista.
- **Errores y validaciones:**
  - Mensajes claros en `.game-alert-danger` y botón retriable.
  - Mantener acciones críticas deshabilitadas hasta cumplir reglas (preventivo a RF-071, RF-072).
- **Final de partida:**
  - Vista host tipo scoreboard (ranking en tabla `.table`, títulos en chips).
  - Player muestra resumen personal y CTA para volver al lobby.

---

## 5. Accesibilidad y usabilidad

- **Contraste:** garantizar relación ≥4.5:1 usando tokens establecidos; evitar aplicar opacidades que comprometan legibilidad.
- **Focus visible:** aprovechar reglas `:focus-visible` existentes (`outline` + `box-shadow`). No sustituir por estilos invisibles.
- **Tamaño táctil mínimo:** botones ≥48 px en móviles (ya cubierto por `.game-btn`), controles de radio/checkbox personalizados deben respetar.
- **Lectura en voz:** textos dinámicos (comentario IA, cambios de fase) deben exponer atributos ARIA (`aria-live="polite"`).
- **Anunciadores:** proporcionar `skip link` (`.game-skip-link`) en host para accesibilidad con teclado.

---

## 6. Animaciones y estados

- Limitarlas en player durante fases críticas para no distraer.
- Usar `prefers-reduced-motion` (extensión futura) para desactivar animaciones intensas en usuarios sensibles.
- Mantener transiciones <500 ms salvo celebraciones (confeti, neon) posteriores a resultados.
- Estandarizar estados `hover`, `active`, `disabled` con el set de clases actual para coherencia.

---

## 7. Contenido y tono

- **Textos host:** tono épico y humorístico breve; titular en mayúsculas moderadas, subtítulo instructivo.
- **Textos player:** directos, empáticos, usando acciones concretas ("Selecciona hasta dos sospechosos").
- **Comentarios IA:** resaltan el caso, el acusado final y la reacción del grupo; proveer fallback de humor en local sin IA (RF-052).
- **Microcopys:** mantener consistencia terminológica con requisitos (fases `Lobby`, `CaseVoting`, `Defense`, `DefenseVoting`, `Scoring`, `Finished`).

---

## 8. Lineamientos de implementación

- Reutilizar tokens y clases existentes antes de crear nuevos estilos; en caso de nueva necesidad, documentar token adicional en esta guía y en `game-design.css`.
- Mantener separación host/player mediante componentes Blazor específicos, pero con estilos comunes.
- Documentar cada nuevo componente visual en este archivo y referenciar pruebas UI Playwright asociadas en `docs/testing.md`.
- Validar visualmente en los dispositivos objetivo (móvil vertical, tablet horizontal, desktop 1080p, TV 4K) antes de cerrar entregables.

---

**Última actualización:** 2025-11-18
