# Patients.Tests - Documentación de Pruebas Unitarias

## Descripción

Este proyecto contiene las pruebas unitarias para la API de Pacientes. Las pruebas están diseñadas para validar la lógica de negocio del controlador `PatientsController` usando el patrón **AAA** (Arrange, Act, Assert).

---

## Tecnologías Utilizadas

- **xUnit** - Framework de pruebas unitarias
- **Moq** - Librería para crear mocks de dependencias
- **FluentAssertions** - Librería para aserciones expresivas y legibles

---

## Estructura del Proyecto

```
Patients.Tests/
├── PatientsControllerBasicTests.cs       # Pruebas básicas (Create, GetAll)
├── PatientsControllerAdvancedTests.cs    # Pruebas avanzadas y casos edge
└── README.md                              # Este archivo
```

---

## Ejecutar las Pruebas

### Desde Visual Studio

1. Abrir **Test Explorer** (Menú: Test → Test Explorer o `Ctrl+E, T`)
2. Click en **Run All** para ejecutar todas las pruebas
3. Ver resultados en tiempo real

### Desde la Terminal

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con output detallado
dotnet test --logger "console;verbosity=detailed"

# Ejecutar solo un proyecto específico
dotnet test Patients.Tests/Patients.Tests.csproj

# Ejecutar con cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

### Usando el script de PowerShell

```powershell
# Ejecutar todas las pruebas
.\run-tests.ps1

# Ejecutar con reporte de cobertura
.\run-tests.ps1 -Coverage
```

---

## Cobertura de Pruebas

### Endpoints Probados

| Endpoint | Método | Pruebas |
|----------|--------|---------|
| `/api/patients` | GET | 8 pruebas |
| `/api/patients` | POST | 9 pruebas |
| `/api/patients/{id}` | GET | Pendiente |
| `/api/patients/{id}` | PUT | Pendiente |
| `/api/patients/{id}` | DELETE | Pendiente |
| `/api/patients/export` | GET | Pendiente |

**Total de pruebas implementadas:** 32

---

## Casos de Prueba

### PatientsControllerBasicTests.cs

#### 1. Create Endpoint

| Prueba | Descripción |
|--------|-------------|
| `Create_WithValidData_ReturnsCreatedAtActionResult` | Verifica que se crea un paciente y retorna `201 Created` |
| `Create_WithValidData_CallsServiceOnce` | Verifica que el servicio se llame exactamente una vez |
| `Create_ReturnsCorrectPatientId` | Verifica que el ID retornado es correcto |
| `Create_WithMinimalData_ReturnsCreatedResult` | Verifica creación con solo campos obligatorios |

#### 2. GetAll Endpoint

| Prueba | Descripción |
|--------|-------------|
| `GetAll_WithoutFilters_ReturnsOkResultWithPagedData` | Retorna datos paginados sin filtros |
| `GetAll_WithNameFilter_CallsServiceWithCorrectParameters` | Filtra por nombre correctamente |
| `GetAll_WithPaginationParameters_ReturnsCorrectPage` | Paginación funciona correctamente |
| `GetAll_WithSortingParameters_CallsServiceWithCorrectSort` | Ordenamiento funciona correctamente |
| `GetAll_WithMultipleFilters_CallsServiceWithAllFilters` | Múltiples filtros funcionan juntos |
| `GetAll_WithEmptyResult_ReturnsEmptyList` | Retorna lista vacía cuando no hay resultados |
| `GetAll_ReturnsCorrectTotalPagesCalculation` | Calcula correctamente el total de páginas |

### PatientsControllerAdvancedTests.cs

#### 1. Casos de Excepción

| Prueba | Descripción |
|--------|-------------|
| `Create_WhenDuplicatePatientExists_ThrowsDuplicatePatientException` | Lanza excepción cuando el paciente ya existe |
| `Create_WhenValidationFails_ThrowsValidationFailedException` | Lanza excepción cuando la validación falla |
| `Create_WithFutureDate_ValidatesBusinessRules` | Valida que la fecha de nacimiento no sea futura |

#### 2. Casos Edge

| Prueba | Descripción |
|--------|-------------|
| `GetAll_WithInvalidPaginationParameters_ServiceHandlesCorrectly` | Maneja parámetros de paginación inválidos |
| `GetAll_WithLargeDataset_ReturnsPaginatedResults` | Maneja grandes conjuntos de datos |
| `GetAll_WithDifferentSortOptions_HandlesCorrectly` | Prueba diferentes opciones de ordenamiento |
| `GetAll_WithDateRangeFilter_ReturnsFilteredResults` | Filtra por rango de fechas |
| `Create_WithDifferentDocumentTypes_CreatesSuccessfully` | Crea pacientes con diferentes tipos de documento |
| `GetAll_WithNameFilter_PerformsPartialMatch` | Búsqueda parcial por nombre |
| `Create_WithValidEmail_StoresEmailCorrectly` | Almacena email correctamente |
| `GetAll_WithDocumentNumberFilter_ReturnsExactMatch` | Búsqueda exacta por número de documento |

---

## Patrón AAA (Arrange-Act-Assert)

Todas las pruebas siguen el patrón AAA para mantener claridad y consistencia:

### Ejemplo 1: Prueba Básica

```csharp
[Fact]
public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
{
    // Arrange - Preparar datos de prueba y configurar mocks
    var createDto = new CreatePatientDto
    {
        DocumentType = "CC",
        DocumentNumber = "123456789",
        FirstName = "Juan",
        LastName = "Pérez",
        BirthDate = new DateTime(1990, 1, 1)
    };

    var expectedPatient = new PatientDto
    {
        PatientId = 1,
        DocumentType = "CC",
        DocumentNumber = "123456789",
        FirstName = "Juan",
        LastName = "Pérez",
        BirthDate = new DateTime(1990, 1, 1),
        CreatedAt = DateTime.UtcNow
    };

    _mockPatientService
        .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
        .ReturnsAsync(expectedPatient);

    // Act - Ejecutar la acción a probar
    var result = await _controller.Create(createDto);

    // Assert - Verificar el resultado
    result.Should().BeOfType<CreatedAtActionResult>();
    var createdResult = result as CreatedAtActionResult;
    createdResult!.Value.Should().BeEquivalentTo(expectedPatient);
}
```

### Ejemplo 2: Prueba con Verificación de Mock

```csharp
[Fact]
public async Task Create_WithValidData_CallsServiceOnce()
{
    // Arrange
    var createDto = new CreatePatientDto
    {
        DocumentType = "CC",
        DocumentNumber = "123456789",
        FirstName = "María",
        LastName = "González",
        BirthDate = new DateTime(1995, 5, 15)
    };

    // Act
    await _controller.Create(createDto);

    // Assert
    _mockPatientService.Verify(
        s => s.CreateAsync(It.Is<CreatePatientDto>(dto =>
            dto.DocumentType == createDto.DocumentType &&
            dto.DocumentNumber == createDto.DocumentNumber
        )),
        Times.Once
    );
}
```

### Ejemplo 3: Prueba Parametrizada

```csharp
[Theory]
[InlineData("CC")]
[InlineData("TI")]
[InlineData("CE")]
[InlineData("PA")]
public async Task Create_WithDifferentDocumentTypes_CreatesSuccessfully(string documentType)
{
    // Arrange
    var createDto = new CreatePatientDto
    {
        DocumentType = documentType,
        DocumentNumber = "123456789",
        FirstName = "Test",
        LastName = "Patient",
        BirthDate = new DateTime(1990, 1, 1)
    };

    var expectedPatient = new PatientDto
    {
        PatientId = 1,
        DocumentType = documentType,
        DocumentNumber = "123456789",
        FirstName = "Test",
        LastName = "Patient",
        BirthDate = new DateTime(1990, 1, 1),
        CreatedAt = DateTime.UtcNow
    };

    _mockPatientService
        .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
        .ReturnsAsync(expectedPatient);

    // Act
    var result = await _controller.Create(createDto);

    // Assert
    var createdResult = result as CreatedAtActionResult;
    createdResult.Should().NotBeNull();
    var returnedPatient = createdResult!.Value as PatientDto;
    returnedPatient!.DocumentType.Should().Be(documentType);
}
```

### Ejemplo 4: Prueba de Excepción

```csharp
[Fact]
public async Task Create_WhenDuplicatePatientExists_ThrowsDuplicatePatientException()
{
    // Arrange
    var createDto = new CreatePatientDto
    {
        DocumentType = "CC",
        DocumentNumber = "123456789",
        FirstName = "Juan",
        LastName = "Pérez",
        BirthDate = new DateTime(1990, 1, 1)
    };

    _mockPatientService
        .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
        .ThrowsAsync(new DuplicatePatientException(
            "Ya existe un paciente con el mismo tipo y número de documento."
        ));

    // Act
    Func<Task> act = async () => await _controller.Create(createDto);

    // Assert
    await act.Should().ThrowAsync<DuplicatePatientException>()
        .WithMessage("Ya existe un paciente con el mismo tipo y número de documento.");
}
```

---

## 🔧 Troubleshooting

### Problema: Las pruebas no se ejecutan

**Solución:**
```bash
# Limpiar y reconstruir
dotnet clean
dotnet build
dotnet test
```

### Problema: Error de dependencias

**Solución:**
```bash
# Restaurar paquetes NuGet
dotnet restore
dotnet build
dotnet test
```

### Problema: Timeout en pruebas

**Solución:**
```bash
# Aumentar timeout
dotnet test --blame-hang-timeout 5m
```

### Problema: Pruebas no aparecen en Test Explorer (Visual Studio)

**Solución:**
1. Abrir **Test Explorer** (Ctrl+E, T)
2. Click en botón de **Refresh**
3. Asegurarse de que el proyecto esté compilado
4. Verificar que los paquetes xUnit estén instalados
