using Patients_Frontend.DTOs;

namespace Patients_Frontend.Services.Interfaces
{
    public interface IAssessmentService
    {
        Task<AssessmentResultDto?> GetPatientAssessmentAsync(int patientId);
    }
}
