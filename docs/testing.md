# Testing en Pandorium

Este documento describe la estrategia de testing del proyecto Pandorium, incluyendo pruebas unitarias y pruebas de interfaz de usuario.

## Visión General

El proyecto utiliza un enfoque de testing en múltiples niveles:

- **Unit Tests**: Prueban la lógica de negocio y servicios de forma aislada
- **UI Tests**: Validan la interfaz de usuario, accesibilidad y diseño responsivo

## Unit Tests

### Tecnologías

- **Framework**: xUnit
- **Mocking**: Moq
- **Coverage**: coverlet
- **.NET**: 10.0

### Estructura

Los tests unitarios se encuentran en `src/GameTribunal.Application.Tests/` y están organizados por capa:

```
GameTribunal.Application.Tests/
├── Domain/          # Tests de entidades y value objects del dominio
│   ├── RoomTests.cs
│   └── PlayerTests.cs
├── Services/        # Tests de servicios de aplicación
│   └── RoomServiceTests.cs
└── SignalR/        # Tests de integración con SignalR
    └── SignalRIntegrationTests.cs
```

### Cobertura de Tests

#### Domain Tests
- **RoomTests**: Valida la creación y gestión de salas
  - Creación de salas con parámetros válidos
  - Validación de parámetros requeridos
  - Añadir/eliminar jugadores
  - Validación de límites (min/max jugadores)
  - Estado de la sala (puede iniciar juego)

- **PlayerTests**: Valida la entidad jugador
  - Creación de jugadores
  - Validación de alias
  - Identificadores únicos

#### Service Tests
- **RoomServiceTests**: Valida la lógica de negocio de salas
  - Creación de salas con código único
  - Persistencia en repositorio
  - Gestión de modos de juego
  - Validación de reglas de negocio

#### SignalR Tests
- **SignalRIntegrationTests**: Valida la comunicación en tiempo real
  - Conexión de clientes
  - Envío y recepción de mensajes
  - Gestión de grupos (salas)
  - Sincronización de estado

### Ejecutar Unit Tests

```bash
# Desde la raíz del proyecto
dotnet test src/GameTribunal.Application.Tests/

# Con cobertura
dotnet test src/GameTribunal.Application.Tests/ --collect:"XPlat Code Coverage"

# Con verbosidad detallada
dotnet test src/GameTribunal.Application.Tests/ --logger "console;verbosity=detailed"

# Filtrar tests específicos
dotnet test src/GameTribunal.Application.Tests/ --filter "FullyQualifiedName~RoomTests"
```

### Convenciones

- Nomenclatura: `MethodName_Scenario_ExpectedBehavior`
- Ejemplo: `CreateRoom_ValidParameters_CreatesRoom`
- Usar `Fact` para tests sin parámetros
- Usar `Theory` + `InlineData` para tests parametrizados
- Arrange-Act-Assert (AAA) pattern
- Un assert por test cuando sea posible

## UI Tests

### Tecnologías

- **Framework**: Playwright for .NET
- **Test Runner**: xUnit
- **Navegadores**: Chromium, Firefox, Webkit
- **.NET**: 10.0

### Estructura

Los tests de interfaz de usuario se encuentran en `src/GameTribunal.Web.UI.Tests/`:

```
GameTribunal.Web.UI.Tests/
├── AccessibilityTests.cs           # Tests WCAG AA
├── DesignSystemTests.cs            # Tests de sistema de diseño
├── ResponsiveDesignTests.cs        # Tests de diseño responsivo
├── VisualRegressionTests.cs        # Tests de regresión visual
├── PlayerConnectionStatusTests.cs  # Tests de estado de conexión de jugadores (RF-013, RF-014, RF-015, RF-016)
├── PlaywrightTest.cs              # Clase base con configuración
└── README.md                       # Documentación específica
```

### Cobertura de Tests

#### AccessibilityTests (11 tests)
Valida cumplimiento WCAG AA:
- Contraste de colores adecuado
- Navegación por teclado funcional
- HTML semántico correcto
- Alt text en todas las imágenes
- Títulos de página descriptivos
- Labels asociados a inputs
- Estados disabled claros
- Soporte prefers-reduced-motion
- Text resize sin romper layout
- Mensajes de error accesibles
- Focus trap en modals

#### DesignSystemTests (10 tests)
Valida consistencia del diseño:
- Hero section con efectos visuales
- Botones con hover effects
- Cards con estilos profesionales
- Paleta de colores consistente
- Jerarquía tipográfica
- Animaciones suaves
- Efectos modernos (glassmorphism, gradientes)
- Sistema de spacing consistente
- Estados de foco visibles
- QR code display

#### ResponsiveDesignTests (10 tests)
Valida múltiples viewports:
- Mobile Portrait (375x667)
- Mobile Landscape (667x375)
- Tablet (768x1024)
- Desktop (1920x1080)
- TV/10-foot UI (1920x1080+)
- Sin overflow horizontal
- Imágenes responsivas
- Touch targets ≥44px
- Grid layouts adaptativos
- Text scaling apropiado

#### VisualRegressionTests (11 tests)
Captura screenshots para comparación:
- Lobby en múltiples resoluciones
- Room con QR code
- Hero section
- Game cards
- Botones (normal y hover)
- Home page
- Componentes individuales

#### PlayerConnectionStatusTests (9 tests)
Valida gestión de estado de conexión de jugadores:
- **RF-013**: Visualización de estados (Conectado, Inactivo, Desconectado)
- **RF-014**: Cambio a Inactivo después de 30 segundos de inactividad
- **RF-015**: Cambio a Desconectado después de 5 minutos de inactividad
- **RF-016**: Propagación en tiempo real de cambios de estado via SignalR
- Estructura de página de lobby
- Estructura de página de unirse a sala
- Formularios de configuración de juego

### Prerequisitos UI Tests

```bash
# Instalar navegadores Playwright
cd src/GameTribunal.Web.UI.Tests
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

Las suites Playwright arrancan automáticamente `GameTribunal.Web` mediante `WebApplicationFactory`, utilizando un puerto HTTPS dinámico para que el navegador pueda interactuar con la aplicación sin pasos manuales previos.

### Ejecutar UI Tests

```bash
# Todos los tests de UI
dotnet test src/GameTribunal.Web.UI.Tests/

# Tests específicos
dotnet test src/GameTribunal.Web.UI.Tests/ --filter "FullyQualifiedName~DesignSystemTests"
dotnet test src/GameTribunal.Web.UI.Tests/ --filter "FullyQualifiedName~AccessibilityTests"
dotnet test src/GameTribunal.Web.UI.Tests/ --filter "FullyQualifiedName~ResponsiveDesignTests"
dotnet test src/GameTribunal.Web.UI.Tests/ --filter "FullyQualifiedName~VisualRegressionTests"

# Ver resultados detallados
dotnet test src/GameTribunal.Web.UI.Tests/ --logger "console;verbosity=detailed"
```

### Screenshots

Los screenshots se generan automáticamente en:
```
src/GameTribunal.Web.UI.Tests/bin/Debug/net10.0/screenshots/
```

## Ejecutar Todos los Tests

```bash
# Desde la raíz del proyecto
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Solo tests unitarios
dotnet test --filter "FullyQualifiedName~Application.Tests"

# Solo tests de UI
dotnet test --filter "FullyQualifiedName~UI.Tests"
```

## CI/CD Integration

Los tests se ejecutan automáticamente en el pipeline de CI/CD definido en `.github/workflows/build-and-test.yml`:

1. **Build**: Compilación del proyecto
2. **Unit Tests**: Ejecución de tests unitarios
3. **UI Tests**: (Opcional) Ejecución de tests de UI

### Workflow Actual

```yaml
- Build (Release)
- Run Unit Tests (GameTribunal.Application.Tests)
```

## Best Practices

### Unit Tests
- ✅ Tests rápidos y deterministas
- ✅ Aislar dependencias con mocks
- ✅ Un test, un concepto
- ✅ Nombres descriptivos
- ✅ Evitar lógica compleja en tests
- ✅ Usar datos de test realistas

### UI Tests
- ✅ Usar selectores estables (id, data-testid)
- ✅ Waits explícitos cuando sea necesario
- ✅ Validar comportamiento, no implementación
- ✅ Screenshots para documentación
- ✅ Tests independientes y parallelizables
- ✅ Configuración compartida en clase base

## Métricas

### Unit Tests
- **Total**: ~15-20 tests
- **Cobertura**: Domain y Application layers
- **Velocidad**: < 5 segundos

### UI Tests
- **Total**: 51 tests (42 existing + 9 PlayerConnectionStatusTests)
- **Viewports**: 7 diferentes
- **Navegadores**: Chromium (principal)
- **Screenshots**: 15+ automáticos

## Roadmap

### Próximas Mejoras
- [ ] Aumentar cobertura de unit tests a 80%+
- [ ] Añadir integration tests para SignalR
- [ ] Tests de performance (LCP, FCP, CLS)
- [ ] Cross-browser testing (Firefox, Safari)
- [ ] Visual regression tracking automático
- [ ] Tests de carga para múltiples jugadores simultáneos

## Referencias

- [xUnit Documentation](https://xunit.net/)
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
