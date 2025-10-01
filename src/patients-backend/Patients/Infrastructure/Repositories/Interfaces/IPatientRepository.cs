using Patients.Domain.Entities;

namespace Patients.Infrastructure.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient> AddAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task<bool> DeleteAsync(int id);
        Task<bool> PatientExistsAsync(int id);
    }
}