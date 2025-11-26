using System.ComponentModel.DataAnnotations;

namespace Patients.Domain.Entities
{
    public class Patient : BaseEntity
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

        [MaxLength(255)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        public virtual Address? PatientAddress { get; set; }
        
        // Créé pour respecter la normalisation 3NF
        public int? AddressId { get; set; }
    }
}