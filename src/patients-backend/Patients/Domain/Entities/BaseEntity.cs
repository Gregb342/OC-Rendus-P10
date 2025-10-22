using System.ComponentModel.DataAnnotations;

namespace Patients.Domain.Entities
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string? DeletedBy { get; set; }
    }

    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        string? CreatedBy { get; set; }
        DateTime? LastModifiedAt { get; set; }
        string? LastModifiedBy { get; set; }
    }

    public abstract class BaseEntity : IAuditable, ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        [MaxLength(100)]
        public string? LastModifiedBy { get; set; }

        // Soft delete fields
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        [MaxLength(100)]
        public string? DeletedBy { get; set; }
    }
}