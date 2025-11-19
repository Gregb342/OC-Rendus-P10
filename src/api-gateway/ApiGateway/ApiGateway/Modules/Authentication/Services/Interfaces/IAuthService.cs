using ApiGateway.Modules.Authentication.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ApiGateway.Modules.Authentication.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);
        string GenerateJwtToken(IdentityUser user);
    }
}
