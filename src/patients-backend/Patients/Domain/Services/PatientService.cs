using Patients.Domain.Entities;
using Patients.Domain.Services.Interfaces;
using Patients.DTOs;
using Patients.Infrastructure.Repositories;
using Patients.Infrastructure.Repositories.Interfaces;

namespace Patients.Domain.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<PatientService> _logger;

        public PatientService(
            IPatientRepository patientRepository,
            IAddressRepository addressRepository,
            ILogger<PatientService> logger)
        {
            _patientRepository = patientRepository;
            _addressRepository = addressRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
        {
            try
            {
                IEnumerable<Patient> patients = await _patientRepository.GetAllAsync();
                _logger.LogInformation("Retrieved {Count} patients from repository", patients.Count());

                var patientsDtos = patients.Select(p => new PatientDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    PhoneNumber = p.PhoneNumber,
                    Address = new AddressDto
                    {
                        Id = p.PatientAddress.Id,
                        Street = p.PatientAddress.Street,
                        City = p.PatientAddress.City,
                        PostalCode = p.PatientAddress.PostalCode,
                        Country = p.PatientAddress.Country
                    }
                }).ToList();

                return patientsDtos;
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return null;
            }
        }

        public async Task<PatientDto?> GetPatientByIdAsync(int id)
        {
            try
            {
                Patient patient = await _patientRepository.GetByIdAsync(id);
                _logger.LogInformation("Patient with id {PatientId} query to PatientRepository", id);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with id {PatientId} not found", id);
                    return null;
                }

                _logger.LogInformation("Patient with id {PatientId} retrieved successfully", id);

                _logger.LogInformation("Returning patient DTO for id {PatientId}", id);
                PatientDto patientDto = new PatientDto
                {
                    Id = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    Address = new AddressDto
                    {
                        Id = patient.PatientAddress.Id,
                        Street = patient.PatientAddress.Street,
                        City = patient.PatientAddress.City,
                        PostalCode = patient.PatientAddress.PostalCode,
                        Country = patient.PatientAddress.Country
                    }
                };

                return patientDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with id {PatientId}", id);
                return null;
            }
        }

        public async Task<int> CreatePatientAsync(PatientCreateDto patientCreateDto)
        {
            try
            {
                Patient patient = new Patient
                {
                    FirstName = patientCreateDto.FirstName,
                    LastName = patientCreateDto.LastName,
                    DateOfBirth = patientCreateDto.DateOfBirth,
                    Gender = patientCreateDto.Gender,
                    PhoneNumber = patientCreateDto.PhoneNumber,
                };

                if (patientCreateDto.Address != null)
                {
                    _logger.LogDebug("Processing address for new patient: {City}, {Country}",
                            patientCreateDto.Address.City, patientCreateDto.Address.Country);

                    var addressEntity = new Address
                    {
                        Street = patientCreateDto.Address.Street,
                        City = patientCreateDto.Address.City,
                        PostalCode = patientCreateDto.Address.PostalCode,
                        Country = patientCreateDto.Address.Country
                    };

                    var savedAddress = await _addressRepository.AddAsync(addressEntity);
                    _logger.LogInformation("Address created with id {AddressId}", savedAddress.Id);

                    patient.AddressId = savedAddress.Id;
                    patient.PatientAddress = savedAddress;
                }

                var createdPatient = await _patientRepository.AddAsync(patient);
                _logger.LogInformation("Patient created successfully with id {PatientId}", createdPatient.Id);

                return createdPatient.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return 0;
            }

        }

        public async Task UpdatePatientAsync(int id, PatientUpdateDto patientUpdateDto)
        {
            try
            {
                Patient existingPatient = await _patientRepository.GetByIdAsync(id);

                if (existingPatient != null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found for update", id);
                }

                existingPatient.FirstName = patientUpdateDto.FirstName;
                existingPatient.LastName = patientUpdateDto.LastName;
                existingPatient.DateOfBirth = patientUpdateDto.DateOfBirth;
                existingPatient.Gender = patientUpdateDto.Gender;
                existingPatient.PhoneNumber = patientUpdateDto.PhoneNumber;

                // Process address if provided
                if (patientUpdateDto.Address != null)
                {
                    _logger.LogDebug("Processing address update for patient {PatientId}: {City}, {Country}",
                        id, patientUpdateDto.Address.City, patientUpdateDto.Address.Country);

                    if (existingPatient.Address != null)
                    {
                        // Update existing address
                        _logger.LogDebug("Updating existing address {AddressId} for patient {PatientId}",
                            existingPatient.PatientAddress.Id, id);

                        existingPatient.PatientAddress.Street = patientUpdateDto.Address.Street;
                        existingPatient.PatientAddress.City = patientUpdateDto.Address.City;
                        existingPatient.PatientAddress.PostalCode = patientUpdateDto.Address.PostalCode;
                        existingPatient.PatientAddress.Country = patientUpdateDto.Address.Country;
                        await _addressRepository.UpdateAsync(existingPatient.PatientAddress);

                        _logger.LogInformation("Address {AddressId} updated for patient {PatientId}",
                            existingPatient.PatientAddress.Id, id);
                    }
                    else
                    {
                        // Create new address
                        _logger.LogDebug("Creating new address for patient {PatientId}", id);

                        var newAddress = new Address
                        {
                            Street = patientUpdateDto.Address.Street,
                            City = patientUpdateDto.Address.City,
                            PostalCode = patientUpdateDto.Address.PostalCode,
                            Country = patientUpdateDto.Address.Country
                        };

                        var savedAddress = await _addressRepository.AddAsync(newAddress);
                        _logger.LogInformation("New address {AddressId} created for patient {PatientId}",
                            savedAddress.Id, id);

                        existingPatient.AddressId = savedAddress.Id;
                        existingPatient.PatientAddress = savedAddress;
                    }
                }

                await _patientRepository.UpdateAsync(existingPatient);
            } catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
            }

        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            bool result = await _patientRepository.DeleteAsync(id);

            if (!result)
            {
                _logger.LogWarning("Patient with id {PatientId} not found for deletion", id);
                return false;
            }

            return true;
        }
    }
}