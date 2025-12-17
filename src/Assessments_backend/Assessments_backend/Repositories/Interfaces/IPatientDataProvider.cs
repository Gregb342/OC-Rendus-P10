using Assessments_backend.Dtos;

namespace Assessments_backend.Repositories.Interfaces
{
    public interface IPatientDataProvider
    {
        Task<PatientDto> GetPatientAsync(int id);
    }
}
