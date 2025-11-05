using Patients_Frontend.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Patients_Frontend.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // =====================
        // 🔹 GET
        // =====================
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            await SetAuthorizationHeaderAsync();
            using var response = await _httpClient.GetAsync(endpoint);

            return await HandleResponseAsync<T>(response);
        }

        // =====================
        // 🔹 POST
        // =====================
        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            await SetAuthorizationHeaderAsync();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(endpoint, content);

            return await HandleResponseAsync<T>(response);
        }

        // =====================
        // 🔹 PUT
        // =====================
        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            await SetAuthorizationHeaderAsync();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PutAsync(endpoint, content);

            return await HandleResponseAsync<T>(response);
        }

        // =====================
        // 🔹 DELETE
        // =====================
        public async Task<bool> DeleteAsync(string endpoint)
        {
            await SetAuthorizationHeaderAsync();
            using var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

        // =====================
        // 🔹 Méthode utilitaire générique
        // =====================
        private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            // ✅ Cas typique de succès sans contenu (204)
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            // ✅ Cas succès avec JSON
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    return default;

                try
                {
                    return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException(
                        $"Erreur de désérialisation JSON pour {response.RequestMessage?.RequestUri}: {ex.Message}\nContenu: {content}"
                    );
                }
            }

            // ⚠️ Cas d'erreur HTTP
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Erreur HTTP {response.StatusCode} lors de l'appel à {response.RequestMessage?.RequestUri} : {errorContent}"
            );
        }
    }
}
