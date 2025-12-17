namespace Assessments_backend.Dtos
{
    public class PatientDto
    {
        public int Id { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
    }
}
