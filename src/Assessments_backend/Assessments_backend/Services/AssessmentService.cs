using Assessments_backend.Dtos;
using Assessments_backend.Enums;
using Assessments_backend.Repositories.Interfaces;
using Assessments_backend.Services.Interfaces;

namespace Assessments_backend.Services
{
    public class AssessmentService : IAssessmentService
    {
        public readonly IPatientDataProvider _patientDataProvider;
        public readonly INoteDataProvider _noteDataProvider;

        public AssessmentService(
            IPatientDataProvider patientDataProvider,
            INoteDataProvider noteDataProvider)
        {
            _patientDataProvider = patientDataProvider;
            _noteDataProvider = noteDataProvider;
        }

        public async Task<AssessmentResultDto> GetAssessment(int patientId)
        {
            var patient = await _patientDataProvider.GetPatientAsync(patientId);

            if (patient is null)
            {
                return null;
            }

            var notes = await _noteDataProvider.GetNotesByPatientIdAsync(patientId);        

            var riskLevel = EvaluateRisk(patient, notes);

            AssessmentResultDto assessmentResult = new AssessmentResultDto
            {
                PatientId = patient.id,
                RiskLevel = riskLevel,
                TriggerCount = 0, // a changer avec le nombre de trigger
                MatchedTrigers = null, // a changer avec la liste des triggers
                AssessedAt = DateTime.UtcNow
            };

            return assessmentResult;
        }

        private RiskLevel EvaluateRisk(PatientDto patient, IEnumerable<NoteDto> notes)
        {
            return RiskLevel.None;
        }
    }
}
