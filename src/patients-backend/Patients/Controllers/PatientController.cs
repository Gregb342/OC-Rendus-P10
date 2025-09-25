using Microsoft.AspNetCore.Mvc;
using Patients.Model.Entities;
using Patients.Services.Interfaces;

namespace Patients.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _patientService.GetAllAsync());
        }

        [HttpGet("{id}")] 
        public async Task<IActionResult> GetById(int id)
        {
            var patient = _patientService.GetByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Patient patient)
        {
            await _patientService.AddAsync(patient);
            return CreatedAtAction(nameof(GetById), new { id = patient.PatientID }, patient);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Patient patient)
        {
            if (id != patient.PatientID) return BadRequest();
            await _patientService.UpdateAsync(patient);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _patientService.DeleteAsync(id);
            return NoContent();
        }
    }
}
