using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Patients.Domain.Entities;

namespace Patients.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filters pour exclure automatiquement les entités supprimées logiquement
            modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Address>().HasQueryFilter(a => !a.IsDeleted);

            // Configuration de la relation Patient-Address
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.PatientAddress)
                .WithMany(a => a.Patients)
                .HasForeignKey(p => p.AddressId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed de données de test (mise à jour avec les nouveaux champs)
            SeedData(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUser;
                        break;

                    case EntityState.Modified:
                        // Ne pas écraser CreatedAt et CreatedBy pour les modifications
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        entry.Property(e => e.CreatedBy).IsModified = false;

                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = currentUser;
                        break;
                }
            }
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed des adresses de test avec les nouveaux champs d'audit
            modelBuilder.Entity<Address>().HasData(
                new Address
                {
                    Id = 1,
                    Street = "123 Main St",
                    City = "New York",
                    PostalCode = "10001",
                    Country = "USA",
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                },
                new Address
                {
                    Id = 2,
                    Street = "456 Park Ave",
                    City = "Boston",
                    PostalCode = "02108",
                    Country = "USA",
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                },
                new Address
                {
                    Id = 3,
                    Street = "789 Maple Rd",
                    City = "Chicago",
                    PostalCode = "60007",
                    Country = "USA",
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                },
                new Address
                {
                    Id = 4,
                    Street = "321 Pine St",
                    City = "Seattle",
                    PostalCode = "98101",
                    Country = "USA",
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                }
            );

            // Seed des patients de test avec les nouveaux champs d'audit
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "TestNone",
                    DateOfBirth = new DateTime(1966,12,31),
                    Gender = "Female",
                    PhoneNumber = "100-222-3333",
                    AddressId = 1,
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                },
                new Patient
                {
                    Id = 2,
                    FirstName = "Test",
                    LastName = "TestBorderline",
                    DateOfBirth = new DateTime(1945,06,24),
                    Gender = "Male",
                    PhoneNumber = "200-333-4444",
                    AddressId = 2,
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                },
                new Patient
                {
                    Id = 3,
                    FirstName = "Test",
                    LastName = "TestInDanger",
                    DateOfBirth = new DateTime(2004,06,18),
                    Gender = "Male",
                    PhoneNumber = "300-444-5555",
                    AddressId = 3,
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                },
                new Patient
                {
                    Id = 4,
                    FirstName = "Test",
                    LastName = "TestEarlyOnset",
                    DateOfBirth = new DateTime(2002,06,28),
                    Gender = "Female",
                    PhoneNumber = "400-555-6666",
                    AddressId = 4,
                    CreatedAt = seedDate,
                    CreatedBy = "System",
                    IsDeleted = false
                }
            );
        }
    }
}