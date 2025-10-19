# Patients API - Prueba Técnica Backend .NET

API RESTful para la gestión de pacientes desarrollada en .NET Core con SQL Server, implementando Entity Framework Core, procedimientos almacenados, validaciones, paginación y pruebas unitarias.

---

## 📋 Tabla de Contenidos

- [Descripción General](#-descripción-general)
- [Arquitectura](#-arquitectura)
- [Tecnologías Utilizadas](#-tecnologías-utilizadas)
- [Requisitos del Sistema](#-requisitos-del-sistema)
- [Instalación y Configuración](#-instalación-y-configuración)
- [Endpoints de la API](#-endpoints-de-la-api)
- [Características Implementadas](#-características-implementadas)
- [Decisiones Técnicas](#-decisiones-técnicas)
- [Pruebas](#-pruebas)
- [Base de Datos](#-base-de-datos)
- [Docker](#-docker)

---

## 🎯 Descripción General

Esta API permite realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre un sistema de gestión de pacientes, con características avanzadas como:

- Validación de datos con FluentValidation
- Control de concurrencia optimista (RowVersion)
- Sistema de auditoría automática
- Exportación de datos a CSV
- Paginación y filtrado de resultados
- Procedimientos almacenados para consultas optimizadas
- Manejo global de errores mediante middleware
- Documentación interactiva con Swagger

---

## 🏗️ Arquitectura

El proyecto sigue una **arquitectura en capas** (Clean Architecture) con separación clara de responsabilidades:

```
Patients.Api/
├── Patients.Api/              # Capa de presentación (API)
│   ├── Controllers/           # Controladores REST
│   ├── Middlewares/           # Middleware personalizado
│   └── Program.cs             # Configuración de la aplicación
│
├── Patients.Application/      # Capa de aplicación (lógica de negocio)
│   ├── DTOs/                  # Data Transfer Objects
│   ├── Interfaces/            # Interfaces de servicios
│   ├── Mappings/              # Configuración de AutoMapper
│   ├── Services/              # Implementación de servicios
│   ├── Validators/            # Validadores FluentValidation
│   └── Exceptions/            # Excepciones personalizadas
│
├── Patients.Domain/           # Capa de dominio (entidades)
│   └── Entities/              # Modelos de dominio
│
├── Patients.Infrastructure/   # Capa de infraestructura (datos)
│   ├── Persistence/           # DbContext y configuración EF
│   └── Migrations/            # Migraciones de base de datos
│
└── Patients.Tests/            # Proyecto de pruebas unitarias
    └── Tests con xUnit, Moq y FluentAssertions
```

### Principios Aplicados

- **Separación de Responsabilidades**: Cada capa tiene un propósito específico
- **Inyección de Dependencias**: Configurada en `Program.cs`
- **Repository Pattern**: A través de DbContext de Entity Framework
- **DTO Pattern**: Separación entre entidades de dominio y objetos de transferencia
- **SOLID Principles**: Especialmente principio de responsabilidad única

---

## 🛠️ Tecnologías Utilizadas

### Backend
- **.NET 8.0** - Framework principal
- **ASP.NET Core 8.0** - API RESTful
- **Entity Framework Core 9.0** - ORM
- **SQL Server** - Base de datos relacional

### Librerías y Paquetes
- **AutoMapper 13.0.1** - Mapeo objeto a objeto
- **FluentValidation 11.11.0** - Validación de modelos
- **Swashbuckle (Swagger) 7.2.0** - Documentación de API

### Pruebas
- **xUnit 2.9.2** - Framework de pruebas unitarias
- **Moq 4.20.72** - Librería de mocking
- **FluentAssertions 7.0.0** - Aserciones expresivas

### DevOps
- **Docker** - Containerización
- **Docker Compose** - Orquestación de contenedores

---

## 💻 Requisitos del Sistema

### Requisitos Obligatorios

- **.NET 8.0 SDK** o superior
- **SQL Server 2019** o superior (o SQL Server en Docker)
- **Docker Desktop** (opcional, para ejecución con Docker)

### Requisitos Opcionales

- **Visual Studio 2022** (v17.8+) o **Visual Studio Code**
- **SQL Server Management Studio (SSMS)** para administración de BD
- **Postman** o **Insomnia** para pruebas de API (Swagger incluido)

---

## 🚀 Instalación y Configuración

### Opción 1: Ejecución con Docker (Recomendado)

#### Paso 1: Clonar el Repositorio

```bash
git clone https://github.com/seanrito/Patients.Api.git
cd Patients.Api
```

#### Paso 2: Iniciar los Servicios

```bash
# Construir e iniciar todos los contenedores
docker-compose up --build

# O usar el script PowerShell incluido
.\run-docker.ps1 build
```

#### Paso 3: Acceder a la API

- **Swagger UI**: http://localhost:5000/swagger
- **API Base URL**: http://localhost:5000/api

#### Comandos Útiles de Docker

```bash
# Ver estado de contenedores
docker-compose ps

# Ver logs
docker-compose logs -f patients-api

# Detener servicios
docker-compose down

# Detener y eliminar volúmenes (borra la BD)
docker-compose down -v
```

---

### Opción 2: Ejecución Local (Sin Docker)

#### Paso 1: Configurar la Base de Datos

1. **Instalar SQL Server** (si no lo tienes)

2. **Crear la base de datos** (se crea automáticamente al ejecutar la app)

3. **Configurar la cadena de conexión** en `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=NOMBRE_SERVIDOR;Database=PatientsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Reemplaza `NOMBRE_SERVIDOR` con tu instancia de SQL Server (ej: `localhost`, `.\SQLEXPRESS`, o el nombre de tu PC).

#### Paso 2: Restaurar Dependencias

```bash
# Desde la raíz del proyecto
dotnet restore
```

#### Paso 3: Aplicar Migraciones

```bash
# Las migraciones se aplican automáticamente al iniciar la aplicación
# Pero puedes aplicarlas manualmente con:
dotnet ef database update --project Patients.Infrastructure --startup-project Patients.Api
```

#### Paso 4: Ejecutar la Aplicación

**Desde Visual Studio:**
1. Abrir `Patients.Api.sln`
2. Establecer `Patients.Api` como proyecto de inicio
3. Presionar `F5` o clic en "Run"

**Desde la Terminal:**
```bash
cd Patients.Api
dotnet run
```

#### Paso 5: Acceder a Swagger

Abre tu navegador en: https://localhost:5001/swagger

---

## 📡 Endpoints de la API

### Base URL
```
http://localhost:5000/api/patients
```

### Endpoints Disponibles

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/patients` | Obtener lista paginada de pacientes |
| `GET` | `/api/patients/{id}` | Obtener un paciente por ID |
| `POST` | `/api/patients` | Crear un nuevo paciente |
| `PUT` | `/api/patients/{id}` | Actualizar un paciente existente |
| `DELETE` | `/api/patients/{id}` | Eliminar un paciente |
| `GET` | `/api/patients/export` | Exportar pacientes a CSV |

---

### 1. Crear Paciente

**Request:**
```http
POST /api/patients
Content-Type: application/json

{
  "documentType": "CC",
  "documentNumber": "123456789",
  "firstName": "Juan",
  "lastName": "Perez",
  "birthDate": "1990-05-15",
  "email": "juan.perez@example.com",
  "phoneNumber": "3001234567"
}
```

**Response (201 Created):**
```json
{
  "patientId": 1,
  "documentType": "CC",
  "documentNumber": "123456789",
  "firstName": "Juan",
  "lastName": "Perez",
  "birthDate": "1990-05-15",
  "email": "juan.perez@example.com",
  "phoneNumber": "3001234567",
  "createdAt": "2025-01-20T10:30:00Z",
  "rowVersion": "AAAAAAAAB9E="
}
```

**Validaciones:**
- `documentType`: Requerido, máximo 10 caracteres
- `documentNumber`: Requerido, máximo 20 caracteres, único por tipo de documento
- `firstName`: Requerido, máximo 80 caracteres
- `lastName`: Requerido, máximo 80 caracteres
- `birthDate`: Requerido, no puede ser fecha futura
- `email`: Opcional, debe ser válido si se proporciona
- `phoneNumber`: Opcional, máximo 20 caracteres

---

### 2. Listar Pacientes (Paginado)

**Request:**
```http
GET /api/patients?page=1&pageSize=10&name=Juan&sortBy=CreatedAt&sortDir=desc
```

**Parámetros de Query:**

| Parámetro | Tipo | Descripción | Requerido |
|-----------|------|-------------|-----------|
| `page` | int | Número de página (default: 1) | No |
| `pageSize` | int | Elementos por página (máx: 100, default: 10) | No |
| `name` | string | Filtro por nombre o apellido (búsqueda parcial) | No |
| `documentNumber` | string | Filtro por número de documento (búsqueda exacta) | No |
| `createdFrom` | date | Filtro de pacientes creados desde esta fecha | No |
| `createdTo` | date | Filtro de pacientes creados hasta esta fecha | No |
| `sortBy` | string | Campo para ordenar: `FirstName`, `LastName`, `CreatedAt` | No |
| `sortDir` | string | Dirección: `asc` o `desc` (default: `asc`) | No |

**Response (200 OK):**
```json
{
  "items": [
    {
      "patientId": 1,
      "documentType": "CC",
      "documentNumber": "123456789",
      "firstName": "Juan",
      "lastName": "Perez",
      "birthDate": "1990-05-15",
      "email": "juan.perez@example.com",
      "phoneNumber": "3001234567",
      "createdAt": "2025-01-20T10:30:00Z",
      "rowVersion": "AAAAAAAAB9E="
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

### 3. Obtener Paciente por ID

**Request:**
```http
GET /api/patients/1
```

**Response (200 OK):**
```json
{
  "patientId": 1,
  "documentType": "CC",
  "documentNumber": "123456789",
  "firstName": "Juan",
  "lastName": "Perez",
  "birthDate": "1990-05-15",
  "email": "juan.perez@example.com",
  "phoneNumber": "3001234567",
  "createdAt": "2025-01-20T10:30:00Z",
  "rowVersion": "AAAAAAAAB9E="
}
```

**Response (404 Not Found):**
```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no existe"
}
```

---

### 4. Actualizar Paciente

**Request:**
```http
PUT /api/patients/1
Content-Type: application/json

{
  "documentType": "CC",
  "documentNumber": "123456789",
  "firstName": "Juan Carlos",
  "lastName": "Perez",
  "birthDate": "1990-05-15",
  "email": "juancarlos.perez@example.com",
  "phoneNumber": "3001234567",
  "rowVersion": "AAAAAAAAB9E="
}
```

> **Nota sobre RowVersion**: El campo `rowVersion` es obligatorio para actualizaciones y garantiza control de concurrencia optimista. Si otro usuario modificó el registro, recibirás un error 409 Conflict.

**Response (200 OK):**
```json
{
  "patientId": 1,
  "documentType": "CC",
  "documentNumber": "123456789",
  "firstName": "Juan Carlos",
  "lastName": "Perez",
  "birthDate": "1990-05-15",
  "email": "juancarlos.perez@example.com",
  "phoneNumber": "3001234567",
  "createdAt": "2025-01-20T10:30:00Z",
  "rowVersion": "AAAAAAAAB9F="
}
```

**Response (409 Conflict):**
```json
{
  "title": "Concurrency Error",
  "status": 409,
  "detail": "El registro fue modificado por otro usuario. Por favor, recarga los datos e intenta nuevamente."
}
```

---

### 5. Eliminar Paciente

**Request:**
```http
DELETE /api/patients/1
```

**Response (204 No Content)**

**Response (404 Not Found):**
```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no existe"
}
```

---

### 6. Exportar a CSV

**Request:**
```http
GET /api/patients/export?createdFrom=2025-01-01&createdTo=2025-01-31&name=Juan
```

**Parámetros de Query:**

| Parámetro | Tipo | Descripción | Requerido |
|-----------|------|-------------|-----------|
| `createdFrom` | date | Fecha inicial (formato: yyyy-MM-dd) | **Sí** |
| `createdTo` | date | Fecha final (formato: yyyy-MM-dd) | No |
| `name` | string | Filtro por nombre o apellido | No |
| `documentNumber` | string | Filtro por número de documento | No |

**Response (200 OK):**
```csv
DocumentType,DocumentNumber,FirstName,LastName,Email,BirthDate,CreatedAt
CC,123456789,Juan,Perez,juan.perez@example.com,1990-05-15,2025-01-20 10:30:00
TI,987654321,Maria,Gonzalez,maria.g@example.com,2005-03-10,2025-01-21 14:20:00
```

> **Content-Type**: `text/csv`  
> **Nombre del archivo**: `patients_YYYYMMDD_HHmmss.csv`

**Response (400 Bad Request):**
```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "El parámetro 'createdFrom' es obligatorio para exportar pacientes."
}
```

---

## ✨ Características Implementadas

### 1. Entity Framework Core

- **Code First**: Modelos definidos en código
- **Migraciones automáticas**: Se aplican al iniciar la aplicación
- **Relaciones**: Configuradas con Fluent API
- **Optimización**: `AsNoTracking()` para consultas de solo lectura

### 2. Procedimientos Almacenados

**sp_GetPatientsForExport**

Procedimiento optimizado para exportación de pacientes con filtros:

```sql
CREATE PROCEDURE sp_GetPatientsForExport
    @Name NVARCHAR(100) = NULL,
    @DocumentNumber NVARCHAR(20) = NULL,
    @CreatedFrom DATETIME2 = NULL,
    @CreatedTo DATETIME2 = NULL
AS
BEGIN
    SELECT 
        PatientId, DocumentType, DocumentNumber, 
        FirstName, LastName, BirthDate, 
        PhoneNumber, Email, CreatedAt, RowVersion
    FROM Patients p
    WHERE 
        (@Name IS NULL OR (p.FirstName LIKE '%' + @Name + '%' OR p.LastName LIKE '%' + @Name + '%'))
        AND (@DocumentNumber IS NULL OR p.DocumentNumber = @DocumentNumber)
        AND (@CreatedFrom IS NULL OR p.CreatedAt >= @CreatedFrom)
        AND (@CreatedTo IS NULL OR p.CreatedAt <= @CreatedTo)
    ORDER BY p.CreatedAt DESC;
END
```

**Ventajas:**
- Mejor rendimiento para consultas complejas
- Ejecución optimizada en el servidor
- Reutilizable desde múltiples servicios

### 3. AutoMapper

Mapeo automático entre entidades y DTOs:

```csharp
// Patient → PatientDto
// CreatePatientDto → Patient
// UpdatePatientDto → Patient
```

**Configuración en `PatientProfile.cs`:**
```csharp
CreateMap<Patient, PatientDto>();
CreateMap<CreatePatientDto, Patient>();
CreateMap<UpdatePatientDto, Patient>()
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.PatientId, opt => opt.Ignore());
```

### 4. FluentValidation

Validaciones declarativas y reutilizables:

**CreatePatientValidator:**
- DocumentType: Requerido, máx 10 caracteres
- DocumentNumber: Requerido, máx 20 caracteres
- FirstName: Requerido, máx 80 caracteres
- LastName: Requerido, máx 80 caracteres
- BirthDate: Requerido, no futura
- Email: Formato válido si se proporciona

**UpdatePatientValidator:**
- Mismas reglas que Create
- Incluye validación de `RowVersion` para concurrencia

### 5. Middleware de Manejo de Errores

**ErrorHandlingMiddleware** intercepta todas las excepciones y retorna respuestas HTTP consistentes:

```csharp
try
{
    await _next(context);
}
catch (ValidationFailedException ex)
{
    await HandleValidationExceptionAsync(context, ex); // 400 Bad Request
}
catch (DuplicatePatientException ex)
{
    await HandleDuplicateExceptionAsync(context, ex); // 409 Conflict
}
catch (ConcurrencyException ex)
{
    await HandleConcurrencyExceptionAsync(context, ex); // 409 Conflict
}
catch (Exception ex)
{
    await HandleExceptionAsync(context, ex); // 500 Internal Server Error
}
```

**Respuesta de error estándar:**
```json
{
  "title": "Validation Error",
  "status": 400,
  "detail": "Los datos proporcionados no son válidos",
  "errors": {
    "BirthDate": ["La fecha de nacimiento no puede ser futura"],
    "Email": ["El correo electrónico no es válido"]
  }
}
```

### 6. Control de Concurrencia Optimista

Uso de `RowVersion` (timestamp de SQL Server):

- Se devuelve en cada respuesta
- Se valida en cada actualización
- Evita sobrescrituras accidentales
- Genera error 409 Conflict si hay conflicto

**Flujo:**
1. Cliente obtiene paciente con `rowVersion: "AAAAAAAAB9E="`
2. Otro usuario modifica el mismo paciente
3. Cliente intenta actualizar con `rowVersion` desactualizado
4. API retorna `409 Conflict`
5. Cliente debe recargar datos frescos

### 7. Sistema de Auditoría

Tabla `AuditLog` registra automáticamente todas las operaciones:

| Campo | Descripción |
|-------|-------------|
| AuditLogId | ID único del registro |
| Entity | Nombre de la entidad (ej: "Patient") |
| EntityId | ID del registro afectado |
| Action | Acción realizada: "Create", "Update", "Delete" |
| Username | Usuario que realizó la acción |
| CreatedAt | Fecha y hora UTC de la acción |
| Changes | JSON con los cambios (opcional) |

**Ejemplo de registro:**
```json
{
  "auditLogId": 1,
  "entity": "Patient",
  "entityId": 5,
  "action": "Update",
  "username": "system",
  "createdAt": "2025-01-20T10:30:00Z",
  "changes": "{\"FirstName\":{\"Old\":\"Juan\",\"New\":\"Juan Carlos\"}}"
}
```

### 8. Paginación y Filtrado

**Características:**
- Paginación eficiente con `Skip()` y `Take()`
- Límite máximo de 100 elementos por página
- Filtros combinables
- Ordenamiento dinámico
- Metadata de paginación en la respuesta

**Ejemplo de respuesta paginada:**
```json
{
  "items": [...],
  "totalCount": 150,
  "page": 2,
  "pageSize": 10,
  "totalPages": 15
}
```

### 9. Exportación a CSV

**Características:**
- Generación eficiente en memoria
- Escape de caracteres especiales (comas, comillas)
- Codificación UTF-8
- Nombre de archivo con timestamp
- Usa procedimiento almacenado para rendimiento

**Campos exportados:**
- DocumentType
- DocumentNumber
- FirstName
- LastName
- Email
- BirthDate
- CreatedAt

---

## 🎯 Decisiones Técnicas

### PUT vs PATCH

**Se eligió PUT (actualización completa)** por las siguientes razones:

#### Ventajas de PUT:
1. **Simplicidad**: El cliente envía el objeto completo, más fácil de entender e implementar
2. **Idempotencia clara**: Múltiples peticiones idénticas producen el mismo resultado
3. **Sin dependencias adicionales**: No requiere paquetes como `Microsoft.AspNetCore.JsonPatch`
4. **Validación completa**: FluentValidation valida el objeto entero
5. **Menos propenso a errores**: No hay riesgo de operaciones parciales inconsistentes

#### Desventajas de PATCH (JSON Patch):
1. **Complejidad**: Requiere entender el formato JSON Patch (RFC 6902)
2. **Validación compleja**: Difícil validar operaciones parciales
3. **Más verboso**: El cliente debe construir arrays de operaciones
4. **Documentación**: Swagger no documenta bien JSON Patch

#### Ejemplo comparativo:

**PUT (Implementado):**
```json
PUT /api/patients/1
{
  "documentType": "CC",
  "documentNumber": "123456789",
  "firstName": "Juan Carlos",  // Actualizado
  "lastName": "Perez",
  "birthDate": "1990-05-15",
  "email": "nuevo@email.com",  // Actualizado
  "phoneNumber": "3001234567",
  "rowVersion": "AAAAAAAAB9E="
}
```

**PATCH (No implementado):**
```json
PATCH /api/patients/1
[
  { "op": "replace", "path": "/firstName", "value": "Juan Carlos" },
  { "op": "replace", "path": "/email", "value": "nuevo@email.com" }
]
```

#### Conclusión:
Para esta aplicación de gestión de pacientes, **PUT es más apropiado** porque:
- Los formularios de edición típicamente cargan todos los campos
- El overhead de enviar el objeto completo es mínimo (pocos campos)
- La claridad y simplicidad son prioritarias
- No hay necesidad de actualizaciones parciales complejas

---

### Arquitectura en Capas

Se eligió una **arquitectura en N capas** inspirada en Clean Architecture:

**Ventajas:**
- **Separación de responsabilidades**: Cada capa tiene un propósito claro
- **Testabilidad**: Fácil hacer mocking de dependencias
- **Mantenibilidad**: Cambios en una capa no afectan otras
- **Escalabilidad**: Fácil agregar nuevas funcionalidades
- **Independencia de frameworks**: Lógica de negocio no depende de EF Core o ASP.NET

---

### Entity Framework Core vs ADO.NET

Se eligió **EF Core** con procedimientos almacenados para casos específicos:

**EF Core para:**
- CRUD básico (más productivo)
- Migraciones automáticas
- Tipado fuerte
- LINQ para consultas complejas

**Procedimientos Almacenados para:**
- Consultas de exportación (mejor rendimiento)
- Operaciones complejas con múltiples tablas
- Lógica de negocio en la BD (cuando aplica)

---

## 🧪 Pruebas

### Ejecución de Pruebas

**Desde Visual Studio:**
1. Abrir **Test Explorer** (`Ctrl+E, T`)
2. Clic en **Run All**

**Desde Terminal:**
```bash
# Ejecutar todas las pruebas
dotnet test

# Con output detallado
dotnet test --logger "console;verbosity=detailed"

# Solo el proyecto de pruebas
dotnet test Patients.Tests/Patients.Tests.csproj
```

### Cobertura de Pruebas

**Total: 32 pruebas unitarias**

| Endpoint | Pruebas | Cobertura |
|----------|---------|-----------|
| `POST /api/patients` | 9 | Casos exitosos, validaciones, duplicados |
| `GET /api/patients` | 8 | Paginación, filtros, ordenamiento |
| `GET /api/patients/{id}` | - | Pendiente |
| `PUT /api/patients/{id}` | - | Pendiente |
| `DELETE /api/patients/{id}` | - | Pendiente |
| `GET /api/patients/export` | - | Pendiente |

### Tipos de Pruebas Implementadas

1. **Pruebas de Casos Exitosos**
   - Crear paciente con datos válidos
   - Listar pacientes sin filtros
   - Paginación correcta

2. **Pruebas de Validación**
   - Fecha de nacimiento futura
   - Email inválido
   - Campos requeridos

3. **Pruebas de Casos Edge**
   - Paginación con parámetros inválidos
   - Filtros múltiples combinados
   - Ordenamiento en ambas direcciones

4. **Pruebas de Excepciones**
   - Paciente duplicado (409 Conflict)
   - Validación fallida (400 Bad Request)

### Ejemplo de Prueba

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
        LastName = "Perez",
        BirthDate = new DateTime(1990, 1, 1)
    };

    var expectedPatient = new PatientDto
    {
        PatientId = 1,
        DocumentType = "CC",
        DocumentNumber = "123456789",
        FirstName = "Juan",
        LastName = "Perez",
        BirthDate = new DateTime(1990, 1, 1),
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
    createdResult!.Value.Should().BeEquivalentTo(expectedPatient);
}
```

---

## 🗄️ Base de Datos

### Modelo de Datos

#### Tabla: Patients

| Campo | Tipo | Restricciones |
|-------|------|---------------|
| PatientId | INT | PK, Identity |
| DocumentType | NVARCHAR(10) | NOT NULL |
| DocumentNumber | NVARCHAR(20) | NOT NULL |
| FirstName | NVARCHAR(80) | NOT NULL |
| LastName | NVARCHAR(80) | NOT NULL |
| BirthDate | DATE | NOT NULL |
| PhoneNumber | NVARCHAR(20) | NULL |
| Email | NVARCHAR(120) | NULL |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() |
| RowVersion | ROWVERSION | Timestamp |

**Índices:**
- PK en `PatientId`
- Unique en combinación (`DocumentType`, `DocumentNumber`)

#### Tabla: AuditLogs

| Campo | Tipo | Restricciones |
|-------|------|---------------|
| AuditLogId | BIGINT | PK, Identity |
| Entity | NVARCHAR(100) | NOT NULL |
| EntityId | INT | NOT NULL |
| Action | NVARCHAR(50) | NOT NULL |
| Username | NVARCHAR(50) | NOT NULL |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT SYSUTCDATETIME() |
| Changes | NVARCHAR(MAX) | NULL |

**Índices:**
- PK en `AuditLogId`
- Índice en (`Entity`, `EntityId`) para búsquedas rápidas

### Migraciones

Las migraciones se aplican automáticamente al iniciar la aplicación. Si necesitas aplicarlas manualmente:

```bash
# Ver migraciones pendientes
dotnet ef migrations list --project Patients.Infrastructure --startup-project Patients.Api

# Aplicar migraciones
dotnet ef database update --project Patients.Infrastructure --startup-project Patients.Api

# Crear nueva migración
dotnet ef migrations add NombreMigracion --project Patients.Infrastructure --startup-project Patients.Api

# Revertir última migración
dotnet ef database update PreviousMigration --project Patients.Infrastructure --startup-project Patients.Api
```

### Script de Base de Datos

Para generar un script SQL de toda la base de datos:

```bash
dotnet ef migrations script --project Patients.Infrastructure --startup-project Patients.Api --output database.sql
```

---

## 🐳 Docker

### Configuración de Docker

El proyecto incluye configuración completa para Docker con:

- **SQL Server 2022** en contenedor
- **API en .NET 8** containerizada
- **Docker Compose** para orquestación
- **Volúmenes persistentes** para la base de datos
- **Health checks** para asegurar que SQL Server esté listo

### Estructura de Docker

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"
    environment:
      - SA_PASSWORD=Password123!
    
  patients-api:
    build: .
    ports:
      - "5000:8080"
    depends_on:
      - sqlserver
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
```

### Comandos Docker

```bash
# Construir e iniciar
docker-compose up --build

# Ejecutar en segundo plano
docker-compose up -d

# Ver logs
docker-compose logs -f patients-api

# Detener servicios
docker-compose down

# Limpiar todo (incluyendo volúmenes)
docker-compose down -v

# Reiniciar un servicio
docker-compose restart patients-api
```

### Acceder a SQL Server en Docker

```bash
# Desde el host
sqlcmd -S localhost,1433 -U sa -P Password123!

# Desde otro contenedor
docker exec -it patients-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Password123!
```

### Credenciales de SQL Server en Docker

- **Host**: localhost
- **Puerto**: 1433
- **Usuario**: sa
- **Contraseña**: Password123!
- **Base de Datos**: PatientsDb

---

## 📚 Documentación Adicional

### Swagger UI

La API incluye documentación interactiva generada automáticamente con Swagger:

- **URL**: http://localhost:5000/swagger
- **Características**:
  - Explorar todos los endpoints
  - Probar peticiones directamente desde el navegador
  - Ver modelos de datos
  - Ejemplos de request/response

### Colección de Postman

Se puede importar la especificación OpenAPI desde Swagger:

1. Abrir Postman
2. Import → Link
3. Pegar: `http://localhost:5000/swagger/v1/swagger.json`

---

## 📦 Estructura de Respuestas

### Respuestas Exitosas

```json
// 200 OK
{
  "patientId": 1,
  "firstName": "Juan",
  ...
}

// 201 Created
Location: /api/patients/1
{
  "patientId": 1,
  ...
}

// 204 No Content
(sin body)
```

### Respuestas de Error

```json
// 400 Bad Request
{
  "title": "Validation Error",
  "status": 400,
  "detail": "Los datos proporcionados no son válidos",
  "errors": {
    "BirthDate": ["La fecha de nacimiento no puede ser futura"]
  }
}

// 404 Not Found
{
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no existe"
}

// 409 Conflict
{
  "title": "Conflict",
  "status": 409,
  "detail": "Ya existe un paciente con el mismo tipo y número de documento"
}

// 500 Internal Server Error
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Ocurrió un error inesperado. Por favor contacte al administrador."
}
```

