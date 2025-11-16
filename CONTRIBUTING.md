# Guía de Contribución

¡Gracias por tu interés en contribuir a **Tribunal Social**! Este documento proporciona pautas para colaborar en el proyecto.

---

## Cómo contribuir

### 1. Reportar problemas (Issues)

Si encuentras un bug o tienes una sugerencia:

1. Verifica que no exista ya un issue similar
2. Crea un nuevo issue con:
   - **Título descriptivo**
   - **Descripción clara** del problema o sugerencia
   - **Pasos para reproducir** (si es un bug)
   - **Comportamiento esperado vs. actual**
   - **Capturas de pantalla** si aplica

### 2. Proponer cambios (Pull Requests)

#### Antes de empezar

1. **Lee la documentación:**
   - `README.md` para visión general
   - `docs/requirements.md` para requisitos funcionales
   - `docs/game-logic.md` para entender las reglas del juego
   - `docs/architecture.md` para la estructura del código

2. **Configura tu entorno:**
   ```bash
   git clone https://github.com/eugenioestrada/unir-tfe.git
   cd unir-tfe
   # Seguir instrucciones de README.md
   ```

3. **Crea una rama:**
   ```bash
   git checkout -b feature/nombre-descriptivo
   # o
   git checkout -b fix/nombre-del-bug
   ```

#### Durante el desarrollo

1. **Sigue las convenciones del proyecto:**
   - Usa las mismas convenciones de nombres que el código existente
   - Respeta la arquitectura en capas (Domain, Application, Infrastructure, Web)
   - Escribe código en español (comentarios, nombres de variables) para consistencia

2. **Escribe tests:**
   - Tests unitarios para lógica de dominio
   - Tests de integración para servicios y APIs
   - Asegúrate de que todos los tests pasen antes de hacer commit

3. **Documenta tus cambios:**
   - Actualiza la documentación si tu cambio afecta el comportamiento
   - Agrega comentarios XML para métodos públicos
   - Actualiza el CHANGELOG si existe

4. **Commits significativos:**
   ```bash
   git commit -m "feat: Agregar validación de alias duplicados"
   git commit -m "fix: Corregir cálculo de acusado en empate"
   git commit -m "docs: Actualizar diagrama de secuencia de defensa"
   ```

   Prefijos recomendados:
   - `feat:` nueva funcionalidad
   - `fix:` corrección de bug
   - `docs:` cambios en documentación
   - `test:` agregar o modificar tests
   - `refactor:` refactorización sin cambio funcional
   - `style:` formateo, punto y coma faltantes
   - `perf:` mejora de rendimiento

#### Enviar el Pull Request

1. **Asegúrate de que todo funciona:**
   ```bash
   dotnet build
   dotnet test
   ```

2. **Push a tu fork:**
   ```bash
   git push origin feature/nombre-descriptivo
   ```

3. **Crea el PR en GitHub:**
   - Título claro y descriptivo
   - Descripción detallada de los cambios
   - Referencia a issues relacionados: "Closes #123"
   - Capturas de pantalla si hay cambios visuales

4. **Espera revisión:**
   - Responde a comentarios y feedback
   - Realiza cambios solicitados
   - Mantén la conversación profesional y constructiva

---

## Estándares de Código

### C# / .NET

- **Estilo:** Seguir las [convenciones de C#](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Nomenclatura:**
  - PascalCase para clases, métodos, propiedades
  - camelCase para variables locales y parámetros
  - `_camelCase` para campos privados
- **Async:** Usar `async/await` para operaciones I/O
- **Nullability:** Aprovechar nullable reference types de C# 9+

### Arquitectura

- **Domain:** Sin dependencias externas, lógica pura
- **Application:** Solo interfaces de infraestructura
- **Infrastructure:** Implementaciones concretas
- **Web:** Presentación y comunicación

No mezclar responsabilidades entre capas.

### Tests

- **Nomenclatura:** `MethodName_Scenario_ExpectedBehavior`
- **AAA Pattern:** Arrange, Act, Assert
- **Mocks:** Usar Moq para dependencias externas
- **Cobertura:** Mínimo 80% en lógica de negocio

Ejemplo:
```csharp
[Fact]
public void AddAccusation_WhenPlayerAccusesSelf_ThrowsInvalidOperationException()
{
    // Arrange
    var round = new Round(/* ... */);
    var player = new Player("Alice");
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => 
        round.AddAccusation(player, player));
}
```

---

## Áreas de Contribución

### Código

- ✅ Implementación de entidades de dominio
- ✅ Desarrollo del GameEngine
- ✅ Servicios de aplicación
- ✅ Componentes Blazor
- ✅ SignalR Hubs
- ✅ Tests unitarios e integración

### Documentación

- ✅ Mejorar documentación existente
- ✅ Agregar ejemplos de uso
- ✅ Traducir documentación (inglés/español)
- ✅ Crear tutoriales y guías

### Diseño

- ✅ Crear casos de juego (CaseDefinition)
- ✅ Diseñar interfaz de usuario
- ✅ Mejorar experiencia de usuario
- ✅ Crear assets gráficos

### Testing

- ✅ Pruebas manuales en diferentes dispositivos
- ✅ Reportar bugs encontrados
- ✅ Validar experiencia de usuario
- ✅ Pruebas de carga y rendimiento

---

## Comunicación

### Canales

- **Issues:** Para bugs, features, y discusiones técnicas
- **Pull Requests:** Para revisión de código
- **Discussions:** Para preguntas generales (si está habilitado)

### Etiqueta

- ✅ Sé respetuoso y constructivo
- ✅ Acepta críticas como oportunidades de aprendizaje
- ✅ Reconoce el trabajo de otros
- ✅ Pregunta si tienes dudas
- ❌ No hagas spam ni publicidad
- ❌ No seas hostil ni agresivo

---

## Proceso de Revisión

Los pull requests serán revisados considerando:

1. **Funcionalidad:** ¿El código hace lo que debe hacer?
2. **Calidad:** ¿El código es legible, mantenible y eficiente?
3. **Tests:** ¿Hay tests adecuados y pasan?
4. **Documentación:** ¿Los cambios están documentados?
5. **Consistencia:** ¿Se sigue el estilo del proyecto?

**Tiempo de respuesta:** Se intentará revisar PRs en 2-3 días hábiles.

---

## Licencia

Al contribuir, aceptas que tu código se distribuirá bajo la misma licencia del proyecto (MIT).

---

## Preguntas Frecuentes

### ¿Puedo contribuir si soy principiante?

¡Absolutamente! Busca issues etiquetados como `good first issue` o `help wanted`. Si tienes dudas, pregunta.

### ¿Cómo encuentro qué hacer?

1. Revisa los issues abiertos
2. Mira el roadmap en `docs/planning.md`
3. Propón tus propias ideas

### ¿Necesito permiso para trabajar en algo?

Para cambios pequeños (typos, bugfixes menores), no es necesario. Para features grandes, abre un issue primero para discutir el enfoque.

### ¿Qué hago si mi PR no es aceptado?

No te desanimes. Lee el feedback, aprende de él, y considera si puedes hacer ajustes. No todos los PRs se aceptan, pero todos son apreciados.

---

## Recursos

- [Documentación de .NET](https://docs.microsoft.com/dotnet/)
- [Guía de SignalR](https://docs.microsoft.com/aspnet/core/signalr/)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

---

¡Gracias por contribuir a **Tribunal Social**! 🎉
