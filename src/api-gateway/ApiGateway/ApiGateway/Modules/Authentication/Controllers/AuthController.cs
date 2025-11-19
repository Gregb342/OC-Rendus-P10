using ApiGateway.Modules.Authentication.Controllers;
using ApiGateway.Modules.Authentication.DTOs;
using ApiGateway.Modules.Authentication.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Modules.Authentication.Controllers
{
    /// <summary>
    /// Contrôleur gérant l'authentification (Login/Register)
    /// </summary>
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authentifie un utilisateur et retourne un token JWT
        /// </summary>
        /// <param name="loginDto">Credentials de l'utilisateur</param>
        /// <returns>Token JWT et informations utilisateur</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Username}", loginDto.Username);

                if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
                {
                    _logger.LogWarning("Login attempt with missing credentials");
                    return BadRequest("Username and password are required");
                }

                var result = await _authService.LoginAsync(loginDto);

                if (result == null)
                {
                    _logger.LogWarning("Failed login attempt for user: {Username}", loginDto.Username);
                    return Unauthorized("Invalid username or password");
                }

                _logger.LogInformation("Successful login for user: {Username}", loginDto.Username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", loginDto.Username);
                return StatusCode(500, "An error occurred during authentication");
            }
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        /// <param name="registerDto">Informations du nouvel utilisateur</param>
        /// <returns>Confirmation de création</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Registration attempt for user: {Username}", registerDto.Username);

                if (string.IsNullOrEmpty(registerDto.Username) ||
                    string.IsNullOrEmpty(registerDto.Email) ||
                    string.IsNullOrEmpty(registerDto.Password))
                {
                    _logger.LogWarning("Registration attempt with missing fields");
                    return BadRequest("Username, email and password are required");
                }

                var result = await _authService.RegisterAsync(registerDto);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed registration for user: {Username}. Errors: {Errors}",
                        registerDto.Username,
                        string.Join(", ", result.Errors.Select(e => e.Description)));

                    return BadRequest(result.Errors);
                }

                _logger.LogInformation("Successful registration for user: {Username}", registerDto.Username);
                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", registerDto.Username);
                return StatusCode(500, "An error occurred during registration");
            }
        }
    }
}