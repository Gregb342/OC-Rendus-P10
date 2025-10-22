using Patients.Domain.Entities;

namespace Patients.Domain.Services.Interfaces
{
    public interface ISoftDeleteService<T> where T : class, ISoftDeletable
    {
        /// <summary>
        /// Effectue une suppression logique d'une entit�
        /// </summary>
        Task<bool> SoftDeleteAsync(int id, string deletedBy);

        /// <summary>
        /// Restaure une entit� supprim�e logiquement
        /// </summary>
        Task<bool> RestoreAsync(int id);

        /// <summary>
        /// R�cup�re toutes les entit�s supprim�es logiquement
        /// </summary>
        Task<IEnumerable<T>> GetDeletedAsync();

        /// <summary>
        /// Effectue une suppression d�finitive (� utiliser avec pr�caution)
        /// </summary>
        Task<bool> HardDeleteAsync(int id);
    }
}