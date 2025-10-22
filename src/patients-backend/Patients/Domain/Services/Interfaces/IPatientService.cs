using Patients.Domain.Entities;
using Patients.DTOs;

namespace Patients.Domain.Services.Interfaces
{
    public interface IPatientService
    {
        // M�thodes existantes mises � jour
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int id);
        
        // Nouvelles m�thodes pour la suppression logique
        Task<bool> SoftDeleteAsync(int id, string deletedBy);
        Task<bool> RestoreAsync(int id);
        Task<IEnumerable<Patient>> GetDeletedAsync();
        Task<bool> HardDeleteAsync(int id);
        Task<IEnumerable<Patient>> GetAllPatientsIncludingDeletedAsync();
    }
}