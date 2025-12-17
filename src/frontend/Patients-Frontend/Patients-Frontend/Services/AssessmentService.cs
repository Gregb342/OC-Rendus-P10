using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AssessmentService> _logger;

        public AssessmentService(IApiService apiService, ILogger<AssessmentService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<AssessmentResultDto?> GetPatientAssessmentAsync(int patientId)
        {
            try
            {
                _logger.LogInformation("Récupération de l'assessment pour le patient {PatientId}", patientId);
                
                var assessment = await _apiService.GetAsync<AssessmentResultDto>($"/assessment/patient/{patientId}");
                
                if (assessment != null)
                {
                    _logger.LogInformation("Assessment récupéré avec succès pour le patient {PatientId} - Niveau de risque: {RiskLevel}", 
                        patientId, assessment.RiskLevel);
                }
                else
                {
                    _logger.LogWarning("Aucun assessment trouvé pour le patient {PatientId}", patientId);
                }
                
                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'assessment pour le patient {PatientId}", patientId);
                return null;
            }
        }
    }
}
