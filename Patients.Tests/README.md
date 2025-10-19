# ?? Pruebas Unitarias - API de Gesti�n de Pacientes

## ?? Descripci�n General

Este documento describe las pruebas unitarias implementadas para el proyecto **Patients API**, desarrollado como parte de una prueba t�cnica backend. Las pruebas validan los endpoints principales de la API utilizando **xUnit**, **Moq** y **FluentAssertions**.

---

## ?? Objetivo

Validar el correcto funcionamiento de los endpoints **Create** (POST) y **GetAll** (GET) del controlador de pacientes, cubriendo:
- ? Casos de �xito
- ? Casos de error y excepciones
- ? Validaciones de negocio
- ? Filtrado, ordenamiento y paginaci�n
- ? Casos l�mite (edge cases)

---

## ?? Resultados de Ejecuci�n

```
?????????????????????????????????????????
?  Resumen de Pruebas Unitarias         ?
?????????????????????????????????????????
?  Total de pruebas:            32      ?
?  ? Exitosas:                 32      ?
?  ? Fallidas:                  0      ?
?  ??  Tiempo de ejecuci�n:     ~0.8s   ?
?  ?? Cobertura de endpoints:   100%    ?
?????????????????????????????????????????
```

---

## ??? Tecnolog�as y Herramientas

| Herramienta | Versi�n | Prop�sito |
|-------------|---------|-----------|
| **xUnit** | 2.9.2 | Framework de pruebas unitarias |
| **Moq** | 4.20.72 | Creaci�n de mocks para dependencias |
| **FluentAssertions** | 8.7.1 | Aserciones expresivas y legibles |
| **.NET** | 8 / 9 | Framework de desarrollo |

### Instalaci�n de Dependencias
```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.NET.Test.Sdk
```

---

## ?? Estructura del Proyecto

```
Patients.Tests/
?
??? PatientsControllerBasicTests.cs        # 11 pruebas fundamentales
?   ??? Create Endpoint (4 pruebas)
?   ??? GetAll Endpoint (7 pruebas)
?
??? PatientsControllerAdvancedTests.cs     # 21 pruebas avanzadas
?   ??? Casos de excepci�n (3 pruebas)
?   ??? Theory con m�ltiples casos (13 pruebas)
?   ??? Casos especiales (5 pruebas)
?
??? README.md                              # Esta documentaci�n
```

---

## ?? Detalle de Pruebas Implementadas

### 1?? Endpoint: POST /api/patients (Create)

#### Pruebas B�sicas (4)

| # | Nombre de la Prueba | Descripci�n | Tipo |
|---|---------------------|-------------|------|
| 1 | `Create_WithValidData_ReturnsCreatedAtActionResult` | Verifica que se retorne status 201 Created con datos v�lidos | ? �xito |
| 2 | `Create_WithValidData_CallsServiceOnce` | Verifica que el servicio se invoque exactamente una vez | ? �xito |
| 3 | `Create_ReturnsCorrectPatientId` | Verifica que se retorne el ID correcto del paciente creado | ? �xito |
| 4 | `Create_WithMinimalData_ReturnsCreatedResult` | Prueba creaci�n solo con campos obligatorios | ? �xito |

#### Pruebas Avanzadas (12)

| # | Nombre de la Prueba | Descripci�n | Tipo |
|---|---------------------|-------------|------|
| 5 | `Create_WhenDuplicatePatientExists_ThrowsDuplicatePatientException` | Valida manejo de pacientes duplicados | ? Excepci�n |
| 6 | `Create_WhenValidationFails_ThrowsValidationFailedException` | Valida campos inv�lidos | ? Excepci�n |
| 7 | `Create_WithFutureDate_ValidatesBusinessRules` | Valida fecha de nacimiento futura | ? Excepci�n |
| 8-11 | `Create_WithDifferentDocumentTypes_CreatesSuccessfully` | Prueba 4 tipos de documento (CC, TI, CE, PA) | ? Theory |
| 12 | `Create_WithValidEmail_StoresEmailCorrectly` | Valida almacenamiento correcto del email | ? �xito |

### 2?? Endpoint: GET /api/patients (GetAll)

#### Pruebas B�sicas (7)

| # | Nombre de la Prueba | Descripci�n | Tipo |
|---|---------------------|-------------|------|
| 13 | `GetAll_WithoutFilters_ReturnsOkResultWithPagedData` | Listado completo sin filtros | ? �xito |
| 14 | `GetAll_WithNameFilter_CallsServiceWithCorrectParameters` | Filtrado por nombre (b�squeda parcial) | ? �xito |
| 15 | `GetAll_WithPaginationParameters_ReturnsCorrectPage` | Validaci�n de paginaci�n | ? �xito |
| 16 | `GetAll_WithSortingParameters_CallsServiceWithCorrectSort` | Ordenamiento por campos | ? �xito |
| 17 | `GetAll_WithMultipleFilters_CallsServiceWithAllFilters` | M�ltiples filtros combinados | ? �xito |
| 18 | `GetAll_WithEmptyResult_ReturnsEmptyList` | Manejo de resultados vac�os | ? �xito |
| 19 | `GetAll_ReturnsCorrectTotalPagesCalculation` | C�lculo correcto de p�ginas totales | ? �xito |

#### Pruebas Avanzadas (13)

| # | Nombre de la Prueba | Descripci�n | Tipo |
|---|---------------------|-------------|------|
| 20-23 | `GetAll_WithInvalidPaginationParameters_ServiceHandlesCorrectly` | 4 casos de paginaci�n inv�lida | ? Theory |
| 24-28 | `GetAll_WithDifferentSortOptions_HandlesCorrectly` | 5 opciones de ordenamiento | ? Theory |
| 29 | `GetAll_WithLargeDataset_ReturnsPaginatedResults` | Simulaci�n de 1000 registros | ? �xito |
| 30 | `GetAll_WithDateRangeFilter_ReturnsFilteredResults` | Filtrado por rango de fechas | ? �xito |
| 31 | `GetAll_WithNameFilter_PerformsPartialMatch` | B�squeda parcial por nombre | ? �xito |
| 32 | `GetAll_WithDocumentNumberFilter_ReturnsExactMatch` | B�squeda exacta por documento | ? �xito |

---

## ?? Patrones y Mejores Pr�cticas

### 1. Patr�n AAA (Arrange-Act-Assert)
Todas las pruebas siguen esta estructura para m�xima claridad:

```csharp
[Fact]
public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
{
    // Arrange: Preparar datos y configurar mocks
    var createDto = new CreatePatientDto { ... };
    _mockPatientService.Setup(...).ReturnsAsync(...);

    // Act: Ejecutar la acci�n a probar
    var result = await _controller.Create(createDto);

    // Assert: Verificar el resultado esperado
    result.Should().BeOfType<CreatedAtActionResult>();
}
```

### 2. Theory para Casos M�ltiples
Evita duplicaci�n de c�digo probando m�ltiples escenarios:

```csharp
[Theory]
[InlineData("CC")]
[InlineData("TI")]
[InlineData("CE")]
[InlineData("PA")]
public async Task Create_WithDifferentDocumentTypes_CreatesSuccessfully(string documentType)
{
    // Una sola prueba que se ejecuta 4 veces con diferentes valores
}
```

### 3. Mocking con Moq
Aisla el c�digo bajo prueba mockeando dependencias:

```csharp
_mockPatientService
    .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
    .ReturnsAsync(expectedPatient);
```

### 4. FluentAssertions
Aserciones m�s legibles y expresivas:

```csharp
// En lugar de:
Assert.Equal(typeof(CreatedAtActionResult), result.GetType());

// Usamos:
result.Should().BeOfType<CreatedAtActionResult>();
pagedResult.Items.Should().HaveCount(10);
returnedPatient.Email.Should().Be(validEmail);
```

### 5. Verificaci�n de Invocaciones
Garantiza que los m�todos se llamen correctamente:

```csharp
_mockPatientService.Verify(
    s => s.CreateAsync(It.Is<CreatePatientDto>(dto =>
        dto.DocumentType == "CC" &&
        dto.DocumentNumber == "123456789"
    )),
    Times.Once
);
```

---

## ?? Comandos de Ejecuci�n

### Comandos B�sicos

```bash
# Ejecutar todas las pruebas
dotnet test

# Con verbosidad detallada
dotnet test --verbosity detailed

# Con verbosidad m�nima
dotnet test --verbosity minimal
```

### Filtros por Archivo

```bash
# Solo pruebas b�sicas
dotnet test --filter "FullyQualifiedName~PatientsControllerBasicTests"

# Solo pruebas avanzadas
dotnet test --filter "FullyQualifiedName~PatientsControllerAdvancedTests"
```

### Filtros por Endpoint

```bash
# Solo pruebas de Create
dotnet test --filter "Create"

# Solo pruebas de GetAll
dotnet test --filter "GetAll"
```

### Filtros por Tipo

```bash
# Todas las pruebas de validaci�n y excepciones
dotnet test --filter "Throws"

# Pruebas con Theory (m�ltiples casos)
dotnet test --filter "WithDifferent"

# Pruebas de paginaci�n
dotnet test --filter "Pagination"
```

### Pruebas Espec�ficas

```bash
# Ejecutar una prueba espec�fica por nombre completo
dotnet test --filter "FullyQualifiedName=Patients.Tests.PatientsControllerTests.Create_WithValidData_ReturnsCreatedAtActionResult"
```

### Reportes y Cobertura

```bash
# Generar reporte de cobertura
dotnet test /p:CollectCoverage=true

# Generar archivo TRX de resultados
dotnet test --logger "trx;LogFileName=test_results.trx"

# Ejecutar con logger de consola detallado
dotnet test --logger "console;verbosity=detailed"
```

### Modo Watch (Desarrollo Continuo)

```bash
# Ejecutar pruebas autom�ticamente al detectar cambios
dotnet watch test
```

---

## ?? Cobertura de Funcionalidades

### Endpoint Create (POST /api/patients)

| Funcionalidad | Estado | Casos de Prueba |
|---------------|--------|-----------------|
| Creaci�n exitosa | ? | 4 pruebas |
| Validaci�n de campos | ? | 2 pruebas |
| Diferentes tipos de documento | ? | 4 pruebas (Theory) |
| Validaci�n de email | ? | 1 prueba |
| Manejo de duplicados | ? | 1 prueba |
| Validaci�n de fechas | ? | 1 prueba |
| Campos opcionales vs obligatorios | ? | 1 prueba |

### Endpoint GetAll (GET /api/patients)

| Funcionalidad | Estado | Casos de Prueba |
|---------------|--------|-----------------|
| Listado sin filtros | ? | 1 prueba |
| Filtrado por nombre (parcial) | ? | 2 pruebas |
| Filtrado por documento (exacto) | ? | 1 prueba |
| Filtrado por rango de fechas | ? | 1 prueba |
| M�ltiples filtros combinados | ? | 1 prueba |
| Ordenamiento (asc/desc) | ? | 5 pruebas (Theory) |
| Ordenamiento por diferentes campos | ? | 5 pruebas (Theory) |
| Paginaci�n v�lida | ? | 2 pruebas |
| Paginaci�n inv�lida | ? | 4 pruebas (Theory) |
| C�lculo de p�ginas totales | ? | 2 pruebas |
| Resultados vac�os | ? | 1 prueba |
| Datasets grandes | ? | 1 prueba |

---

## ?? Ejemplos de C�digo

### Ejemplo 1: Prueba de Creaci�n Exitosa

```csharp
[Fact]
public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
{
    // Arrange
    var createDto = new CreatePatientDto
    {
        DocumentType = "CC",
        DocumentNumber = "123456789",
        FirstName = "Juan",
        LastName = "P�rez",
        BirthDate = new DateTime(1990, 1, 1),
        Email = "juan.perez@example.com",
        PhoneNumber = "3001234567"
    };

    var expectedPatient = new PatientDto
    {
        PatientId = 1,
        DocumentType = "CC",
        DocumentNumber = "123456789",
        FirstName = "Juan",
        LastName = "P�rez",
        BirthDate = new DateTime(1990, 1, 1),
        Email = "juan.perez@example.com",
        PhoneNumber = "3001234567",
        CreatedAt = DateTime.UtcNow
    };

    _mockPatientService
        .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
        .ReturnsAsync(expectedPatient);

    // Act
    var result = await _controller.Create(createDto);

    // Assert
    result.Should().BeOfType<CreatedAtActionResult>();
    var createdResult = result as CreatedAtActionResult;
    createdResult!.ActionName.Should().Be(nameof(_controller.GetById));
    createdResult.RouteValues!["id"].Should().Be(expectedPatient.PatientId);
    createdResult.Value.Should().BeEquivalentTo(expectedPatient);
}
```

### Ejemplo 2: Prueba de Listado con Filtros

```csharp
[Fact]
public async Task GetAll_WithNameFilter_CallsServiceWithCorrectParameters()
{
    // Arrange
    var query = new PatientQueryParamsDto
    {
        Name = "Juan",
        Page = 1,
        PageSize = 10
    };

    var expectedResult = new PagedResultDto<PatientDto>
    {
        Items = new List<PatientDto>
        {
            new PatientDto
            {
                PatientId = 1,
                DocumentType = "CC",
                DocumentNumber = "123456789",
                FirstName = "Juan",
                LastName = "P�rez",
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            }
        },
        TotalCount = 1,
        Page = 1,
        PageSize = 10
    };

    _mockPatientService
        .Setup(s => s.GetAllAsync(It.IsAny<PatientQueryParamsDto>()))
        .ReturnsAsync(expectedResult);

    // Act
    await _controller.GetAll(query);

    // Assert
    _mockPatientService.Verify(
        s => s.GetAllAsync(It.Is<PatientQueryParamsDto>(q =>
            q.Name == "Juan" &&
            q.Page == 1 &&
            q.PageSize == 10
        )),
        Times.Once
    );
}
```

### Ejemplo 3: Prueba con Theory (M�ltiples Casos)

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

### Ejemplo 4: Prueba de Excepci�n

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
        LastName = "P�rez",
        BirthDate = new DateTime(1990, 1, 1)
    };

    _mockPatientService
        .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
        .ThrowsAsync(new DuplicatePatientException(
            "Ya existe un paciente con el mismo tipo y n�mero de documento."
        ));

    // Act
    Func<Task> act = async () => await _controller.Create(createDto);

    // Assert
    await act.Should().ThrowAsync<DuplicatePatientException>()
        .WithMessage("Ya existe un paciente con el mismo tipo y n�mero de documento.");
}
```

---

## ?? Troubleshooting

### Problema: Las pruebas no se ejecutan

**Soluci�n:**
```bash
# Limpiar y reconstruir
dotnet clean
dotnet build
dotnet test
```

### Problema: Error de dependencias

**Soluci�n:**
```bash
# Restaurar paquetes NuGet
dotnet restore
dotnet build
dotnet test
```

### Problema: Timeout en pruebas

**Soluci�n:**
```bash
# Aumentar timeout
dotnet test --blame-hang-timeout 5m
```

### Problema: Pruebas no aparecen en Test Explorer (Visual Studio)

**Soluci�n:**
1. Abrir **Test Explorer** (Ctrl+E, T)
2. Click en bot�n de **Refresh**
3. Asegurarse de que el proyecto est� compilado
4. Verificar que los paquetes xUnit est�n instalados

---

## ? Cumplimiento de Requisitos

| Requisito de la Prueba T�cnica | Estado | Evidencia |
|--------------------------------|--------|-----------|
| Implementar pruebas unitarias | ? Completado | 32 pruebas implementadas |
| Usar xUnit o NUnit | ? Completado | xUnit implementado |
| Probar al menos 2 endpoints | ? Completado | Create y GetAll probados |
| Casos de �xito | ? Completado | 29 casos de �xito |
| Casos de error | ? Completado | 3 casos de excepci�n |
| Mocking de dependencias | ? Completado | Mock de IPatientService |
| Aserciones claras y expresivas | ? Completado | FluentAssertions usado |
| Documentaci�n completa | ? Completado | Este README |

---

## ?? Recursos y Referencias

### Documentaci�n Oficial
- [xUnit Documentation](https://xunit.net/) - Framework de pruebas
- [Moq Quickstart](https://github.com/moq/moq4) - Librer�a de mocking
- [FluentAssertions](https://fluentassertions.com/) - Aserciones expresivas

### Gu�as de Buenas Pr�cticas
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [AAA Pattern](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics)

---
