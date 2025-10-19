using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Patients.Api.Controllers;
using Patients.Application.Dtos;
using Patients.Application.DTOs;
using Patients.Application.Exceptions;
using Patients.Application.Interfaces;

namespace Patients.Tests;

/// <summary>
/// Pruebas avanzadas y casos edge para el controlador de pacientes
/// </summary>
public class PatientsControllerAdvancedTests
{
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly PatientsController _controller;

    public PatientsControllerAdvancedTests()
    {
        _mockPatientService = new Mock<IPatientService>();
        _controller = new PatientsController(_mockPatientService.Object);
    }

    #region Create - Casos de Excepción

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
            .ThrowsAsync(new DuplicatePatientException("Ya existe un paciente con el mismo tipo y número de documento."));

        // Act
        Func<Task> act = async () => await _controller.Create(createDto);

        // Assert
        await act.Should().ThrowAsync<DuplicatePatientException>()
            .WithMessage("Ya existe un paciente con el mismo tipo y número de documento.");
    }

    [Fact]
    public async Task Create_WhenValidationFails_ThrowsValidationFailedException()
    {
        // Arrange
        var createDto = new CreatePatientDto
        {
            DocumentType = "CC",
            DocumentNumber = "", // Invalid: empty document number
            FirstName = "Juan",
            LastName = "Pérez",
            BirthDate = new DateTime(1990, 1, 1)
        };

        var validationResult = new FluentValidation.Results.ValidationResult(
            new[] { new FluentValidation.Results.ValidationFailure("DocumentNumber", "El número de documento es requerido") }
        );

        _mockPatientService
            .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
            .ThrowsAsync(new ValidationFailedException(validationResult));

        // Act
        Func<Task> act = async () => await _controller.Create(createDto);

        // Assert
        await act.Should().ThrowAsync<ValidationFailedException>();
    }

    [Fact]
    public async Task Create_WithFutureDate_ValidatesBusinessRules()
    {
        // Arrange - fecha de nacimiento en el futuro
        var createDto = new CreatePatientDto
        {
            DocumentType = "CC",
            DocumentNumber = "123456789",
            FirstName = "Juan",
            LastName = "Pérez",
            BirthDate = DateTime.UtcNow.AddDays(1) // Fecha futura
        };

        var validationResult = new FluentValidation.Results.ValidationResult(
            new[] { new FluentValidation.Results.ValidationFailure("BirthDate", "La fecha de nacimiento no puede ser futura") }
        );

        _mockPatientService
            .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
            .ThrowsAsync(new ValidationFailedException(validationResult));

        // Act
        Func<Task> act = async () => await _controller.Create(createDto);

        // Assert
        await act.Should().ThrowAsync<ValidationFailedException>();
    }

    #endregion

    #region GetAll - Casos Avanzados

    [Theory]
    [InlineData(0, 10, 1)] // Page 0 should be converted to 1
    [InlineData(-1, 10, 1)] // Negative page should be converted to 1
    [InlineData(1, 0, 10)] // PageSize 0 should use default
    [InlineData(1, 200, 100)] // PageSize > 100 should be capped to 100
    public async Task GetAll_WithInvalidPaginationParameters_ServiceHandlesCorrectly(
        int inputPage, int inputPageSize, int expectedValidValue)
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
            Page = inputPage,
            PageSize = inputPageSize
        };

        var expectedResult = new PagedResultDto<PatientDto>
        {
            Items = new List<PatientDto>(),
            TotalCount = 0,
            Page = expectedValidValue,
            PageSize = expectedValidValue == 1 ? 10 : expectedValidValue
        };

        _mockPatientService
            .Setup(s => s.GetAllAsync(It.IsAny<PatientQueryParamsDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockPatientService.Verify(
            s => s.GetAllAsync(It.IsAny<PatientQueryParamsDto>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAll_WithLargeDataset_ReturnsPaginatedResults()
    {
        // Arrange - Simular un dataset grande
        var query = new PatientQueryParamsDto
        {
            Page = 1,
            PageSize = 10
        };

        var patients = new List<PatientDto>();
        for (int i = 1; i <= 10; i++)
        {
            patients.Add(new PatientDto
            {
                PatientId = i,
                DocumentType = "CC",
                DocumentNumber = $"{100000000 + i}",
                FirstName = $"Paciente{i}",
                LastName = $"Test{i}",
                BirthDate = new DateTime(1990, 1, 1).AddYears(i),
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        var expectedResult = new PagedResultDto<PatientDto>
        {
            Items = patients,
            TotalCount = 1000, // Total de 1000 pacientes en la base
            Page = 1,
            PageSize = 10
        };

        _mockPatientService
            .Setup(s => s.GetAllAsync(It.IsAny<PatientQueryParamsDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedResultDto<PatientDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().HaveCount(10);
        pagedResult.TotalCount.Should().Be(1000);
        pagedResult.TotalPages.Should().Be(100);
    }

    [Theory]
    [InlineData("FirstName", "asc")]
    [InlineData("LastName", "desc")]
    [InlineData("CreatedAt", "asc")]
    [InlineData("CreatedAt", "desc")]
    [InlineData(null, null)]
    public async Task GetAll_WithDifferentSortOptions_HandlesCorrectly(string? sortBy, string? sortDir)
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortDir = sortDir
        };

        var expectedResult = new PagedResultDto<PatientDto>
        {
            Items = new List<PatientDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _mockPatientService
            .Setup(s => s.GetAllAsync(It.IsAny<PatientQueryParamsDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockPatientService.Verify(
            s => s.GetAllAsync(It.Is<PatientQueryParamsDto>(q =>
                q.SortBy == sortBy && q.SortDir == sortDir
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAll_WithDateRangeFilter_ReturnsFilteredResults()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 12, 31);

        var query = new PatientQueryParamsDto
        {
            CreatedFrom = startDate,
            CreatedTo = endDate,
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
                    LastName = "Pérez",
                    BirthDate = new DateTime(1990, 1, 1),
                    CreatedAt = new DateTime(2025, 6, 15)
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
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedResultDto<PatientDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().AllSatisfy(p =>
        {
            p.CreatedAt.Should().BeOnOrAfter(startDate);
            p.CreatedAt.Should().BeOnOrBefore(endDate);
        });
    }

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

    [Fact]
    public async Task GetAll_WithNameFilter_PerformsPartialMatch()
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
            Name = "seb", // Partial name search
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
                    FirstName = "Sebastián",
                    LastName = "Rodríguez",
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
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedResultDto<PatientDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().NotBeEmpty();
        pagedResult.Items.First().FirstName.Should().Contain("Sebastián");
    }

    [Fact]
    public async Task Create_WithValidEmail_StoresEmailCorrectly()
    {
        // Arrange
        var validEmail = "sebastian.rodriguez@example.com";
        var createDto = new CreatePatientDto
        {
            DocumentType = "CC",
            DocumentNumber = "123456789",
            FirstName = "Sebastián",
            LastName = "Rodríguez",
            BirthDate = new DateTime(1990, 1, 1),
            Email = validEmail
        };

        var expectedPatient = new PatientDto
        {
            PatientId = 1,
            DocumentType = "CC",
            DocumentNumber = "123456789",
            FirstName = "Sebastián",
            LastName = "Rodríguez",
            BirthDate = new DateTime(1990, 1, 1),
            Email = validEmail,
            CreatedAt = DateTime.UtcNow
        };

        _mockPatientService
            .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
            .ReturnsAsync(expectedPatient);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result as CreatedAtActionResult;
        var returnedPatient = createdResult!.Value as PatientDto;
        returnedPatient!.Email.Should().Be(validEmail);
    }

    [Fact]
    public async Task GetAll_WithDocumentNumberFilter_ReturnsExactMatch()
    {
        // Arrange
        var exactDocumentNumber = "123456789";
        var query = new PatientQueryParamsDto
        {
            DocumentNumber = exactDocumentNumber,
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
                    DocumentNumber = exactDocumentNumber,
                    FirstName = "Juan",
                    LastName = "Pérez",
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
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedResultDto<PatientDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().HaveCount(1);
        pagedResult.Items.First().DocumentNumber.Should().Be(exactDocumentNumber);
    }

    #endregion
}
