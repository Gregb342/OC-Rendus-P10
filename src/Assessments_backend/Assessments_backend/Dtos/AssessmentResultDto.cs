using Assessments_backend.Enums;

namespace Assessments_backend.Dtos
{
    public class AssessmentResultDto
    {
        public int PatientId { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public int TriggerCount { get; set; }
        public List<string> MatchedTrigers { get; set; } = new();
        public DateTimeOffset AssessedAt { get; set; }
    }
}
