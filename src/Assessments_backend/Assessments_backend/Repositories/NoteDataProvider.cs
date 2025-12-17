using Assessments_backend.Dtos;
using Assessments_backend.Repositories.Interfaces;
using System.Net.Http.Headers;

namespace Assessments_backend.Repositories
{
    public class NoteDataProvider : INoteDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NoteDataProvider> _logger;

        public NoteDataProvider(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<NoteDataProvider> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<NoteDto>> GetNotesByPatientIdAsync(int patientId)
        {
            _logger.LogInformation("Requête de récupération des notes du patient {PatientId} via API Gateway", patientId);
            
            // Récupération du token JWT de la requête entrante
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Aucun token d'autorisation trouvé pour la requête des notes du patient {PatientId}", patientId);
                throw new UnauthorizedAccessException("No authorization token found");
            }

            _logger.LogDebug("Propagation du token JWT vers l'API Gateway pour les notes du patient {PatientId}", patientId);
            
            // Propagation du token vers l'API Gateway
            _httpClient.DefaultRequestHeaders.Authorization = 
                AuthenticationHeaderValue.Parse(token);

            try
            {
                var response = await _httpClient.GetAsync($"notes/patient/{patientId}");
                
                response.EnsureSuccessStatusCode();
                
                var notes = await response.Content.ReadFromJsonAsync<IEnumerable<NoteDto>>() 
                    ?? new List<NoteDto>();
                
                _logger.LogInformation("{NoteCount} notes récupérées avec succès pour le patient {PatientId} via API Gateway", 
                    notes.Count(), patientId);
                return notes;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Échec de la requête HTTP pour les notes du patient {PatientId}", patientId);
                throw;
            }
        }
    }
}
