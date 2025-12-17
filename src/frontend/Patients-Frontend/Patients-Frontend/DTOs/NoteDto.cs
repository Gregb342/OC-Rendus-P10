namespace Patients_Frontend.DTOs
{
    public class NoteDto
    {
        public string Id { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string NoteContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
