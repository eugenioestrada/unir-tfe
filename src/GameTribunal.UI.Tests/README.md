# GameTribunal UI Tests

Este proyecto contiene pruebas automatizadas de interfaz de usuario utilizando Playwright para .NET.

## Objetivo

Validar que la interfaz de usuario del juego Pandorium funciona correctamente en múltiples dispositivos y cumple con estándares de accesibilidad.

## Pruebas Incluidas

### DesignSystemTests (10 tests)
Valida la consistencia del sistema de diseño:
- Hero section con efectos visuales
- Botones con efectos hover
- Cards con estilo profesional
- Paleta de colores
- Tipografía con jerarquía
- Animaciones
- Efectos modernos (glassmorphism)
- Spacing consistente
- Estados de foco
- Display de QR code

### ResponsiveDesignTests (10 tests)
Valida el diseño responsivo:
- Mobile Portrait (375x667)
- Mobile Landscape (667x375)
- Tablet (768x1024)
- Desktop (1920x1080)
- TV/10-foot UI (1920x1080+)
- Sin overflow horizontal
- Imágenes responsivas
- Touch targets mínimos (44x44px)
- Grid layout adaptativo

### AccessibilityTests (11 tests)
Valida accesibilidad WCAG AA:
- Contraste de colores
- Navegación por teclado
- HTML semántico
- Alt text en imágenes
- Títulos de página
- Labels en formularios
- Elementos deshabilitados claros
- Soporte para prefers-reduced-motion
- Redimensionamiento de texto
- Mensajes de error accesibles

### VisualRegressionTests (11 tests)
Captura screenshots para validación visual:
- Lobby (desktop/mobile/tablet)
- Room con QR code (desktop/mobile)
- Hero section
- Game cards
- Botones
- Home page

## Ejecutar las Pruebas

### Prerequisitos
```bash
# Instalar navegadores Playwright
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

### Ejecutar todas las pruebas
```bash
dotnet test
```

### Ejecutar pruebas específicas
```bash
# Solo pruebas de diseño
dotnet test --filter "FullyQualifiedName~DesignSystemTests"

# Solo pruebas de responsividad
dotnet test --filter "FullyQualifiedName~ResponsiveDesignTests"

# Solo pruebas de accesibilidad
dotnet test --filter "FullyQualifiedName~AccessibilityTests"

# Solo capturas de pantalla
dotnet test --filter "FullyQualifiedName~VisualRegressionTests"
```

### Ver Screenshots
Los screenshots se generan en:
```
bin/Debug/net10.0/screenshots/
```

## Tecnologías

- **Playwright for .NET**: Framework de testing E2E
- **NUnit**: Framework de testing
- **.NET 10**: Runtime
- **Chromium**: Navegador para testing

