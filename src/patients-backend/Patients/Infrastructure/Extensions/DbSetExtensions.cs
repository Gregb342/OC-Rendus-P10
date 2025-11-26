using Microsoft.EntityFrameworkCore;
using Patients.Domain.Entities;

namespace Patients.Infrastructure.Extensions
{
    public static class DbSetExtensions
    {
        /// <summary>
        /// Extension générique pour effectuer une suppression logique sur un DbSet
        /// </summary>
        public static async Task<bool> SoftDeleteEntityAsync<T>(
            this DbSet<T> dbSet,
            int id,
            string deletedBy
        ) where T : class, ISoftDeletable
        {
            var entity = await dbSet
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);

            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = deletedBy;

            return true;
        }
    }
}
