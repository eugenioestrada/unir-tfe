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
```

## Documentación Completa

Para información detallada sobre la suite de testing (Unit Tests y UI Tests), consulta:

📚 **[docs/testing.md](../../docs/testing.md)**

Incluye:
- Estrategia de testing completa
- Cobertura de tests unitarios y de UI
- Comandos avanzados de ejecución
- Best practices y convenciones
- Métricas y roadmap
