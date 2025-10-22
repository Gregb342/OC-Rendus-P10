using Microsoft.EntityFrameworkCore;
using Patients.Data;
using Patients.Domain.Entities;
using Patients.Domain.Services.Interfaces;
using Patients.DTOs;
using Patients.Infrastructure.Extensions;
using Patients.Infrastructure.Repositories.Interfaces;

namespace Patients.Domain.Services
{
    public class PatientService : IPatientService, ISoftDeleteService<Patient>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PatientService> _logger;

        public PatientService(
            IPatientRepository patientRepository,
            IAddressRepository addressRepository,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PatientService> logger)
        {
            _patientRepository = patientRepository;
            _addressRepository = addressRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
        {
            try
            {
                var patients = await _patientRepository.GetAllAsync();
                _logger.LogInformation("Retrieved {Count} patients from repository", patients.Count());

                var patientsDtos = patients.Select(p => new PatientDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    PhoneNumber = p.PhoneNumber,
                    Address = p.PatientAddress != null ? new AddressDto
                    {
                        Id = p.PatientAddress.Id,
                        Street = p.PatientAddress.Street,
                        City = p.PatientAddress.City,
                        PostalCode = p.PatientAddress.PostalCode,
                        Country = p.PatientAddress.Country
                    } : null
                }).ToList();

                return patientsDtos;
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return new List<PatientDto>();
            }
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                _logger.LogInformation("Patient with id {PatientId} query to PatientRepository", id);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with id {PatientId} not found", id);
                    return null;
                }

                _logger.LogInformation("Patient with id {PatientId} retrieved successfully", id);
                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with id {PatientId}", id);
                return null;
            }
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            try
            {
                var createdPatient = await _patientRepository.AddAsync(patient);
                _logger.LogInformation("Patient created successfully with id {PatientId}", createdPatient.Id);
                return createdPatient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                throw;
            }
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            try
            {
                await _patientRepository.UpdateAsync(patient);
                _logger.LogInformation("Patient with id {PatientId} updated successfully", patient.Id);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with id {PatientId}", patient.Id);
                throw;
            }
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            try
            {
                var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
                var result = await SoftDeleteAsync(id, currentUser);
                
                if (result)
                {
                    _logger.LogInformation("Patient with id {PatientId} soft deleted by {User}", id, currentUser);
                }
                else
                {
                    _logger.LogWarning("Patient with id {PatientId} not found for deletion", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient with id {PatientId}", id);
                return false;
            }
        }

        // Implémentation de ISoftDeleteService<Patient>
        public async Task<bool> SoftDeleteAsync(int id, string deletedBy)
        {
            try
            {
                var result = await _context.Patients.SoftDeleteAsync(id, deletedBy);
                if (result)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Patient with id {PatientId} soft deleted by {DeletedBy}", id, deletedBy);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting patient with id {PatientId}", id);
                return false;
            }
        }

        public async Task<bool> RestoreAsync(int id)
        {
            try
            {
                var result = await _context.Patients.RestoreAsync(id);
                if (result)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Patient with id {PatientId} restored successfully", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring patient with id {PatientId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<Patient>> GetDeletedAsync()
        {
            try
            {
                var deletedPatients = await _context.Patients
                    .OnlyDeleted()
                    .Include(p => p.PatientAddress)
                    .ToListAsync();
        
                _logger.LogInformation("Retrieved {Count} deleted patients", deletedPatients.Count);
                return deletedPatients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deleted patients");
                return new List<Patient>();
            }
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            try
            {
                // Récupérer l'entité même si elle est supprimée logiquement
                var patient = await _context.Patients
                    .IncludeDeleted()
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (patient == null)
                {
                    _logger.LogWarning("Patient with id {PatientId} not found for hard deletion", id);
                    return false;
                }

                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            
                _logger.LogWarning("Patient with id {PatientId} permanently deleted from database", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting patient with id {PatientId}", id);
                return false;
            }
        }

        // Méthodes pour récupérer les patients avec différents états
        public async Task<IEnumerable<Patient>> GetAllPatientsIncludingDeletedAsync()
        {
            try
            {
                var allPatients = await _context.Patients
                    .IncludeDeleted()
                    .Include(p => p.PatientAddress)
                    .ToListAsync();
          
                _logger.LogInformation("Retrieved {Count} patients including deleted ones", allPatients.Count);
                return allPatients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all patients including deleted");
                return new List<Patient>();
            }
        }
    }
}