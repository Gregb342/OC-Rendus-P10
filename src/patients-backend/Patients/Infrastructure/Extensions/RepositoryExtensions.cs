using Microsoft.EntityFrameworkCore;
using Patients.Domain.Entities;

namespace Patients.Infrastructure.Extensions
{
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Inclut les entit�s supprim�es logiquement dans les r�sultats de la requ�te
        /// </summary>
        public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
        {
            return query.IgnoreQueryFilters();
        }

        /// <summary>
        /// Retourne uniquement les entit�s supprim�es logiquement
        /// </summary>
        public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
        {
            return query.IgnoreQueryFilters().Where(e => e.IsDeleted);
        }

        /// <summary>
        /// Effectue une suppression logique sur une entit�
        /// </summary>
        public static async Task<bool> SoftDeleteAsync<T>(this DbSet<T> dbSet, int id, string deletedBy)
            where T : class, ISoftDeletable
        {
            // Utiliser IgnoreQueryFilters pour trouver l'entit� m�me si elle est d�j� supprim�e
            var entity = await dbSet.IgnoreQueryFilters()
              .Where(e => EF.Property<int>(e, "Id") == id)
                 .FirstOrDefaultAsync();

            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = deletedBy;

            return true;
        }

        /// <summary>
        /// Restaure une entit� supprim�e logiquement
        /// </summary>
        public static async Task<bool> RestoreAsync<T>(this DbSet<T> dbSet, int id)
                 where T : class, ISoftDeletable
        {
            var entity = await dbSet.IgnoreQueryFilters()
                .Where(e => EF.Property<int>(e, "Id") == id)
               .FirstOrDefaultAsync();

            if (entity == null) return false;

            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;

            return true;
        }
    }
}