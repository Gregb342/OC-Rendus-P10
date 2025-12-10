using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Assessments_backend.Services.Interfaces;
using Assessments_backend.Dtos;

namespace Assessments_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssessmentController
    {
        public readonly IAssessmentService _assessmentService;

        public AssessmentController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }

        // GET: api/assessment/patient/5

        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentResultDto>> GetAssessment(int patientId)
        {
            if (patientId <= 0)
            {
                return null;
            }

            var assessment = await _assessmentService.GetAssessment(patientId);

            if (assessment == null)
            {
                return null;
            }

            return assessment;
        }

    }
}
