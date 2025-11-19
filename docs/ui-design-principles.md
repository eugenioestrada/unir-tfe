# Principios de diseño de UI

Este documento describe el lenguaje visual definitivo de Pandorium y establece los patrones obligatorios para toda implementación de interfaz. Su alcance cubre host, jugadores y vistas auxiliares, garantizando coherencia con los requisitos funcionales y no funcionales de `docs/requirements.md`, la lógica de `docs/game-logic.md` y la arquitectura descrita en `docs/architecture.md`. Cada decisión aquí recogida debe reflejarse en componentes Blazor, pruebas Playwright y estilos CSS de `src/GameTribunal.Web/wwwroot/game-design.css`.

---

## 1. Principios rectores

- **Espectáculo legible:** El juego es una fiesta visual sin sacrificar claridad. La jerarquía tipográfica y la iluminación focal dirigen la atención a las decisiones deterministas del motor (RF-020 a RF-074).
- **Escenografía social compartida:** Host y jugadores viven la misma narrativa con perspectivas adaptadas. Todo elemento clave se sincroniza en tiempo real (RF-011, RF-017) y comunica progreso y estados en menos de 500 ms (RNF-002).
- **Consistencia total:** Cada componente reutiliza tokens e interacciones documentadas. Cualquier nueva funcionalidad debe declarar su correspondencia con estos principios (RNF-011).
- **Accesibilidad celebrada:** Las animaciones, colores y microcopys respetan WCAG 2.2 AA, navegación con teclado y usuarios sensibles a movimiento (RNF-001, RNF-004).
- **Determinismo visual:** Los cambios de fase y resultados expresan el orden del motor con patrones de transición previsibles; no se esconde información bajo efectos ni se añade aleatoriedad visual.
- **Visibilidad continua:** Cada vista debe mostrar la totalidad de elementos funcionales sin desplazamientos ni capas modales que oculten información, ajustando escala y jerarquía para cumplir RNF-010 en cualquier resolución.

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

### 2.5 Breakpoints obligatorios

| Alias | Ancho mínimo | Orientación | Escenario objetivo |
|-------|--------------|-------------|--------------------|
| `xs`  | 320 px       | Portrait    | Móvil ultra compacto |
| `sm`  | 375 px       | Portrait    | Móvil compacto |
| `md`  | 414 px       | Portrait    | Móvil amplio |
| `lg`  | 768 px       | Portrait    | Tablet vertical |
| `xl`  | 1024 px      | Landscape   | Tablet horizontal / Laptop pequeño |
| `2xl` | 1280 px      | Landscape   | Laptop estándar |
| `3xl` | 1440 px      | Landscape   | Laptop grande / Monitor 2K reducido |
| `4xl` | 1920 px      | Landscape   | Desktop 1080p (TV 1080p incluido) |
| `5xl` | 2560 px      | Landscape   | Monitor 2.5K / TV 4K reescalado |
| `6xl` | 3840 px      | Landscape   | TV 4K nativo |

Los componentes deben mantener visibilidad completa y tipografía legible en cada breakpoint, usando escalado progresivo sin introducir barras de desplazamiento horizontales.

---

## 3. Arquitectura de vistas

### 3.1 Host Lobby (RF-001 a RF-007)

- **Layout sin desplazamiento:** El escenario se compone de una retícula fija 2x2 dentro de un contenedor 16:9 escalado (letterboxing/pillarboxing según corresponda). Todos los módulos ajustan tipografía y padding mediante tokens `--scale-stage-*` para permanecer visibles sin scroll.
- **Zona hero:** `game-hero game-hero-lobby` ocupa la celda superior izquierda, mantiene proporción 3:2 y restringe su altura a `clamp(18rem, 32vh, 26rem)` en viewports ≥1024 px para liberar el resto de la retícula; nunca supera el 40% de la altura visible.
- **Información crítica:** QR, código y timeline residen en la mitad inferior distribuida en subpaneles que compactan listas con densidad `compact` cuando hay más de 8 jugadores, evitando barras de desplazamiento.
- **Indicadores de estado:** Los avatares usan un mosaico `game-grid-roster` que prioriza ajuste de columnas (máx. 4 columnas) y reduce avatar a 72 px manteniendo etiquetas visibles.
- **CTA en contexto:** `Iniciar partida` permanece en una barra fija `game-footer-host` dentro del mismo viewport; los tooltips persistentes se integran como subtítulos para evitar popups flotantes.

### 3.2 Host Partida (RF-020 a RF-052)

- **Escenario matricial:** Sustituye el tri-panel apilado por una retícula 12 columnas que se agrupa en tres paneles simultáneos (`case`, `flow`, `commentary`) con anchos 4-4-4 en ≥1600 px y 5-4-3 en ≤1366 px, manteniendo siempre los tres módulos completos visibles.
- **Gestión de densidad:** La lista de acusaciones/defensas se presenta en tarjetas condensadas con altura fija y scroll interno deshabilitado; cuando superan la capacidad, se activa compresión tipográfica y paginación automática visible (píldoras enumeradas) que muestra todas las entradas simultáneamente en miniatura.
- **HUDs integrados:** Banda superior e indicadores de fase se integran en el margen superior de la retícula con altura máxima de 96 px; ningún HUD flota fuera del canvas ni provoca solapamientos.
- **Panel de jugadores:** Reemplaza el carrusel por `game-grid-players` (matriz 3xN) con indicadores miniatura. En resoluciones menores, el grid reduce a 2 filas mediante escala uniforme, nunca oculta elementos detrás de controles de navegación.

### 3.3 Host Scoring & Finished (RF-040 a RF-074)

- **Scoreboard compacto:** `game-table-score` adopta tipografía condensa y columnas de ancho fijo. Si la lista supera 12 jugadores, se habilita modo doble columna dentro del mismo lienzo, replicando encabezados para evitar scroll.
- **Highlights visibles:** Los módulos de MVP y defensas heroicas se alinean en la misma retícula, con tarjetas de 320 px de alto máximo y texto autoajustado para caber en la vista final.
- **Acciones finales:** Botones `Compartir resumen` y `Nueva partida` se integran en una barra inferior fija dentro del canvas principal sin superponer el ranking.

### 3.4 Player Flow

- **Layout fijo:** Cada fase se representa en panel central `game-stage-player` con altura máxima igual al viewport. El contenido se divide en bloques colapsables automáticos (`accordion-inline`) que permanecen expandidos por defecto para mostrar toda la información sin desplazamiento.
- **CTA sin modales:** Las acciones se alojan en la barra `game-footer-cta` anclada al mismo lienzo. Se eliminan modales a pantalla completa; en su lugar, se utilizan paneles laterales plegables que conviven con el resto del contenido.
- **Visibilidad de decisiones:** Opciones de votación y defensa se muestran en matriz 2 columnas que degrada a 1 columna compactando tipografía antes de provocar scroll; se asegura que el botón de confirmación permanezca simultáneamente visible.

### 3.5 Patrones compartidos

- `game-toast-stack` se limita a mensajes inline dentro del canvas; su altura máxima garantiza que no oculte controles permanentes ni genere desplazamiento.
- `game-connection-indicator` y skeletons se integran en slots reservados de la retícula para evitar saltos de layout y mantener la visibilidad constante de todos los elementos.

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
