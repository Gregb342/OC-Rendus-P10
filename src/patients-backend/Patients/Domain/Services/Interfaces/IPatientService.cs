using Patients.Domain.Entities;
using Patients.DTOs;

namespace Patients.Domain.Services.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto?> GetPatientByIdAsync(int id);
        Task<int> CreatePatientAsync(PatientCreateDto patient);
        Task UpdatePatientAsync(int id, PatientUpdateDto patient);
        Task<bool> DeletePatientAsync(int id);
    }
}