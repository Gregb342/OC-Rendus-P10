using System.ComponentModel.DataAnnotations;

namespace notes_backend.DTOs
{
    public class NoteUpdateDto
    {
        [Required(ErrorMessage ="Le nom du médecin est requis")]
        public string DoctorName { get; set; } = string.Empty;
        [Required(ErrorMessage ="Le contenu de la note est requis")]
        public string NoteContent { get; set; } = string.Empty;
    }
}
