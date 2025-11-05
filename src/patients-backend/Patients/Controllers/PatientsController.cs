using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patients.Domain.Entities;
using Patients.Domain.Services.Interfaces;
using Patients.DTOs;
using Patients.Infrastructure.Repositories.Interfaces;

namespace Patients.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(
            IPatientService patientService,
            IAddressRepository addressRepository,
            ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _addressRepository = addressRepository;
            _logger = logger;
        }

        // GET: api/patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
        {
            try
            {
                _logger.LogInformation("GetPatients called. User: " + User?.Identity?.Name);

                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogInformation($"Authorization header: {authHeader ?? "None"}");

                var patientDtos = await _patientService.GetAllPatientsAsync();
                return Ok(patientDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return StatusCode(500, "An error occurred while retrieving patients");
            }
        }

        // GET: api/patients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetPatient(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);

                if (patient == null)
                {
                    return NotFound();
                }

                var patientDto = new PatientDto
                {
                    Id = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    Address = patient.PatientAddress != null ? new AddressDto
                    {
                        Id = patient.PatientAddress.Id,
                        Street = patient.PatientAddress.Street,
                        City = patient.PatientAddress.City,
                        PostalCode = patient.PatientAddress.PostalCode,
                        Country = patient.PatientAddress.Country
                    } : null
                };

                return Ok(patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient with id {id}");
                return StatusCode(500, "An error occurred while retrieving the patient");
            }
        }

        // POST: api/patients
        [HttpPost]
        public async Task<ActionResult<PatientDto>> CreatePatient(PatientCreateDto patientCreateDto)
        {
            try
            {
                // Create a new patient entity
                var patientEntity = new Patient
                {
                    FirstName = patientCreateDto.FirstName,
                    LastName = patientCreateDto.LastName,
                    DateOfBirth = patientCreateDto.DateOfBirth,
                    Gender = patientCreateDto.Gender,
                    PhoneNumber = patientCreateDto.PhoneNumber
                };

                // Process address if provided
                if (patientCreateDto.Address != null)
                {
                    var addressEntity = new Address
                    {
                        Street = patientCreateDto.Address.Street,
                        City = patientCreateDto.Address.City,
                        PostalCode = patientCreateDto.Address.PostalCode,
                        Country = patientCreateDto.Address.Country
                    };

                    var savedAddress = await _addressRepository.AddAsync(addressEntity);
                    patientEntity.AddressId = savedAddress.Id;
                    patientEntity.PatientAddress = savedAddress;
                }

                // Save the patient
                var createdPatient = await _patientService.CreatePatientAsync(patientEntity);

                // Return the created patient
                var patientDto = new PatientDto
                {
                    Id = createdPatient.Id,
                    FirstName = createdPatient.FirstName,
                    LastName = createdPatient.LastName,
                    DateOfBirth = createdPatient.DateOfBirth,
                    Gender = createdPatient.Gender,
                    PhoneNumber = createdPatient.PhoneNumber,
                    Address = createdPatient.PatientAddress != null ? new AddressDto
                    {
                        Id = createdPatient.PatientAddress.Id,
                        Street = createdPatient.PatientAddress.Street,
                        City = createdPatient.PatientAddress.City,
                        PostalCode = createdPatient.PatientAddress.PostalCode,
                        Country = createdPatient.PatientAddress.Country
                    } : null
                };

                return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.Id }, patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return StatusCode(500, "An error occurred while creating the patient");
            }
        }

        // PUT: api/patients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, PatientUpdateDto patientUpdateDto)
        {
            try
            {
                var existingPatient = await _patientService.GetPatientByIdAsync(id);

                if (existingPatient == null)
                {
                    return NotFound();
                }

                // Update patient properties
                existingPatient.FirstName = patientUpdateDto.FirstName;
                existingPatient.LastName = patientUpdateDto.LastName;
                existingPatient.DateOfBirth = patientUpdateDto.DateOfBirth;
                existingPatient.Gender = patientUpdateDto.Gender;
                existingPatient.PhoneNumber = patientUpdateDto.PhoneNumber;

                // Process address if provided
                if (patientUpdateDto.Address != null)
                {
                    if (existingPatient.PatientAddress != null)
                    {
                        // Update existing address
                        existingPatient.PatientAddress.Street = patientUpdateDto.Address.Street;
                        existingPatient.PatientAddress.City = patientUpdateDto.Address.City;
                        existingPatient.PatientAddress.PostalCode = patientUpdateDto.Address.PostalCode;
                        existingPatient.PatientAddress.Country = patientUpdateDto.Address.Country;
                        await _addressRepository.UpdateAsync(existingPatient.PatientAddress);
                    }
                    else
                    {
                        // Create new address
                        var newAddress = new Address
                        {
                            Street = patientUpdateDto.Address.Street,
                            City = patientUpdateDto.Address.City,
                            PostalCode = patientUpdateDto.Address.PostalCode,
                            Country = patientUpdateDto.Address.Country
                        };

                        var savedAddress = await _addressRepository.AddAsync(newAddress);
                        existingPatient.AddressId = savedAddress.Id;
                        existingPatient.PatientAddress = savedAddress;
                    }
                }

                await _patientService.UpdatePatientAsync(existingPatient);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating patient with id {id}");
                return StatusCode(500, "An error occurred while updating the patient");
            }
        }

        // DELETE: api/patients/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
                var result = await _patientService.DeletePatientAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting patient with id {id}");
                return StatusCode(500, "An error occurred while deleting the patient");
            }
        }                   
       
    }
}