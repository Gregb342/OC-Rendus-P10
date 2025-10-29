using Microsoft.EntityFrameworkCore;
using Patients.Domain.Entities;

namespace Patients.Infrastructure.Extensions
{
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Inclut les entités supprimées logiquement dans les résultats de la requête
        /// </summary>
        public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
        {
            return query.IgnoreQueryFilters();
        }

        /// <summary>
        /// Retourne uniquement les entités supprimées logiquement
        /// </summary>
        public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
        {
            return query.IgnoreQueryFilters().Where(e => e.IsDeleted);
        }

        /// <summary>
        /// Effectue une suppression logique sur une entité
        /// </summary>
        public static async Task<bool> SoftDeleteAsync<T>(this DbSet<T> dbSet, int id, string deletedBy)
            where T : class, ISoftDeletable
        {
            // Utiliser IgnoreQueryFilters pour trouver l'entité même si elle est déjà supprimée
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
        /// Restaure une entité supprimée logiquement
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