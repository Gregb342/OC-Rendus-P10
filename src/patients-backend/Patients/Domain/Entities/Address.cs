using System.ComponentModel.DataAnnotations;

namespace Patients.Domain.Entities
{
    public class Address : BaseEntity
    {
        [MaxLength(100)]
        public string Street { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;
      
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }
}