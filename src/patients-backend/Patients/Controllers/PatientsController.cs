using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            var traceId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["User"] = User?.Identity?.Name ?? "Anonymous"
            }))
                try
            {
                _logger.LogInformation("Handling GET /api/patients");
                
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogDebug("Authorization header present: {HasAuthHeader}", !string.IsNullOrEmpty(authHeader));

                var patients = await _patientService.GetAllPatientsAsync();
                _logger.LogInformation("Retrieved {Count} patients from service", patients.Count());

                var patientDtos = patients.Select(p => new PatientDto
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

                _logger.LogInformation("Returning {Count} patients as DTOs", patientDtos.Count);
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
            var traceId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["User"] = User?.Identity?.Name ?? "Anonymous",
                ["PatientId"] = id  
            }))
            try
            {
                _logger.LogInformation("Handling GET /api/patients/{PatientId}", id);
                
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogDebug("Authorization header present: {HasAuthHeader}", !string.IsNullOrEmpty(authHeader));

                var patient = await _patientService.GetPatientByIdAsync(id);
                _logger.LogDebug("Patient service query completed for id {PatientId}", id);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with id {PatientId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Patient with id {PatientId} retrieved successfully", id);

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

                _logger.LogInformation("Returning patient DTO for id {PatientId}", id);
                return Ok(patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with id {PatientId}", id);
                return StatusCode(500, "An error occurred while retrieving the patient");
            }
        }

        // POST: api/patients
        [HttpPost]
        public async Task<ActionResult<PatientDto>> CreatePatient(PatientCreateDto patientCreateDto)
        {
            var traceId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["User"] = User?.Identity?.Name ?? "Anonymous"
            }))
            try
            {
                _logger.LogInformation("Handling POST /api/patients");
                _logger.LogDebug("Creating patient: {FirstName} {LastName}, DOB: {DateOfBirth}", 
                    patientCreateDto.FirstName, patientCreateDto.LastName, patientCreateDto.DateOfBirth);
                
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogDebug("Authorization header present: {HasAuthHeader}", !string.IsNullOrEmpty(authHeader));

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
                    
                    patientEntity.AddressId = savedAddress.Id;
                    patientEntity.PatientAddress = savedAddress;
                }

                // Save the patient
                var createdPatient = await _patientService.CreatePatientAsync(patientEntity);
                _logger.LogInformation("Patient created successfully with id {PatientId}", createdPatient.Id);

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

                _logger.LogInformation("Returning created patient DTO with id {PatientId}", createdPatient.Id);
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
            var traceId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["User"] = User?.Identity?.Name ?? "Anonymous",
                ["PatientId"] = id
            }))
            try
            {
                _logger.LogInformation("Handling PUT /api/patients/{PatientId}", id);
                _logger.LogDebug("Updating patient {PatientId}: {FirstName} {LastName}, DOB: {DateOfBirth}", 
                    id, patientUpdateDto.FirstName, patientUpdateDto.LastName, patientUpdateDto.DateOfBirth);
                
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogDebug("Authorization header present: {HasAuthHeader}", !string.IsNullOrEmpty(authHeader));

                var existingPatient = await _patientService.GetPatientByIdAsync(id);

                if (existingPatient == null)
                {
                    _logger.LogWarning("Patient with id {PatientId} not found for update", id);
                    return NotFound();
                }

                _logger.LogDebug("Existing patient found, updating properties for id {PatientId}", id);

                // Update patient properties
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
                    
                    if (existingPatient.PatientAddress != null)
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

                await _patientService.UpdatePatientAsync(existingPatient);
                _logger.LogInformation("Patient {PatientId} updated successfully", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with id {PatientId}", id);
                return StatusCode(500, "An error occurred while updating the patient");
            }
        }

        // DELETE: api/patients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var traceId = Guid.NewGuid();
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["User"] = User?.Identity?.Name ?? "Anonymous",
                ["PatientId"] = id
            }))
            try
            {
                _logger.LogInformation("Handling DELETE /api/patients/{PatientId}", id);
                
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogDebug("Authorization header present: {HasAuthHeader}", !string.IsNullOrEmpty(authHeader));

                _logger.LogDebug("Attempting to delete patient with id {PatientId}", id);
                var result = await _patientService.DeletePatientAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Patient with id {PatientId} not found for deletion", id);
                    return NotFound();
                }

                _logger.LogInformation("Patient {PatientId} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient with id {PatientId}", id);
                return StatusCode(500, "An error occurred while deleting the patient");
            }
        }
    }
}