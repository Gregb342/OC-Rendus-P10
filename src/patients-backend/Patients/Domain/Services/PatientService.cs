using Patients.Domain.Entities;
using Patients.Domain.Services.Interfaces;
using Patients.Infrastructure.Repositories.Interfaces;

namespace Patients.Domain.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _patientRepository.GetAllAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _patientRepository.GetByIdAsync(id);
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            return await _patientRepository.AddAsync(patient);
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            await _patientRepository.UpdateAsync(patient);
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            return await _patientRepository.DeleteAsync(id);
        }
    }
}