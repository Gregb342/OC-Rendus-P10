using System.ComponentModel.DataAnnotations;

namespace Patients_Frontend.DTOs
{
    public class NoteCreateDto
    {
        public int PatientId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string NoteContent { get; set; } = string.Empty;
    }
}
