using Assessments_backend.Dtos;
using Assessments_backend.Repositories.Interfaces;

namespace Assessments_backend.Repositories
{
    public class PatientDataProvider : IPatientDataProvider
    {
        public Task<PatientDto> GetPatientAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
