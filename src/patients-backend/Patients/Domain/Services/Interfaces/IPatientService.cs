using Patients.Domain.Entities;

namespace Patients.Domain.Services.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int id);
    }
}