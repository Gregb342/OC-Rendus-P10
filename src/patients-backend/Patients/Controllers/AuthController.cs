using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Patients.DTOs;

namespace Patients.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var traceId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["Username"] = model.Username,
                ["RequestIP"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            }))
            try
            {
                _logger.LogInformation("Handling POST /api/auth/login for user {Username}", model.Username);
                
                // Log request details (sans informations sensibles)
                _logger.LogDebug("Login attempt from IP {RequestIP} for user {Username}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", model.Username);
                
                // Validation des données d'entrée
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    _logger.LogWarning("Login attempt with missing credentials for user {Username}", model.Username ?? "null");
                    return BadRequest("Username and password are required");
                }

                _logger.LogDebug("Searching for user {Username} in user store", model.Username);
                var user = await _userManager.FindByNameAsync(model.Username);
                
                if (user == null)
                {
                    _logger.LogWarning("User {Username} not found in user store", model.Username);
                    return Unauthorized();
                }

                _logger.LogDebug("User {Username} found, checking password", model.Username);
                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                
                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password provided for user {Username}", model.Username);
                    return Unauthorized();
                }

                _logger.LogInformation("User {Username} authenticated successfully, generating token", model.Username);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                _logger.LogDebug("Creating JWT token for user {Username} with {ClaimsCount} claims", 
                    model.Username, authClaims.Count);

                var token = GetToken(authClaims);
                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation("JWT token generated successfully for user {Username}, expires at {TokenExpiry}", 
                    model.Username, token.ValidTo);
                
                _logger.LogDebug("Token details - Issuer: {TokenIssuer}, Audience: {TokenAudience}, JTI: {TokenJti}", 
                    token.Issuer, token.Audiences.FirstOrDefault(), 
                    authClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value);

                var response = new
                {
                    token = tokenString,
                    expiration = token.ValidTo
                };

                _logger.LogInformation("Login successful for user {Username}, returning token response", model.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process for user {Username}", model.Username);
                return StatusCode(500, "An error occurred during authentication");
            }
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var traceId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["Method"] = "GetToken"
            }))
            try
            {
                _logger.LogDebug("Starting JWT token creation process");
                
                var jwtSecret = _configuration["JWT:Secret"] ?? "DefaultSecretKeyForDevThatShouldBeChangedInProduction";
                var issuer = _configuration["JWT:ValidIssuer"];
                var audience = _configuration["JWT:ValidAudience"];
                
                _logger.LogDebug("JWT configuration - Issuer: {TokenIssuer}, Audience: {TokenAudience}", issuer, audience);
                
                // Validation de la configuration
                if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                {
                    _logger.LogError("JWT configuration is incomplete - Issuer: {TokenIssuer}, Audience: {TokenAudience}", 
                        issuer ?? "null", audience ?? "null");
                    throw new InvalidOperationException("JWT configuration is incomplete");
                }

                // Validation de la taille de la clé JWT
                var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);
                if (keyBytes.Length < 32) // 256 bits minimum
                {
                    _logger.LogError("JWT secret key is too short. Current length: {KeyLength} bytes, minimum required: 32 bytes (256 bits)", keyBytes.Length);
                    throw new InvalidOperationException($"JWT secret key must be at least 32 characters long (256 bits). Current length: {keyBytes.Length} bytes");
                }

                if (jwtSecret == "DefaultSecretKeyForDevThatShouldBeChangedInProduction")
                {
                    _logger.LogWarning("Using default JWT secret key - this should be changed in production");
                }

                var authSigningKey = new SymmetricSecurityKey(keyBytes);
                var expirationTime = DateTime.Now.AddHours(3);

                _logger.LogDebug("Creating JWT token with expiration time {ExpirationTime}", expirationTime);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    expires: expirationTime,
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                _logger.LogDebug("JWT token created successfully with {ClaimsCount} claims", authClaims.Count);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating JWT token");
                throw;
            }
        }
    }
}