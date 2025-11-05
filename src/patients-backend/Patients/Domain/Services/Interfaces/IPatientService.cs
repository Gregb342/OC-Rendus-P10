using Patients.Domain.Entities;
using Patients.DTOs;

namespace Patients.Domain.Services.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int id);
        Task<bool> SoftDeleteAsync(int id, string deletedBy);
    }
}