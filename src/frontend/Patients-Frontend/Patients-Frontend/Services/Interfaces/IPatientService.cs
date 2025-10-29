using Patients_Frontend.DTOs;

namespace Patients_Frontend.Services.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto?> GetPatientByIdAsync(int id);
        Task<PatientDto> CreatePatientAsync(PatientCreateDto patientCreateDto);
        Task<PatientDto> UpdatePatientAsync(int id, PatientUpdateDto patientUpdateDto);
        Task<bool> DeletePatientAsync(int id);
    }
}
