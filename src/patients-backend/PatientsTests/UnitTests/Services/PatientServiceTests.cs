using Moq;
using Microsoft.Extensions.Logging;
using Patients.Domain.Entities;
using Patients.Domain.Services;
using Patients.Domain.Services.Interfaces;
using Patients.DTOs;
using Patients.Infrastructure.Repositories.Interfaces;

namespace PatientsTests.UnitTests.Services;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _mockPatientRepository;
    private readonly Mock<IAddressRepository> _mockAddressRepository;
    private readonly Mock<ILogger<PatientService>> _mockLogger;
    private readonly PatientService _patientService;

    public PatientServiceTests()
    {
        _mockPatientRepository = new Mock<IPatientRepository>();
        _mockAddressRepository = new Mock<IAddressRepository>();
        _mockLogger = new Mock<ILogger<PatientService>>();
        
        _patientService = new PatientService(
            _mockPatientRepository.Object,
            _mockAddressRepository.Object,
            _mockLogger.Object);
    }

    #region GetAllPatientsAsync Tests

    [Fact]
    public async Task GetAllPatientsAsync_WhenPatientsExist_ReturnsPatientDtos()
    {
        // Arrange
        var expectedPatients = CreateTestPatients();
        _mockPatientRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(expectedPatients);

        // Act
        var result = await _patientService.GetAllPatientsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var patientsList = result.ToList();
        Assert.Equal("John", patientsList[0].FirstName);
        Assert.Equal("Doe", patientsList[0].LastName);
        Assert.Equal("Jane", patientsList[1].FirstName);
        Assert.Equal("Smith", patientsList[1].LastName);
        
        _mockPatientRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllPatientsAsync_WhenRepositoryThrowsException_ReturnsNull()
    {
        // Arrange
        _mockPatientRepository.Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _patientService.GetAllPatientsAsync();

        // Assert
        Assert.Null(result);
        _mockPatientRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllPatientsAsync_WhenNoPatientsExist_ReturnsEmptyList()
    {
        // Arrange
        _mockPatientRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Patient>());

        // Act
        var result = await _patientService.GetAllPatientsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockPatientRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetPatientByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(42)]
    public async Task GetPatientByIdAsync_WhenPatientExists_ReturnsPatientDto(int patientId)
    {
        // Arrange
        var expectedPatient = CreateTestPatient(patientId);
        _mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId))
            .ReturnsAsync(expectedPatient);

        // Act
        var result = await _patientService.GetPatientByIdAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patientId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.NotNull(result.Address);
        Assert.Equal("123 Main St", result.Address.Street);
        
        _mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetPatientByIdAsync_WhenPatientDoesNotExist_ReturnsNull(int patientId)
    {
        // Arrange
        _mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId))
            .ReturnsAsync((Patient?)null);

        // Act
        var result = await _patientService.GetPatientByIdAsync(patientId);

        // Assert
        Assert.Null(result);
        _mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId), Times.Once);
    }

    [Fact]
    public async Task GetPatientByIdAsync_WhenRepositoryThrowsException_ReturnsNull()
    {
        // Arrange
        var patientId = 1;
        _mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _patientService.GetPatientByIdAsync(patientId);

        // Assert
        Assert.Null(result);
        _mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId), Times.Once);
    }

    #endregion

    #region CreatePatientAsync Tests

    [Fact]
    public async Task CreatePatientAsync_WhenValidPatientWithAddress_ReturnsPatientId()
    {
        // Arrange
        var patientCreateDto = CreateTestPatientCreateDto();
        var savedAddress = CreateTestAddress(1);
        var savedPatient = CreateTestPatient(1);

        _mockAddressRepository.Setup(repo => repo.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync(savedAddress);
        _mockPatientRepository.Setup(repo => repo.AddAsync(It.IsAny<Patient>()))
            .ReturnsAsync(savedPatient);

        // Act
        var result = await _patientService.CreatePatientAsync(patientCreateDto);

        // Assert
        Assert.Equal(1, result);
        
        _mockAddressRepository.Verify(repo => repo.AddAsync(It.IsAny<Address>()), Times.Once);
        _mockPatientRepository.Verify(repo => repo.AddAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Fact]
    public async Task CreatePatientAsync_WhenValidPatientWithoutAddress_ReturnsPatientId()
    {
        // Arrange
        var patientCreateDto = CreateTestPatientCreateDto();
        patientCreateDto.Address = null;
        var savedPatient = CreateTestPatient(1);

        _mockPatientRepository.Setup(repo => repo.AddAsync(It.IsAny<Patient>()))
            .ReturnsAsync(savedPatient);

        // Act
        var result = await _patientService.CreatePatientAsync(patientCreateDto);

        // Assert
        Assert.Equal(1, result);
        
        _mockAddressRepository.Verify(repo => repo.AddAsync(It.IsAny<Address>()), Times.Never);
        _mockPatientRepository.Verify(repo => repo.AddAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Theory]
    [InlineData("Repository error")]
    [InlineData("Database connection failed")]
    public async Task CreatePatientAsync_WhenRepositoryThrowsException_ReturnsZero(string errorMessage)
    {
        // Arrange
        var patientCreateDto = CreateTestPatientCreateDto();
        _mockAddressRepository.Setup(repo => repo.AddAsync(It.IsAny<Address>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _patientService.CreatePatientAsync(patientCreateDto);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region UpdatePatientAsync Tests

    [Fact]
    public async Task UpdatePatientAsync_WhenPatientExistsWithExistingAddress_UpdatesBoth()
    {
        // Arrange
        var patientId = 1;
        var patientUpdateDto = CreateTestPatientUpdateDto();
        var existingPatient = CreateTestPatient(patientId);

        _mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId))
            .ReturnsAsync(existingPatient);

        // Act
        await _patientService.UpdatePatientAsync(patientId, patientUpdateDto);

        // Assert
        _mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId), Times.Once);
        _mockAddressRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Address>()), Times.Once);
        _mockPatientRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePatientAsync_WhenPatientExistsWithoutAddress_CreatesNewAddress()
    {
        // Arrange
        var patientId = 1;
        var patientUpdateDto = CreateTestPatientUpdateDto();
        var existingPatient = CreateTestPatient(patientId);
        existingPatient.PatientAddress = null;
        existingPatient.AddressId = null;
        
        var newAddress = CreateTestAddress(2);

        _mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId))
            .ReturnsAsync(existingPatient);
        _mockAddressRepository.Setup(repo => repo.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync(newAddress);

        // Act
        await _patientService.UpdatePatientAsync(patientId, patientUpdateDto);

        // Assert
        _mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId), Times.Once);
        _mockAddressRepository.Verify(repo => repo.AddAsync(It.IsAny<Address>()), Times.Once);
        _mockPatientRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePatientAsync_WhenPatientExistsWithoutAddressInDto_UpdatesPatientOnly()
    {
        // Arrange
        var patientId = 1;
        var patientUpdateDto = CreateTestPatientUpdateDto();
        patientUpdateDto.Address = null;
        var existingPatient = CreateTestPatient(patientId);

        _mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId))
            .ReturnsAsync(existingPatient);

        // Act
        await _patientService.UpdatePatientAsync(patientId, patientUpdateDto);

        // Assert
        _mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId), Times.Once);
        _mockAddressRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Address>()), Times.Never);
        _mockAddressRepository.Verify(repo => repo.AddAsync(It.IsAny<Address>()), Times.Never);
        _mockPatientRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Theory]
    [InlineData("Repository error")]
    [InlineData("Database timeout")]
    public async Task UpdatePatientAsync_WhenRepositoryThrowsException_LogsError(string errorMessage)
    {
        // Arrange
        var patientId = 1;
        var patientUpdateDto = CreateTestPatientUpdateDto();
        
        _mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId))
            .ThrowsAsync(new Exception(errorMessage));

        // Act & Assert - Should not throw
        await _patientService.UpdatePatientAsync(patientId, patientUpdateDto);
        
        _mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId), Times.Once);
    }

    #endregion

    #region DeletePatientAsync Tests

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(999, false)]
    public async Task DeletePatientAsync_WhenCalled_ReturnsRepositoryResult(int patientId, bool expectedResult)
    {
        // Arrange
        _mockPatientRepository.Setup(repo => repo.DeleteAsync(patientId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _patientService.DeletePatientAsync(patientId);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockPatientRepository.Verify(repo => repo.DeleteAsync(patientId), Times.Once);
    }

    #endregion

    #region Helper Methods

    private List<Patient> CreateTestPatients()
    {
        return new List<Patient>
        {
            new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                PhoneNumber = "123-456-7890",
                AddressId = 1,
                PatientAddress = new Address
                {
                    Id = 1,
                    Street = "123 Main St",
                    City = "New York",
                    PostalCode = "10001",
                    Country = "USA"
                }
            },
            new Patient
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Female",
                PhoneNumber = "987-654-3210",
                AddressId = 2,
                PatientAddress = new Address
                {
                    Id = 2,
                    Street = "456 Oak Ave",
                    City = "Los Angeles",
                    PostalCode = "90210",
                    Country = "USA"
                }
            }
        };
    }

    private Patient CreateTestPatient(int id)
    {
        return new Patient
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            PhoneNumber = "123-456-7890",
            AddressId = 1,
            PatientAddress = new Address
            {
                Id = 1,
                Street = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA"
            }
        };
    }

    private Address CreateTestAddress(int id)
    {
        return new Address
        {
            Id = id,
            Street = "123 Main St",
            City = "New York",
            PostalCode = "10001",
            Country = "USA"
        };
    }

    private PatientCreateDto CreateTestPatientCreateDto()
    {
        return new PatientCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            PhoneNumber = "123-456-7890",
            Address = new AddressCreateDto
            {
                Street = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA"
            }
        };
    }

    private PatientUpdateDto CreateTestPatientUpdateDto()
    {
        return new PatientUpdateDto
        {
            FirstName = "John Updated",
            LastName = "Doe Updated",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            PhoneNumber = "123-456-7890",
            Address = new AddressCreateDto
            {
                Street = "456 Updated St",
                City = "Updated City",
                PostalCode = "12345",
                Country = "USA"
            }
        };
    }

    #endregion
}