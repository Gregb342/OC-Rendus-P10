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
            _logger.LogInformation($"Login attempt for user: {model.Username}");
            
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var token = GetToken(authClaims);
                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation($"User {model.Username} authenticated successfully. Token generated.");
                
                _logger.LogDebug($"Token expiry: {token.ValidTo}");
                _logger.LogDebug($"Token issuer: {_configuration["JWT:ValidIssuer"]}");
                _logger.LogDebug($"Token audience: {_configuration["JWT:ValidAudience"]}");

                return Ok(new
                {
                    token = tokenString,
                    expiration = token.ValidTo
                });
            }
            
            _logger.LogWarning($"Authentication failed for user: {model.Username}");
            return Unauthorized();
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var jwtSecret = _configuration["JWT:Secret"] ?? "DefaultSecretKeyForDevThatShouldBeChangedInProduction";
            var issuer = _configuration["JWT:ValidIssuer"];
            var audience = _configuration["JWT:ValidAudience"];
            
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            _logger.LogDebug($"Creating token with issuer: {issuer}, audience: {audience}");

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
    }
}