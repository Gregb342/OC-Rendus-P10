using Patients.Domain.Entities;

namespace Patients.Domain.Services.Interfaces
{
    public interface ISoftDeleteService<T> where T : class, ISoftDeletable
    {
        /// <summary>
        /// Effectue une suppression logique d'une entité
        /// </summary>
        Task<bool> SoftDeleteAsync(int id, string deletedBy);
    }
}