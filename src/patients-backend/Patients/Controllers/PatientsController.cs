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

                var patientDtos = await _patientService.GetAllPatientsAsync();
                _logger.LogInformation("Retrieved {Count} patients from service", patientDtos.Count());

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

                var patientDto = await _patientService.GetPatientByIdAsync(id);
                _logger.LogDebug("Patient service query completed for id {PatientId}", id);

                if (patientDto == null)
                {
                    _logger.LogWarning("Patient with id {PatientId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Patient with id {PatientId} retrieved successfully", id);

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
             
                var createdPatientId = await _patientService.CreatePatientAsync(patientCreateDto);
                _logger.LogInformation("Patient created successfully with id {PatientId}", createdPatientId);

                _logger.LogInformation("Returning created patient id {PatientId}", createdPatientId);
                return CreatedAtAction(nameof(GetPatient), new { id = createdPatientId });
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

                await _patientService.UpdatePatientAsync(id, patientUpdateDto);
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