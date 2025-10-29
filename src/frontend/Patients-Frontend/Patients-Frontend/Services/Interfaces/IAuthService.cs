using Patients_Frontend.DTOs;

namespace Patients_Frontend.Services.Interfaces
{
    public interface IAuthService
    {
        event Action? AuthenticationStateChanged;
        Task<bool> LoginAsync(LoginDto loginDto);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetTokenAsync();
    }
}
