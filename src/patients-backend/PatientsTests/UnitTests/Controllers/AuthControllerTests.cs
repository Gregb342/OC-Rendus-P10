using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Patients.Controllers;
using Patients.DTOs;
using System.Security.Claims;

namespace PatientsTests.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        // Setup UserManager mock
        var store = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        
        _controller = new AuthController(
            _mockUserManager.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Setup HttpContext
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Setup default configuration values
        SetupConfiguration();
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginDto = CreateTestLoginDto();
        var user = new IdentityUser { UserName = loginDto.Username };
        
        _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        var tokenProperty = response.GetType().GetProperty("token");
        var expirationProperty = response.GetType().GetProperty("expiration");
        
        Assert.NotNull(tokenProperty);
        Assert.NotNull(expirationProperty);
        
        var token = tokenProperty.GetValue(response)?.ToString();
        Assert.False(string.IsNullOrEmpty(token));
        
        _mockUserManager.Verify(um => um.FindByNameAsync(loginDto.Username), Times.Once);
        _mockUserManager.Verify(um => um.CheckPasswordAsync(user, loginDto.Password), Times.Once);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("username", "")]
    [InlineData("", "")]
    [InlineData(null, "password")]
    [InlineData("username", null)]
    public async Task Login_WithMissingCredentials_ReturnsBadRequest(string username, string password)
    {
        // Arrange
        var loginDto = new LoginDto { Username = username, Password = password };

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username and password are required", badRequestResult.Value);
        
        _mockUserManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = CreateTestLoginDto();
        
        _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username))
            .ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        
        _mockUserManager.Verify(um => um.FindByNameAsync(loginDto.Username), Times.Once);
        _mockUserManager.Verify(um => um.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = CreateTestLoginDto();
        var user = new IdentityUser { UserName = loginDto.Username };
        
        _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        
        _mockUserManager.Verify(um => um.FindByNameAsync(loginDto.Username), Times.Once);
        _mockUserManager.Verify(um => um.CheckPasswordAsync(user, loginDto.Password), Times.Once);
    }

    [Theory]
    [InlineData("testuser1", "password123")]
    [InlineData("admin", "admin123")]
    [InlineData("user@example.com", "mypassword")]
    public async Task Login_WithDifferentValidUsers_ReturnsTokens(string username, string password)
    {
        // Arrange
        var loginDto = new LoginDto { Username = username, Password = password };
        var user = new IdentityUser { UserName = username };
        
        _mockUserManager.Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        _mockUserManager.Verify(um => um.FindByNameAsync(username), Times.Once);
        _mockUserManager.Verify(um => um.CheckPasswordAsync(user, password), Times.Once);
    }

    [Fact]
    public async Task Login_WhenUserManagerThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var loginDto = CreateTestLoginDto();
        
        _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("An error occurred during authentication", statusResult.Value);
        
        _mockUserManager.Verify(um => um.FindByNameAsync(loginDto.Username), Times.Once);
    }

    [Fact]
    public async Task Login_WithValidCredentials_GeneratesValidJwtToken()
    {
        // Arrange
        var loginDto = CreateTestLoginDto();
        var user = new IdentityUser { UserName = loginDto.Username };
        
        _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var tokenProperty = response.GetType().GetProperty("token");
        var token = tokenProperty?.GetValue(response)?.ToString();
        
        Assert.NotNull(token);
        
        // Validate JWT token format
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(token));
        
        var jwtToken = tokenHandler.ReadJwtToken(token);
        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Contains("TestAudience", jwtToken.Audiences);
        
        // Check for ClaimTypes.Name instead of unique_name
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == loginDto.Username);
    }

    [Fact]
    public async Task Login_ChecksTokenExpiration_IsInFuture()
    {
        // Arrange
        var loginDto = CreateTestLoginDto();
        var user = new IdentityUser { UserName = loginDto.Username };
        
        _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(true);

        var beforeLogin = DateTime.Now;

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var expirationProperty = response.GetType().GetProperty("expiration");
        var expiration = (DateTime)expirationProperty?.GetValue(response)!;
        
        Assert.True(expiration > beforeLogin);
        Assert.True(expiration > DateTime.Now);
        // Token should expire in approximately 3 hours
        Assert.True(expiration <= DateTime.Now.AddHours(4)); 
    }

    #endregion

    #region Helper Methods

    private LoginDto CreateTestLoginDto()
    {
        return new LoginDto
        {
            Username = "testuser",
            Password = "testpassword123"
        };
    }

    private void SetupConfiguration()
    {
        var configurationItems = new Dictionary<string, string>
        {
            ["JWT:Secret"] = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256Encryption",
            ["JWT:ValidIssuer"] = "TestIssuer",
            ["JWT:ValidAudience"] = "TestAudience"
        };

        _mockConfiguration.Setup(c => c[It.IsAny<string>()])
            .Returns((string key) => configurationItems.TryGetValue(key, out var value) ? value : null);

        foreach (var item in configurationItems)
        {
            _mockConfiguration.Setup(c => c[item.Key]).Returns(item.Value);
        }
    }

    #endregion
}