using System.ComponentModel.DataAnnotations;

namespace Patients_Frontend.DTOs
{
    public class PatientUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public AddressCreateDto? Address { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
