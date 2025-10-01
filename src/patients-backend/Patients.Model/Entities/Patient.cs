using System;
using System.ComponentModel.DataAnnotations;

namespace Patients.Model.Entities
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }
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
        [MaxLength(250)]
        public string? Address { get; set; }
        [Phone]
        [MaxLength(20)]
        public string? PhoneNUmber { get; set; }
    }
}
