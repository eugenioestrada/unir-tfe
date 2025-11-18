# Principios de diseño de UI

Este documento describe el lenguaje visual definitivo de Pandorium y establece los patrones obligatorios para toda implementación de interfaz. Su alcance cubre host, jugadores y vistas auxiliares, garantizando coherencia con los requisitos funcionales y no funcionales de `docs/requirements.md`, la lógica de `docs/game-logic.md` y la arquitectura descrita en `docs/architecture.md`. Cada decisión aquí recogida debe reflejarse en componentes Blazor, pruebas Playwright y estilos CSS de `src/GameTribunal.Web/wwwroot/game-design.css`.

---

## 1. Principios rectores

- **Espectáculo legible:** El juego es una fiesta visual sin sacrificar claridad. La jerarquía tipográfica y la iluminación focal dirigen la atención a las decisiones deterministas del motor (RF-020 a RF-074).
- **Escenografía social compartida:** Host y jugadores viven la misma narrativa con perspectivas adaptadas. Todo elemento clave se sincroniza en tiempo real (RF-011, RF-017) y comunica progreso y estados en menos de 500 ms (RNF-002).
- **Consistencia total:** Cada componente reutiliza tokens e interacciones documentadas. Cualquier nueva funcionalidad debe declarar su correspondencia con estos principios (RNF-011).
- **Accesibilidad celebrada:** Las animaciones, colores y microcopys respetan WCAG 2.2 AA, navegación con teclado y usuarios sensibles a movimiento (RNF-001, RNF-004).
- **Determinismo visual:** Los cambios de fase y resultados expresan el orden del motor con patrones de transición previsibles; no se esconde información bajo efectos ni se añade aleatoriedad visual.

---

## 2. Lenguaje visual integral

### 2.1 Paleta narrativa

| Token | Valor | Significado narrativo | Referencia de uso |
|-------|-------|-----------------------|-------------------|
| `--color-primary` | `#FF5533` | Foco de acción, CTA esenciales | Botones de inicio, envío de acusaciones (RF-020, RF-022)
| `--color-secondary` | `#6A29FF` | Ritmo dramático alterno | Tabs, conmutadores de vista host
| `--color-hero` | `linear-gradient(135deg, #FF5533 0%, #6A29FF 100%)` | Escenarios hero y overlays | Vista host en TV y modales épicos
| `--color-surface` | `#0E0E17` | Fondo base | Layout host/player
| `--color-elevation-1` | `#16162A` | Contenedores elevados | Cards, paneles de fase
| `--color-success` | `#00C9A7` | Estados afirmativos | Reconexiones, defensas aceptadas (RF-028)
| `--color-warning` | `#FFB84D` | Conteos regresivos | Temporizadores de votación
| `--color-danger` | `#FF4D6D` | Alertas y errores | Alias duplicado, desconexión (RF-006, RF-012)
| `--color-info` | `#4EA8DE` | Mensajería contextual | Comentario IA, hints

Los degradados hero y los halos (`--shadow-glow`, `--shadow-neon`) se reservan para feedback épico: inicio de ronda, acusación final y scoreboard. Se prohíbe mezclar más de dos gradientes simultáneos por vista para preservar legibilidad.

### 2.2 Tipografía y ritmo

- **Familia primaria:** `Inter` con codificación completa para legibilidad en distancias largas.
- **Familia display:** `Space Grotesk` para titulares y numerales dramáticos; fallback a `Poppins`.
- **Escala modular:** Base `--font-size-md` = 18 px. Reglas:
  - Host TV ≥1400 px: titulares `--font-size-6xl`, subtítulos `--font-size-3xl`, texto operativo `--font-size-xl`.
  - Player móvil ≤640 px: titulares `--font-size-2xl`, cuerpo `--font-size-base`, botones `--font-size-lg`.
- **Kerning y tracking:** Ajustar `letter-spacing` positivo (+0.05em) en títulos de scoreboard para efecto retro futurista sin perder legibilidad.
- **Ritmo vertical:** Espaciado base `--space-base` = 16 px; todos los stacks se definen como múltiplos (`0.5x, 1x, 2x, 4x`). Ningún margen ad-hoc.

### 2.3 Iconografía y ornamentos

- Iconos lineales neon (`game-icon-*`) con grosor 2 px y esquinas suaves. Deben alinearse a la cuadrícula de 24 px.
- Ornamentación holográfica (`game-ornament`) se aplica en host únicamente, con animación slow pulse (<0.3 opacidad) para evitar distracción en player.
- Introducir `game-glyph-phase-*` para representar cada fase con símbolos únicos reaprovechables en paneles, toasts y scoreboard.

### 2.4 Motion system

- Utilizar la escala `--transition-snap (120ms)`, `--transition-flow (240ms)`, `--transition-ritual (440ms)`.
- Transiciones de fase: panel saliente fade-down + panel entrante slide-up con retardo de 80 ms para enfatizar determinismo.
- Interacciones críticas (CTA, votación) deben confirmar con microfeedback `game-animate-press` (scale 0.96 → 1.02 → 1) durante 180 ms.
- Respetar `prefers-reduced-motion`; en ese modo, sustituir animaciones por cambios instantáneos con sombra intensificada.

---

## 3. Arquitectura de vistas

### 3.1 Host Lobby (RF-001 a RF-007)

- **Layout en capas:**
  - `game-hero` ocupa 60% superior con degradado hero, título impactante y resumen del modo.
  - Zona inferior split en dos cards: izquierda `game-card game-card-spotlight` (QR + código + compartir) y derecha `game-card game-card-roster` (lista de jugadores).
- **Narrativa temporal:** Ilustrar progreso de preparación con `game-timeline` (puntos: Crear sala, Jugadores listos, Iniciar). Cada punto cambia de color al cumplirse.
- **Indicadores de estado:** Los avatares muestran halos y barras de latencia. Utilizar chips `game-chip-status` para `Conectado`, `Inactivo`, `Desconectado`.
- **Reglas de CTA:** `Iniciar partida` se habilita cuando contador de jugadores ≥4; mostrar tooltip persistente con reglas RF-003.

### 3.2 Host Partida (RF-020 a RF-052)

- **Estructura tri-panel:**
  - Panel izquierdo `game-panel-case`: caso actual con ilustración vectorial, nivel de picante y texto principal.
  - Panel central `game-panel-flow`: fase activa con barra de progreso vertical, tarjetas de acusación, defensa o puntuaciones según fase.
  - Panel derecho `game-panel-commentary`: comentario IA, log de eventos y fallback humorístico.
- **HUD superior:** Banda translúcida muestra ronda actual, temporizador y estado del service AI.
- **HUD inferior:** Carrusel de jugadores con badges para acusaciones recibidas, votos emitidos y títulos. Se mueve automáticamente pero permite exploración manual con foco accesible.
- **Transiciones de fase:** Animar panel central con `slide-axis` (CaseVoting ←→ Defense ←→ DefenseVoting). Mantener overlay de countdown sincronizado con SignalR.

### 3.3 Host Scoring & Finished (RF-040 a RF-074)

- **Scoreboard cinemático:** Presentar ranking en `game-table-score` con columnas: Jugador, Puntos ronda, Puntos totales, Títulos.
- **Módulo de highlights:** Cards destacadas para MVP de acusaciones, defensa heroica y predicción perfecta.
- **Clausura:** Botón `Compartir resumen` y `Nueva partida` lado a lado; animación de confeti vectorial sobria.

### 3.4 Player Flow

- **Barra contextual fija:** Cabezal con nombre del jugador, avatar dinámico y fase actual; color de fondo cambia según fase para orientación rápida.
- **Wizard determinista:** Cada fase se expresa como pantalla dedica con CTA primaria en footer `game-footer-cta` para asegurar ergonomía en móviles.
- **Feedback háptico virtual:** Añadir clases `game-btn-haptic` para simular vibración visual (resplandor pulsante) cuando se requiere acción inmediata.
- **Defensa y votación:** Modales a pantalla completa con narrativa breve, opciones en formato segmentado y accesos directos para confirmación.
- **Resumen personal:** Al finalizar ronda, mostrar card de puntos, decisiones acertadas y comentario personalizado.

### 3.5 Patrones compartidos

- `game-toast-stack` arriba a la derecha en host y arriba centrado en player; auto-dismiss 4 s salvo errores críticos.
- Skeletons `game-skeleton-grid` replican proporciones exactas de cards para evitar saltos de layout.
- Always-on `game-connection-indicator` (esfera verde/ámbar/roja) sincronizada con RF-013 y RNF-004.

---

## 4. Biblioteca de componentes obligatorios

| Componente | Descripción | Estados obligatorios | Pruebas UI requeridas |
|------------|-------------|----------------------|-----------------------|
| `game-panel-case` | Presenta caso activo con imagen y texto | Normal, Skeleton, Sin IA | `CaseFlow.Host.Tests` (nuevo)
| `game-card-roster` | Lista de jugadores con status y badges | Vacío, Lleno, Reordenado | `Lobby.Roster.Tests`
| `game-footer-cta` | Barra fija con CTA primaria/secundaria | Enabled, Disabled, Loading | `Player.CTA.Tests`
| `game-timeline` | Línea de preparación lobby | Paso pendiente/completo/bloqueado | `Lobby.Timeline.Tests`
| `game-table-score` | Ranking final y por ronda | Orden asc/desc, empate | `Scoring.Scoreboard.Tests`

Cada componente debe exponerse como partial Blazor y contar con historia en Storybook interno (pendiente) o documentación equivalente. Al introducir variantes, actualizar esta tabla y el mapeo en `docs/testing.md`.

---

## 5. Sistema de interacción

- **Estados:** Todos los controles tienen estados `default`, `hover`, `pressed`, `focus-visible`, `disabled`. Reutilizar tokens `--state-*` para colores.
- **Validaciones:** Mostrar inline dentro del componente (barra inferior de color + mensaje) y duplicar en toast solo si afecta a progresión crítica.
- **Temporizadores:** Emplear `game-timer-ring` (SVG animado) sincronizado con SignalR y fallback textual cuando motion desactivado.
- **Audio opcional:** Documentar cues sonoros sutiles (no implementados aún) que acompañan fases clave; deben poder desactivarse desde configuraciones.
- **Microcopys:** Mantener tono entre épico y cómplice. Evitar jerga que no aparezca en requisitos. Ejemplos concretos incluidos en Storybook.

---

## 6. Accesibilidad, rendimiento y confiabilidad

- **Contraste:** Auditorías periódicas con Lighthouse ≥90. Tokens de color incluyen variantes de alto contraste (`*-hc`).
- **Focus management:** Al cambiar de fase, mover foco al primer elemento interactivo relevante sin perder contexto.
- **Lectores de pantalla:** `aria-live="polite"` para resumen de fase, `aria-live="assertive"` para temporizadores críticos (<10 s).
- **Performance:** Limitar texturas y filtros CSS costosos; máximo de 120 kb de assets por vista sin contar tipografías. Cachear imágenes de casos.
- **Resiliencia:** Cargar fallback textual instantáneo si IA falla, evitar placeholders vacíos (RF-052, RF-074). Las reconexiones muestran skeletons + toast confirmación.

---

## 7. Gobernanza y trazabilidad

- Toda nueva funcionalidad debe vincularse a tokens y componentes existentes; si se crean nuevos, documentarlos aquí y en `game-design.css` antes del merge.
- Actualizar `docs/testing.md` con la matriz requisito ↔ prueba UI ↔ componente cuando se introduzcan o modifiquen interacciones.
- Reforzar en revisiones de código la comprobación de estos principios (ver RNF-011 en `docs/requirements.md`).
- Los prototipos aprobados (Figma o equivalente) se archivan junto a este documento en formato PDF para consulta.

---

**Última actualización:** 2025-11-18
