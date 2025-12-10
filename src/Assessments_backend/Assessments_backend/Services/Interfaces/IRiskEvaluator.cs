using Assessments_backend.Dtos;

namespace Assessments_backend.Services.Interfaces
{
    public interface IRiskEvaluator
    {
        RiskEvaluationResult Evaluate(PatientDto patient, IEnumerable<NoteDto> notes);
    }
}
