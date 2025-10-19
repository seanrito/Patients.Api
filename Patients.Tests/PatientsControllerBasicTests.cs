using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Patients.Api.Controllers;
using Patients.Application.Dtos;
using Patients.Application.DTOs;
using Patients.Application.Interfaces;

namespace Patients.Tests;

public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();
        _controller = new PatientsController(_mockPatientService.Object);
    }

    #region Create Endpoint Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreatePatientDto
        {
            DocumentType = "CC",
            DocumentNumber = "123456789",
            FirstName = "Juan",
            LastName = "Pérez",
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
            LastName = "Pérez",
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

    [Fact]
    public async Task Create_WithValidData_CallsServiceOnce()
    {
        // Arrange
        var createDto = new CreatePatientDto
        {
            DocumentType = "TI",
            DocumentNumber = "987654321",
            FirstName = "María",
            LastName = "González",
            BirthDate = new DateTime(2005, 5, 15),
            Email = "maria.gonzalez@example.com"
        };

        var expectedPatient = new PatientDto
        {
            PatientId = 2,
            DocumentType = "TI",
            DocumentNumber = "987654321",
            FirstName = "María",
            LastName = "González",
            BirthDate = new DateTime(2005, 5, 15),
            Email = "maria.gonzalez@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _mockPatientService
            .Setup(s => s.CreateAsync(It.IsAny<CreatePatientDto>()))
            .ReturnsAsync(expectedPatient);

        // Act
        await _controller.Create(createDto);

        // Assert
        _mockPatientService.Verify(
            s => s.CreateAsync(It.Is<CreatePatientDto>(dto =>
                dto.DocumentType == createDto.DocumentType &&
                dto.DocumentNumber == createDto.DocumentNumber &&
                dto.FirstName == createDto.FirstName &&
                dto.LastName == createDto.LastName
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Create_ReturnsCorrectPatientId()
    {
        // Arrange
        var createDto = new CreatePatientDto
        {
            DocumentType = "CE",
            DocumentNumber = "CE123456",
            FirstName = "Carlos",
            LastName = "Rodríguez",
            BirthDate = new DateTime(1985, 8, 20),
            PhoneNumber = "3109876543"
        };

        var expectedPatientId = 42;
        var expectedPatient = new PatientDto
        {
            PatientId = expectedPatientId,
            DocumentType = "CE",
            DocumentNumber = "CE123456",
            FirstName = "Carlos",
            LastName = "Rodríguez",
            BirthDate = new DateTime(1985, 8, 20),
            PhoneNumber = "3109876543",
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
        createdResult!.RouteValues!["id"].Should().Be(expectedPatientId);
        
        var returnedPatient = createdResult.Value as PatientDto;
        returnedPatient.Should().NotBeNull();
        returnedPatient!.PatientId.Should().Be(expectedPatientId);
    }

    [Fact]
    public async Task Create_WithMinimalData_ReturnsCreatedResult()
    {
        // Arrange - Solo campos obligatorios
        var createDto = new CreatePatientDto
        {
            DocumentType = "CC",
            DocumentNumber = "111222333",
            FirstName = "Pedro",
            LastName = "Martínez",
            BirthDate = new DateTime(1995, 3, 10)
        };

        var expectedPatient = new PatientDto
        {
            PatientId = 3,
            DocumentType = "CC",
            DocumentNumber = "111222333",
            FirstName = "Pedro",
            LastName = "Martínez",
            BirthDate = new DateTime(1995, 3, 10),
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
        var returnedPatient = createdResult!.Value as PatientDto;
        returnedPatient.Should().NotBeNull();
        returnedPatient!.Email.Should().BeNull();
        returnedPatient.PhoneNumber.Should().BeNull();
    }

    #endregion

    #region GetAll Endpoint Tests

    [Fact]
    public async Task GetAll_WithoutFilters_ReturnsOkResultWithPagedData()
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
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
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new PatientDto
                {
                    PatientId = 2,
                    DocumentType = "TI",
                    DocumentNumber = "987654321",
                    FirstName = "María",
                    LastName = "González",
                    BirthDate = new DateTime(2005, 5, 15),
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            },
            TotalCount = 2,
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
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
    }

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

    [Fact]
    public async Task GetAll_WithPaginationParameters_ReturnsCorrectPage()
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
            Page = 2,
            PageSize = 5
        };

        var expectedResult = new PagedResultDto<PatientDto>
        {
            Items = new List<PatientDto>
            {
                new PatientDto
                {
                    PatientId = 6,
                    DocumentType = "CC",
                    DocumentNumber = "666777888",
                    FirstName = "Paciente",
                    LastName = "Seis",
                    BirthDate = new DateTime(1992, 6, 6),
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 15,
            Page = 2,
            PageSize = 5
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
        pagedResult!.Page.Should().Be(2);
        pagedResult.PageSize.Should().Be(5);
        pagedResult.TotalCount.Should().Be(15);
        pagedResult.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAll_WithSortingParameters_CallsServiceWithCorrectSort()
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = "CreatedAt",
            SortDir = "desc"
        };

        var expectedResult = new PagedResultDto<PatientDto>
        {
            Items = new List<PatientDto>
            {
                new PatientDto
                {
                    PatientId = 2,
                    DocumentType = "TI",
                    DocumentNumber = "987654321",
                    FirstName = "María",
                    LastName = "González",
                    BirthDate = new DateTime(2005, 5, 15),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new PatientDto
                {
                    PatientId = 1,
                    DocumentType = "CC",
                    DocumentNumber = "123456789",
                    FirstName = "Juan",
                    LastName = "Pérez",
                    BirthDate = new DateTime(1990, 1, 1),
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            },
            TotalCount = 2,
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
                q.SortBy == "CreatedAt" &&
                q.SortDir == "desc"
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAll_WithMultipleFilters_CallsServiceWithAllFilters()
    {
        // Arrange
        var createdFrom = new DateTime(2025, 1, 1);
        var createdTo = new DateTime(2025, 12, 31);
        
        var query = new PatientQueryParamsDto
        {
            Name = "Sebastián",
            DocumentNumber = "123456789",
            CreatedFrom = createdFrom,
            CreatedTo = createdTo,
            Page = 1,
            PageSize = 20,
            SortBy = "FirstName",
            SortDir = "asc"
        };

        var expectedResult = new PagedResultDto<PatientDto>
        {
            Items = new List<PatientDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20
        };

        _mockPatientService
            .Setup(s => s.GetAllAsync(It.IsAny<PatientQueryParamsDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _controller.GetAll(query);

        // Assert
        _mockPatientService.Verify(
            s => s.GetAllAsync(It.Is<PatientQueryParamsDto>(q =>
                q.Name == "Sebastián" &&
                q.DocumentNumber == "123456789" &&
                q.CreatedFrom == createdFrom &&
                q.CreatedTo == createdTo &&
                q.Page == 1 &&
                q.PageSize == 20 &&
                q.SortBy == "FirstName" &&
                q.SortDir == "asc"
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAll_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
            Name = "NoExiste",
            Page = 1,
            PageSize = 10
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
        var okResult = result.Result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedResultDto<PatientDto>;
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().BeEmpty();
        pagedResult.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ReturnsCorrectTotalPagesCalculation()
    {
        // Arrange
        var query = new PatientQueryParamsDto
        {
            Page = 1,
            PageSize = 10
        };

        var expectedResult = new PagedResultDto<PatientDto>
        {
            Items = new List<PatientDto>(),
            TotalCount = 25,
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
        pagedResult!.TotalPages.Should().Be(3); // 25 / 10 = 3 páginas
    }

    #endregion
}
