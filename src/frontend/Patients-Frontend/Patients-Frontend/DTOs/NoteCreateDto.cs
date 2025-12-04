using System.ComponentModel.DataAnnotations;

namespace Patients_Frontend.DTOs
{
    public class NoteCreateDto
    {
        [Required(ErrorMessage = "L'ID du patient est requis")]
        public int PatientId { get; set; }
        [Required(ErrorMessage = "Le nom du médecin est requis")]
        public string DoctorName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le contenu de la note est requis")]
        public string NoteContent { get; set; } = string.Empty;
    }
}
