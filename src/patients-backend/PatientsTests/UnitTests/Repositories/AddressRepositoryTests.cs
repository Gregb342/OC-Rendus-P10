using Microsoft.EntityFrameworkCore;
using Patients.Data;
using Patients.Domain.Entities;
using Patients.Infrastructure.Repositories;

namespace PatientsTests.UnitTests.Repositories;

public class AddressRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AddressRepository _repository;

    public AddressRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new AddressRepository(_context);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenAddressesExist_ReturnsAllAddresses()
    {
        // Arrange
        var addresses = CreateTestAddresses();
        await _context.Addresses.AddRangeAsync(addresses);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        
        var addressesList = result.ToList();
        Assert.Contains(addressesList, a => a.Street == "123 Main St" && a.City == "New York");
        Assert.Contains(addressesList, a => a.Street == "456 Oak Ave" && a.City == "Los Angeles");
        Assert.Contains(addressesList, a => a.Street == "789 Pine Rd" && a.City == "Chicago");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoAddressesExist_ReturnsEmptyCollection()
    {
        // Arrange - No addresses added to context

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task GetAllAsync_WithDifferentAmounts_ReturnsCorrectCount(int addressCount)
    {
        // Arrange
        var addresses = new List<Address>();
        for (int i = 0; i < addressCount; i++)
        {
            addresses.Add(new Address
            {
                Street = $"Street {i}",
                City = $"City {i}",
                PostalCode = $"1000{i}",
                Country = "USA"
            });
        }
        await _context.Addresses.AddRangeAsync(addresses);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(addressCount, result.Count());
    }

    #endregion

    #region GetByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(42)]
    public async Task GetByIdAsync_WhenAddressExists_ReturnsCorrectAddress(int addressId)
    {
        // Arrange
        var address = new Address
        {
            Id = addressId,
            Street = "123 Test St",
            City = "Test City",
            PostalCode = "12345",
            Country = "Test Country"
        };
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(addressId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addressId, result.Id);
        Assert.Equal("123 Test St", result.Street);
        Assert.Equal("Test City", result.City);
        Assert.Equal("12345", result.PostalCode);
        Assert.Equal("Test Country", result.Country);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetByIdAsync_WhenAddressDoesNotExist_ReturnsNull(int addressId)
    {
        // Arrange - No address with this ID exists

        // Act
        var result = await _repository.GetByIdAsync(addressId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithMultipleAddresses_ReturnsCorrectOne()
    {
        // Arrange
        var addresses = CreateTestAddresses();
        await _context.Addresses.AddRangeAsync(addresses);
        await _context.SaveChangesAsync();
        
        var targetAddress = addresses.First(a => a.City == "Los Angeles");

        // Act
        var result = await _repository.GetByIdAsync(targetAddress.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetAddress.Id, result.Id);
        Assert.Equal("456 Oak Ave", result.Street);
        Assert.Equal("Los Angeles", result.City);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WhenValidAddress_AddsAddressAndReturnsWithId()
    {
        // Arrange
        var address = new Address
        {
            Street = "New Street",
            City = "New City",
            PostalCode = "54321",
            Country = "New Country"
        };

        // Act
        var result = await _repository.AddAsync(address);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("New Street", result.Street);
        Assert.Equal("New City", result.City);
        Assert.Equal("54321", result.PostalCode);
        Assert.Equal("New Country", result.Country);
        
        // Verify it was actually saved to database
        var savedAddress = await _context.Addresses.FindAsync(result.Id);
        Assert.NotNull(savedAddress);
        Assert.Equal("New Street", savedAddress.Street);
    }

    [Theory]
    [InlineData("123 Main St", "New York", "10001", "USA")]
    [InlineData("456 Oak Ave", "Los Angeles", "90210", "USA")]
    [InlineData("789 Elm St", "Chicago", "60601", "USA")]
    public async Task AddAsync_WithDifferentAddressData_SavesCorrectly(string street, string city, string postalCode, string country)
    {
        // Arrange
        var address = new Address
        {
            Street = street,
            City = city,
            PostalCode = postalCode,
            Country = country
        };

        // Act
        var result = await _repository.AddAsync(address);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(street, result.Street);
        Assert.Equal(city, result.City);
        Assert.Equal(postalCode, result.PostalCode);
        Assert.Equal(country, result.Country);
    }

    [Fact]
    public async Task AddAsync_MultipleAddresses_AllGetUniqueIds()
    {
        // Arrange
        var addresses = new List<Address>
        {
            new Address { Street = "Street 1", City = "City 1", PostalCode = "10001", Country = "Country 1" },
            new Address { Street = "Street 2", City = "City 2", PostalCode = "10002", Country = "Country 2" },
            new Address { Street = "Street 3", City = "City 3", PostalCode = "10003", Country = "Country 3" }
        };

        // Act
        var results = new List<Address>();
        foreach (var address in addresses)
        {
            results.Add(await _repository.AddAsync(address));
        }

        // Assert
        Assert.All(results, r => Assert.True(r.Id > 0));
        Assert.Equal(results.Count, results.Select(r => r.Id).Distinct().Count()); // All IDs should be unique
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenAddressExists_UpdatesAddressSuccessfully()
    {
        // Arrange
        var address = new Address
        {
            Street = "Original Street",
            City = "Original City",
            PostalCode = "00000",
            Country = "Original Country"
        };
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        
        // Modify address data
        address.Street = "Updated Street";
        address.City = "Updated City";
        address.PostalCode = "99999";
        address.Country = "Updated Country";

        // Act
        await _repository.UpdateAsync(address);

        // Assert
        var updatedAddress = await _context.Addresses.FindAsync(address.Id);
        Assert.NotNull(updatedAddress);
        Assert.Equal("Updated Street", updatedAddress.Street);
        Assert.Equal("Updated City", updatedAddress.City);
        Assert.Equal("99999", updatedAddress.PostalCode);
        Assert.Equal("Updated Country", updatedAddress.Country);
    }

    [Theory]
    [InlineData("Updated Street 1", "Updated City 1")]
    [InlineData("Updated Street 2", "Updated City 2")]
    [InlineData("Updated Street 3", "Updated City 3")]
    public async Task UpdateAsync_WithDifferentUpdates_UpdatesCorrectly(string newStreet, string newCity)
    {
        // Arrange
        var address = new Address
        {
            Street = "Original Street",
            City = "Original City",
            PostalCode = "12345",
            Country = "USA"
        };
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        
        address.Street = newStreet;
        address.City = newCity;

        // Act
        await _repository.UpdateAsync(address);

        // Assert
        var updatedAddress = await _context.Addresses.FindAsync(address.Id);
        Assert.NotNull(updatedAddress);
        Assert.Equal(newStreet, updatedAddress.Street);
        Assert.Equal(newCity, updatedAddress.City);
    }

    [Fact]
    public async Task UpdateAsync_OnlyPartialUpdate_UpdatesOnlyChangedFields()
    {
        // Arrange
        var address = new Address
        {
            Street = "Original Street",
            City = "Original City",
            PostalCode = "12345",
            Country = "USA"
        };
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        
        // Only update street and postal code
        address.Street = "Updated Street";
        address.PostalCode = "54321";

        // Act
        await _repository.UpdateAsync(address);

        // Assert
        var updatedAddress = await _context.Addresses.FindAsync(address.Id);
        Assert.NotNull(updatedAddress);
        Assert.Equal("Updated Street", updatedAddress.Street);
        Assert.Equal("Original City", updatedAddress.City); // Should remain unchanged
        Assert.Equal("54321", updatedAddress.PostalCode);
        Assert.Equal("USA", updatedAddress.Country); // Should remain unchanged
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenAddressExists_DeletesAddressAndReturnsTrue()
    {
        // Arrange
        var address = new Address
        {
            Street = "To Delete Street",
            City = "To Delete City",
            PostalCode = "99999",
            Country = "To Delete Country"
        };
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        var addressId = address.Id;

        // Act
        var result = await _repository.DeleteAsync(addressId);

        // Assert
        Assert.True(result);
        
        // Verify address is deleted
        var deletedAddress = await _context.Addresses.FindAsync(addressId);
        Assert.Null(deletedAddress);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task DeleteAsync_WhenAddressDoesNotExist_ReturnsFalse(int addressId)
    {
        // Arrange - No address with this ID exists

        // Act
        var result = await _repository.DeleteAsync(addressId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WithMultipleAddresses_DeletesOnlyTargetAddress()
    {
        // Arrange
        var addresses = CreateTestAddresses();
        await _context.Addresses.AddRangeAsync(addresses);
        await _context.SaveChangesAsync();
        
        var addressToDelete = addresses.First();
        var remainingAddresses = addresses.Skip(1).ToList();

        // Act
        var result = await _repository.DeleteAsync(addressToDelete.Id);

        // Assert
        Assert.True(result);
        
        // Verify target address is deleted
        var deletedAddress = await _context.Addresses.FindAsync(addressToDelete.Id);
        Assert.Null(deletedAddress);
        
        // Verify other addresses still exist
        foreach (var remainingAddress in remainingAddresses)
        {
            var existingAddress = await _context.Addresses.FindAsync(remainingAddress.Id);
            Assert.NotNull(existingAddress);
        }
    }

    [Fact]
    public async Task DeleteAsync_WhenAddressReferencedByPatient_StillDeletesSuccessfully()
    {
        // Arrange
        var address = new Address
        {
            Street = "Referenced Street",
            City = "Referenced City",
            PostalCode = "12345",
            Country = "USA"
        };
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();

        var patient = new Patient
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            PhoneNumber = "123-456-7890",
            AddressId = address.Id,
            PatientAddress = address
        };
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(address.Id);

        // Assert
        Assert.True(result);
        
        // Verify address is deleted
        var deletedAddress = await _context.Addresses.FindAsync(address.Id);
        Assert.Null(deletedAddress);
    }

    #endregion

    #region Helper Methods

    private List<Address> CreateTestAddresses()
    {
        return new List<Address>
        {
            new Address
            {
                Street = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA"
            },
            new Address
            {
                Street = "456 Oak Ave",
                City = "Los Angeles",
                PostalCode = "90210",
                Country = "USA"
            },
            new Address
            {
                Street = "789 Pine Rd",
                City = "Chicago",
                PostalCode = "60601",
                Country = "USA"
            }
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}