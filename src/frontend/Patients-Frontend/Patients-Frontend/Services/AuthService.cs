using Microsoft.AspNetCore.Components.Authorization;
using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;
using System.Text.Json;

namespace Patients_Frontend.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly CustomAuthStateProvider _authStateProvider;

        public event Action? AuthenticationStateChanged;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        public async Task<bool> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginDto, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                Console.WriteLine($"Tentative de login avec: {loginDto.Username}");

                // Appel vers votre API Gateway
                var response = await _httpClient.PostAsync("/auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erreur HTTP: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erreur détaillée: {errorContent}");
                    return false;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Réponse de l'API: {responseJson}");

                var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (result.TryGetProperty("token", out var tokenElement))
                {
                    var token = tokenElement.GetString();
                    if (string.IsNullOrEmpty(token))
                        return false;

                    await _authStateProvider.MarkUserAsAuthenticated(token);
                    AuthenticationStateChanged?.Invoke();
                    return true;
                }

                // Si pas de token mais réponse 200, considérer comme authentifié
                // et créer un token simple pour l'état local
                await _authStateProvider.MarkUserAsAuthenticated("authenticated");
                AuthenticationStateChanged?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du login : {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.MarkUserAsLoggedOut();
            AuthenticationStateChanged?.Invoke();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.IsAuthenticated ?? false;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _authStateProvider.GetTokenAsync();
        }
    }
}