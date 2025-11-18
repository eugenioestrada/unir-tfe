# GameTribunal UI Tests

Pruebas automatizadas de interfaz de usuario con Playwright para .NET.

## Objetivo

Validar que la interfaz del juego funciona correctamente en múltiples dispositivos y cumple con estándares de accesibilidad.

## Ejecución Rápida

```bash
# Instalar navegadores Playwright (primera vez)
pwsh bin/Debug/net10.0/playwright.ps1 install chromium

# Ejecutar todas las pruebas
dotnet test

# Ejecutar categoría específica
dotnet test --filter "FullyQualifiedName~AccessibilityTests"

# Ejecutar pruebas de RNF-010 (Fullscreen Layout)
dotnet test --filter "FullyQualifiedName~FullscreenLayoutTests"
```

## Nuevas Pruebas: RNF-010

Se han añadido 23 pruebas específicas para validar el **RNF-010: Adaptar layout a pantalla completa**.

### Categorías de Pruebas RNF-010

- **No Horizontal Scrolling**: 8 tests en diferentes viewports (320x568 a 3840x2160)
- **No Vertical Scrolling**: 4 tests en dispositivos estándar
- **Dynamic Viewport Filling**: 3 tests de adaptación dinámica
- **Responsive Adaptation**: Tests de cambios de viewport
- **Mobile Browser Chrome**: Tests de adaptación a chrome del navegador móvil
- **Accessibility**: Tests de accesibilidad sin scroll

Ver documentación completa en: [docs/rnf-010-implementation.md](../../docs/rnf-010-implementation.md)

## Documentación Completa

Para información detallada sobre la suite de testing (Unit Tests y UI Tests), consulta:

📚 **[docs/testing.md](../../docs/testing.md)**

Incluye:
- Estrategia de testing completa
- Cobertura de tests unitarios y de UI
- Comandos avanzados de ejecución
- Best practices y convenciones
- Métricas y roadmap
