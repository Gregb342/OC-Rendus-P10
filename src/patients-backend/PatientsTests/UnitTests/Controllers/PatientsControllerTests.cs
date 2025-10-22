using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Patients.Controllers;
using Patients.Domain.Entities;
using Patients.Domain.Services.Interfaces;
using Patients.DTOs;
using Patients.Infrastructure.Repositories.Interfaces;

namespace PatientsTests.UnitTests.Controllers;

public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly Mock<IAddressRepository> _mockAddressRepository;
    private readonly Mock<ILogger<PatientsController>> _mockLogger;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();
    _mockAddressRepository = new Mock<IAddressRepository>();
 _mockLogger = new Mock<ILogger<PatientsController>>();
 
     _controller = new PatientsController(
            _mockPatientService.Object,
   _mockAddressRepository.Object,
            _mockLogger.Object);

      // Setup HttpContext for authorization testing
        var claims = new[]
  {
 new Claim(ClaimTypes.Name, "testuser"),
          new Claim(ClaimTypes.NameIdentifier, "123")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
   var principal = new ClaimsPrincipal(identity);
        
_controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
      {
       User = principal
 }
   };

    _controller.HttpContext.Request.Headers["Authorization"] = "Bearer test-token";
    }

    #region GetPatients Tests

    [Fact]
    public async Task GetPatients_WhenPatientsExist_ReturnsOkWithPatients()
    {
        // Arrange
  var expectedPatients = CreateTestPatientDtos();
        _mockPatientService.Setup(service => service.GetAllPatientsAsync())
        .ReturnsAsync(expectedPatients);

        // Act
      var result = await _controller.GetPatients();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<PatientDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
  var returnedPatients = Assert.IsAssignableFrom<IEnumerable<PatientDto>>(okResult.Value);
        Assert.Equal(2, returnedPatients.Count());
     
        _mockPatientService.Verify(service => service.GetAllPatientsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPatients_WhenNoPatientsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
    _mockPatientService.Setup(service => service.GetAllPatientsAsync())
        .ReturnsAsync(new List<PatientDto>());

        // Act
        var result = await _controller.GetPatients();

        // Assert
     var actionResult = Assert.IsType<ActionResult<IEnumerable<PatientDto>>>(result);
var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPatients = Assert.IsAssignableFrom<IEnumerable<PatientDto>>(okResult.Value);
        Assert.Empty(returnedPatients);
        
   _mockPatientService.Verify(service => service.GetAllPatientsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPatients_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockPatientService.Setup(service => service.GetAllPatientsAsync())
       .ThrowsAsync(new Exception("Database connection failed"));

        // Act
     var result = await _controller.GetPatients();

// Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<PatientDto>>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
    Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("An error occurred while retrieving patients", statusResult.Value);
        
        _mockPatientService.Verify(service => service.GetAllPatientsAsync(), Times.Once);
    }

    #endregion

 #region GetPatient Tests

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(42)]
    public async Task GetPatient_WhenPatientExists_ReturnsOkWithPatient(int patientId)
    {
        // Arrange
    var expectedPatient = CreateTestPatient(patientId);
   _mockPatientService.Setup(service => service.GetPatientByIdAsync(patientId))
            .ReturnsAsync(expectedPatient);

        // Act
     var result = await _controller.GetPatient(patientId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PatientDto>>(result);
 var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
  var returnedPatient = Assert.IsType<PatientDto>(okResult.Value);
        Assert.Equal(patientId, returnedPatient.Id);
   Assert.Equal("John", returnedPatient.FirstName);
        
        _mockPatientService.Verify(service => service.GetPatientByIdAsync(patientId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetPatient_WhenPatientDoesNotExist_ReturnsNotFound(int patientId)
    {
     // Arrange
        _mockPatientService.Setup(service => service.GetPatientByIdAsync(patientId))
         .ReturnsAsync((Patient?)null);

        // Act
    var result = await _controller.GetPatient(patientId);

        // Assert
   var actionResult = Assert.IsType<ActionResult<PatientDto>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
   
        _mockPatientService.Verify(service => service.GetPatientByIdAsync(patientId), Times.Once);
    }

    [Fact]
    public async Task GetPatient_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var patientId = 1;
        _mockPatientService.Setup(service => service.GetPatientByIdAsync(patientId))
    .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPatient(patientId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PatientDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
  Assert.Equal("An error occurred while retrieving the patient", statusResult.Value);
     
        _mockPatientService.Verify(service => service.GetPatientByIdAsync(patientId), Times.Once);
    }

    #endregion

    #region CreatePatient Tests

    [Fact]
    public async Task CreatePatient_WhenValidPatient_ReturnsCreatedAtAction()
    {
        // Arrange
        var patientCreateDto = CreateTestPatientCreateDto();
        var savedAddress = CreateTestAddress(1);
  var createdPatient = CreateTestPatient(1);

        _mockAddressRepository.Setup(repo => repo.AddAsync(It.IsAny<Address>()))
   .ReturnsAsync(savedAddress);
        _mockPatientService.Setup(service => service.CreatePatientAsync(It.IsAny<Patient>()))
     .ReturnsAsync(createdPatient);

        // Act
      var result = await _controller.CreatePatient(patientCreateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PatientDto>>(result);
  var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(_controller.GetPatient), createdResult.ActionName);
        
    _mockPatientService.Verify(service => service.CreatePatientAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Theory]
    [InlineData("Repository error")]
    [InlineData("Database connection failed")]
    public async Task CreatePatient_WhenServiceThrowsException_ReturnsInternalServerError(string errorMessage)
    {
        // Arrange
        var patientCreateDto = CreateTestPatientCreateDto();
        _mockAddressRepository.Setup(repo => repo.AddAsync(It.IsAny<Address>()))
       .ThrowsAsync(new Exception(errorMessage));

        // Act
   var result = await _controller.CreatePatient(patientCreateDto);

      // Assert
        var actionResult = Assert.IsType<ActionResult<PatientDto>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
      Assert.Equal(500, statusResult.StatusCode);
  Assert.Equal("An error occurred while creating the patient", statusResult.Value);
    }

    #endregion

    #region UpdatePatient Tests

    [Fact]
    public async Task UpdatePatient_WhenValidUpdate_ReturnsNoContent()
    {
        // Arrange
        var patientId = 1;
        var patientUpdateDto = CreateTestPatientUpdateDto();
        var existingPatient = CreateTestPatient(patientId);

   _mockPatientService.Setup(service => service.GetPatientByIdAsync(patientId))
       .ReturnsAsync(existingPatient);

        // Act
        var result = await _controller.UpdatePatient(patientId, patientUpdateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        _mockPatientService.Verify(service => service.GetPatientByIdAsync(patientId), Times.Once);
        _mockPatientService.Verify(service => service.UpdatePatientAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Theory]
[InlineData("Repository error")]
    [InlineData("Patient not found")]
    public async Task UpdatePatient_WhenServiceThrowsException_ReturnsInternalServerError(string errorMessage)
    {
        // Arrange
        var patientId = 1;
        var patientUpdateDto = CreateTestPatientUpdateDto();
        _mockPatientService.Setup(service => service.GetPatientByIdAsync(patientId))
  .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _controller.UpdatePatient(patientId, patientUpdateDto);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
     Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("An error occurred while updating the patient", statusResult.Value);
        
        _mockPatientService.Verify(service => service.GetPatientByIdAsync(patientId), Times.Once);
    }

    #endregion

    #region DeletePatient Tests

  [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public async Task DeletePatient_WhenPatientExists_ReturnsNoContent(int patientId, bool serviceResult)
  {
        // Arrange
        _mockPatientService.Setup(service => service.DeletePatientAsync(patientId))
 .ReturnsAsync(serviceResult);

        // Act
 var result = await _controller.DeletePatient(patientId);

     // Assert
        Assert.IsType<NoContentResult>(result);
        
        _mockPatientService.Verify(service => service.DeletePatientAsync(patientId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public async Task DeletePatient_WhenPatientDoesNotExist_ReturnsNotFound(int patientId)
    {
      // Arrange
        _mockPatientService.Setup(service => service.DeletePatientAsync(patientId))
            .ReturnsAsync(false);

// Act
    var result = await _controller.DeletePatient(patientId);

        // Assert
     Assert.IsType<NotFoundResult>(result);
    
    _mockPatientService.Verify(service => service.DeletePatientAsync(patientId), Times.Once);
    }

    [Fact]
    public async Task DeletePatient_WhenServiceThrowsException_ReturnsInternalServerError()
{
      // Arrange
     var patientId = 1;
 _mockPatientService.Setup(service => service.DeletePatientAsync(patientId))
       .ThrowsAsync(new Exception("Database error"));

      // Act
        var result = await _controller.DeletePatient(patientId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
 Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("An error occurred while deleting the patient", statusResult.Value);
        
        _mockPatientService.Verify(service => service.DeletePatientAsync(patientId), Times.Once);
    }

    #endregion

    #region Helper Methods

    private List<PatientDto> CreateTestPatientDtos()
  {
        return new List<PatientDto>
        {
      new PatientDto
   {
     Id = 1,
        FirstName = "John",
       LastName = "Doe",
      DateOfBirth = new DateTime(1990, 1, 1),
    Gender = "Male",
        PhoneNumber = "123-456-7890",
    Address = new AddressDto
          {
        Id = 1,
             Street = "123 Main St",
            City = "New York",
 PostalCode = "10001",
Country = "USA"
           }
            },
            new PatientDto
          {
           Id = 2,
                FirstName = "Jane",
         LastName = "Smith",
          DateOfBirth = new DateTime(1985, 5, 15),
    Gender = "Female",
            PhoneNumber = "987-654-3210",
    Address = new AddressDto
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