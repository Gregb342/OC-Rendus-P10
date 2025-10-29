using System.ComponentModel.DataAnnotations;

namespace Patients_Frontend.DTOs
{
    public class AddressCreateDto
    {
        [MaxLength(100)]
        public string Street { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;
    }
}
