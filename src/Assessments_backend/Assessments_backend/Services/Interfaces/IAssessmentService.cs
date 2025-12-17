using Assessments_backend.Dtos;

namespace Assessments_backend.Services.Interfaces
{
    public interface IAssessmentService
    {
        Task<AssessmentResultDto> GetAssessment(int id);
    }
}
