namespace Patients_Frontend.DTOs
{
    public class AssessmentResultDto
    {
        public int PatientId { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public int TriggerCount { get; set; }
        public List<string> MatchedTrigers { get; set; } = new();
        public DateTimeOffset AssessedAt { get; set; }
    }

    public enum RiskLevel
    {
        None = 0,
        Borderline = 1,
        InDanger = 2,
        EarlyOnset = 3
    }
}
