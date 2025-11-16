# Planificación del Proyecto

Este documento recoge la planificación temporal, hitos principales, riesgos y estrategia de pruebas.

---

## Roadmap y Hitos

| Hito                | Fecha estimada | Estado |
|---------------------|----------------|--------|
| Modelo de dominio   | 2025-11-30     | Pendiente |
| Persistencia inicial| 2025-12-07     | Pendiente |
| SignalR y contratos | 2025-12-14     | Pendiente |
| Interfaz Blazor     | 2025-12-21     | Pendiente |
| IA comentarista     | 2026-01-10     | Pendiente |
| Pruebas técnicas    | 2026-01-17     | Pendiente |
| Validación usuarios | 2026-01-24     | Pendiente |

---

## Análisis de Riesgos

- Riesgo: Latencia excesiva en tiempo real
  - Mitigación: Pruebas de carga y optimización SignalR
- Riesgo: Dificultad integración IA
  - Mitigación: Prototipo temprano y fallback a mensajes estáticos
- Riesgo: Problemas de compatibilidad en navegadores
  - Mitigación: Test en dispositivos y navegadores variados

---

## Estrategia de Pruebas

- Pruebas unitarias sobre motor de juego y servicios
- Pruebas de integración con SignalR y Blazor
- Pruebas de carga (simulación de 4-16 jugadores)
- Validación con usuarios reales

---

(Completar y actualizar según evolución del proyecto)
