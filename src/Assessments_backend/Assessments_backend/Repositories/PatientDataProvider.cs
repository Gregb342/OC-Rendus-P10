using Assessments_backend.Dtos;
using Assessments_backend.Repositories.Interfaces;
using System.Net.Http.Headers;

namespace Assessments_backend.Repositories
{
    public class PatientDataProvider : IPatientDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PatientDataProvider> _logger;

        public PatientDataProvider(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<PatientDataProvider> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<PatientDto> GetPatientAsync(int id)
        {
            _logger.LogInformation("Requête de récupération du patient {PatientId} via API Gateway", id);
            
            // Récupération du token JWT de la requête entrante
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Aucun token d'autorisation trouvé pour la requête du patient {PatientId}", id);
                throw new UnauthorizedAccessException("No authorization token found");
            }

            _logger.LogDebug("Propagation du token JWT vers l'API Gateway pour le patient {PatientId}", id);
            
            // Propagation du token vers l'API Gateway
            _httpClient.DefaultRequestHeaders.Authorization = 
                AuthenticationHeaderValue.Parse(token);

            try
            {
                var response = await _httpClient.GetAsync($"patients/{id}");
                
                response.EnsureSuccessStatusCode();
                
                var patient = await response.Content.ReadFromJsonAsync<PatientDto>() 
                    ?? throw new InvalidOperationException($"Patient with ID {id} not found");
                
                _logger.LogInformation("Patient {PatientId} récupéré avec succès via API Gateway", id);
                return patient;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Échec de la requête HTTP pour le patient {PatientId}", id);
                throw;
            }
        }
    }
}
