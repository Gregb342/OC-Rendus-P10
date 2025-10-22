using Patients.Domain.Entities;

namespace Patients.Domain.Services.Interfaces
{
    public interface ISoftDeleteService<T> where T : class, ISoftDeletable
    {
        /// <summary>
        /// Effectue une suppression logique d'une entité
        /// </summary>
        Task<bool> SoftDeleteAsync(int id, string deletedBy);

        /// <summary>
        /// Restaure une entité supprimée logiquement
        /// </summary>
        Task<bool> RestoreAsync(int id);

        /// <summary>
        /// Récupère toutes les entités supprimées logiquement
        /// </summary>
        Task<IEnumerable<T>> GetDeletedAsync();

        /// <summary>
        /// Effectue une suppression définitive (à utiliser avec précaution)
        /// </summary>
        Task<bool> HardDeleteAsync(int id);
    }
}