using Microsoft.EntityFrameworkCore;
using Patients.Data;
using Patients.Domain.Entities;
using Patients.Infrastructure.Repositories;

namespace PatientsTests.UnitTests.Repositories;

public class PatientRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PatientRepository _repository;

    public PatientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new PatientRepository(_context);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenPatientsExist_ReturnsAllPatients()
    {
        // Arrange
        var patients = CreateTestPatients();
        await _context.Patients.AddRangeAsync(patients);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var patientsList = result.ToList();
        Assert.Contains(patientsList, p => p.FirstName == "John" && p.LastName == "Doe");
        Assert.Contains(patientsList, p => p.FirstName == "Jane" && p.LastName == "Smith");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoPatientsExist_ReturnsEmptyCollection()
    {
        // Arrange - No patients added to context

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_IncludesPatientAddress()
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var retrievedPatient = result.First();
        Assert.NotNull(retrievedPatient.PatientAddress);
        Assert.Equal("123 Main St", retrievedPatient.PatientAddress.Street);
        Assert.Equal("New York", retrievedPatient.PatientAddress.City);
    }

    #endregion

    #region GetByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(42)]
    public async Task GetByIdAsync_WhenPatientExists_ReturnsPatientWithAddress(int patientId)
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        patient.Id = patientId;
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patientId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.NotNull(result.PatientAddress);
        Assert.Equal("123 Main St", result.PatientAddress.Street);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetByIdAsync_WhenPatientDoesNotExist_ReturnsNull(int patientId)
    {
        // Arrange - No patient with this ID exists

        // Act
        var result = await _repository.GetByIdAsync(patientId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPatientExistsWithoutAddress_ReturnsPatientWithNullAddress()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            PhoneNumber = "123-456-7890"
        };
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.Id);
        Assert.Null(result.PatientAddress);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WhenValidPatient_AddsPatientAndReturnsWithId()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "New",
            LastName = "Patient",
            DateOfBirth = new DateTime(1995, 6, 15),
            Gender = "Female",
            PhoneNumber = "555-0123"
        };

        // Act
        var result = await _repository.AddAsync(patient);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("Patient", result.LastName);
        
        // Verify it was actually saved to database
        var savedPatient = await _context.Patients.FindAsync(result.Id);
        Assert.NotNull(savedPatient);
        Assert.Equal("New", savedPatient.FirstName);
    }

    [Fact]
    public async Task AddAsync_WhenPatientWithAddress_SavesBothEntities()
    {
        // Arrange
        var address = new Address
        {
            Street = "456 Oak St",
            City = "Boston",
            PostalCode = "02101",
            Country = "USA"
        };
        
        var patient = new Patient
        {
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = new DateTime(1988, 3, 10),
            Gender = "Male",
            PhoneNumber = "555-0456",
            PatientAddress = address
        };

        // Act
        var result = await _repository.AddAsync(patient);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.NotNull(result.PatientAddress);
        Assert.True(result.PatientAddress.Id > 0);
        
        // Verify both entities were saved
        var savedPatient = await _context.Patients
            .Include(p => p.PatientAddress)
            .FirstOrDefaultAsync(p => p.Id == result.Id);
        Assert.NotNull(savedPatient);
        Assert.NotNull(savedPatient.PatientAddress);
        Assert.Equal("456 Oak St", savedPatient.PatientAddress.Street);
    }

    [Theory]
    [InlineData("Male", "123-456-7890")]
    [InlineData("Female", "987-654-3210")]
    [InlineData("Other", "555-0199")]
    public async Task AddAsync_WithDifferentGendersAndPhones_SavesCorrectly(string gender, string phone)
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Test",
            LastName = "Patient",
            DateOfBirth = new DateTime(1992, 8, 20),
            Gender = gender,
            PhoneNumber = phone
        };

        // Act
        var result = await _repository.AddAsync(patient);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gender, result.Gender);
        Assert.Equal(phone, result.PhoneNumber);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenPatientExists_UpdatesPatientSuccessfully()
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
        
        // Modify patient data
        patient.FirstName = "Updated";
        patient.LastName = "Name";
        patient.PhoneNumber = "999-888-7777";

        // Act
        await _repository.UpdateAsync(patient);

        // Assert
        var updatedPatient = await _context.Patients.FindAsync(patient.Id);
        Assert.NotNull(updatedPatient);
        Assert.Equal("Updated", updatedPatient.FirstName);
        Assert.Equal("Name", updatedPatient.LastName);
        Assert.Equal("999-888-7777", updatedPatient.PhoneNumber);
    }

    [Fact]
    public async Task UpdateAsync_WhenPatientAddressUpdated_UpdatesAddressToo()
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
        
        // Modify address
        patient.PatientAddress!.Street = "Updated Street";
        patient.PatientAddress.City = "Updated City";

        // Act
        await _repository.UpdateAsync(patient);

        // Assert
        var updatedPatient = await _context.Patients
            .Include(p => p.PatientAddress)
            .FirstOrDefaultAsync(p => p.Id == patient.Id);
        Assert.NotNull(updatedPatient);
        Assert.NotNull(updatedPatient.PatientAddress);
        Assert.Equal("Updated Street", updatedPatient.PatientAddress.Street);
        Assert.Equal("Updated City", updatedPatient.PatientAddress.City);
    }

    [Theory]
    [InlineData("1980-01-01")]
    [InlineData("2000-12-31")]
    [InlineData("1995-06-15")]
    public async Task UpdateAsync_WithDifferentDatesOfBirth_UpdatesCorrectly(string dateString)
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
        
        var newDate = DateTime.Parse(dateString);
        patient.DateOfBirth = newDate;

        // Act
        await _repository.UpdateAsync(patient);

        // Assert
        var updatedPatient = await _context.Patients.FindAsync(patient.Id);
        Assert.NotNull(updatedPatient);
        Assert.Equal(newDate, updatedPatient.DateOfBirth);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenPatientExists_DeletesPatientAndReturnsTrue()
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
        var patientId = patient.Id;

        // Act
        var result = await _repository.DeleteAsync(patientId);

        // Assert
        Assert.True(result);
        
        // Verify patient is deleted
        var deletedPatient = await _context.Patients.FindAsync(patientId);
        Assert.Null(deletedPatient);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task DeleteAsync_WhenPatientDoesNotExist_ReturnsFalse(int patientId)
    {
        // Arrange - No patient with this ID exists

        // Act
        var result = await _repository.DeleteAsync(patientId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenPatientHasAddress_DeletesPatientButKeepsAddress()
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
        var patientId = patient.Id;
        var addressId = patient.PatientAddress!.Id;

        // Act
        var result = await _repository.DeleteAsync(patientId);

        // Assert
        Assert.True(result);
        
        // Verify patient is deleted but address remains
        var deletedPatient = await _context.Patients.FindAsync(patientId);
        Assert.Null(deletedPatient);
        
        var address = await _context.Addresses.FindAsync(addressId);
        Assert.NotNull(address); // Address should still exist
    }

    #endregion

    #region PatientExistsAsync Tests

    [Fact]
    public async Task PatientExistsAsync_WhenPatientExists_ReturnsTrue()
    {
        // Arrange
        var patient = CreateTestPatientWithAddress();
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PatientExistsAsync(patient.Id);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task PatientExistsAsync_WhenPatientDoesNotExist_ReturnsFalse(int patientId)
    {
        // Arrange - No patient with this ID exists

        // Act
        var result = await _repository.PatientExistsAsync(patientId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PatientExistsAsync_WithMultiplePatients_ReturnsCorrectResults()
    {
        // Arrange
        var patients = CreateTestPatients();
        await _context.Patients.AddRangeAsync(patients);
        await _context.SaveChangesAsync();
        
        var existingId = patients.First().Id;
        var nonExistingId = 999;

        // Act & Assert
        Assert.True(await _repository.PatientExistsAsync(existingId));
        Assert.False(await _repository.PatientExistsAsync(nonExistingId));
    }

    #endregion

    #region Helper Methods

    private List<Patient> CreateTestPatients()
    {
        return new List<Patient>
        {
            new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                PhoneNumber = "123-456-7890"
            },
            new Patient
            {
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Female",
                PhoneNumber = "987-654-3210"
            }
        };
    }

    private Patient CreateTestPatientWithAddress()
    {
        var address = new Address
        {
            Street = "123 Main St",
            City = "New York",
            PostalCode = "10001",
            Country = "USA"
        };

        return new Patient
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            PhoneNumber = "123-456-7890",
            PatientAddress = address
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}