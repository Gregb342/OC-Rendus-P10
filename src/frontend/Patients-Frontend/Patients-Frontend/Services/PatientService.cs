using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Services
{
    public class PatientService : IPatientService
    {
        private readonly IApiService _apiService;

        public PatientService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<PatientDto>> GetAllPatientsAsync()
        {
            var patients = await _apiService.GetAsync<List<PatientDto>>("/patients");
            return patients ?? new List<PatientDto>();
        }

        public async Task<PatientDto?> GetPatientByIdAsync(int id)
        {
            return await _apiService.GetAsync<PatientDto>($"/patients/{id}");
        }

        public async Task<PatientDto> CreatePatientAsync(PatientCreateDto patientCreateDto)
        {
            var result = await _apiService.PostAsync<PatientDto>("/patients", patientCreateDto);
            return result ?? throw new InvalidOperationException("Erreur lors de la création du patient");
        }

        public async Task<PatientDto> UpdatePatientAsync(int id, PatientUpdateDto patientUpdateDto)
        {
            var result = await _apiService.PutAsync<PatientDto>($"/patients/{id}", patientUpdateDto);
            return result ?? throw new InvalidOperationException("Erreur lors de la mise à jour du patient");
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            return await _apiService.DeleteAsync($"/patients/{id}");
        }
    }
}