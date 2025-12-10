using Assessments_backend.Dtos;
using Assessments_backend.Repositories.Interfaces;
using Assessments_backend.Services.Interfaces;

public class AssessmentService : IAssessmentService
{
    private readonly IPatientDataProvider _patientDataProvider;
    private readonly INoteDataProvider _noteDataProvider;
    private readonly IRiskEvaluator _riskEvaluator;

    public AssessmentService(
        IPatientDataProvider patientDataProvider,
        INoteDataProvider noteDataProvider,
        IRiskEvaluator riskEvaluator)
    {
        _patientDataProvider = patientDataProvider;
        _noteDataProvider = noteDataProvider;
        _riskEvaluator = riskEvaluator;
    }

    public async Task<AssessmentResultDto?> GetAssessment(int patientId)
    {
        var patient = await _patientDataProvider.GetPatientAsync(patientId);

        if (patient is null)
        {
            return null;
        }

        var notes = await _noteDataProvider.GetNotesByPatientIdAsync(patientId);

        var evaluation = _riskEvaluator.Evaluate(patient, notes);

        return new AssessmentResultDto
        {
            PatientId = patient.Id,
            RiskLevel = evaluation.RiskLevel,
            TriggerCount = evaluation.TriggerCount,
            MatchedTrigers = evaluation.MatchedTriggers,
            AssessedAt = DateTime.UtcNow
        };
    }
}
