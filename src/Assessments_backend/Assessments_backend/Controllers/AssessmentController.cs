using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Assessments_backend.Services.Interfaces;
using Assessments_backend.Dtos;

namespace Assessments_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(IAssessmentService assessmentService, ILogger<AssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        /// <summary>
        /// Obtient l'évaluation du risque de diabète pour un patient
        /// </summary>
        /// <param name="patientId">ID du patient</param>
        /// <returns>Résultat de l'évaluation du risque</returns>
        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(typeof(AssessmentResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssessmentResultDto>> GetAssessment(int patientId)
        {
            _logger.LogInformation("Début de la requête d'évaluation pour le patient {PatientId}", patientId);
            
            if (patientId <= 0)
            {
                _logger.LogWarning("ID de patient invalide: {PatientId}", patientId);
                return BadRequest(new { message = "L'ID du patient doit être supérieur à 0" });
            }

            try
            {
                var assessment = await _assessmentService.GetAssessment(patientId);

                if (assessment == null)
                {
                    _logger.LogWarning("Aucune évaluation trouvée pour le patient {PatientId}", patientId);
                    return NotFound(new { message = $"Aucune évaluation trouvée pour le patient {patientId}" });
                }

                _logger.LogInformation("Évaluation réussie pour le patient {PatientId} - Niveau de risque: {RiskLevel}", 
                    patientId, assessment.RiskLevel);
                return Ok(assessment);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Erreur d'autorisation lors de l'évaluation du patient {PatientId}", patientId);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Patient {PatientId} introuvable", patientId);
                return NotFound(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Service indisponible lors de l'évaluation du patient {PatientId}", patientId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { message = "Service temporairement indisponible", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de l'évaluation du patient {PatientId}", patientId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Une erreur s'est produite lors de l'évaluation", details = ex.Message });
            }
        }
    }
}
