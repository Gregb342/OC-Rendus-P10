using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Patients.Domain.Entities;

namespace Patients.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation Patient-Address
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.PatientAddress)
                .WithMany(a => a.Patients)
                .HasForeignKey(p => p.AddressId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Seed de données de test
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed des adresses de test
            modelBuilder.Entity<Address>().HasData(
                new Address { Id = 1, Street = "123 Main St", City = "New York", PostalCode = "10001", Country = "USA" },
                new Address { Id = 2, Street = "456 Park Ave", City = "Boston", PostalCode = "02108", Country = "USA" },
                new Address { Id = 3, Street = "789 Maple Rd", City = "Chicago", PostalCode = "60007", Country = "USA" },
                new Address { Id = 4, Street = "321 Pine St", City = "Seattle", PostalCode = "98101", Country = "USA" }
            );

            // Seed des patients de test
            modelBuilder.Entity<Patient>().HasData(
                new Patient 
                { 
                    Id = 1, 
                    FirstName = "John", 
                    LastName = "Doe", 
                    DateOfBirth = new DateTime(1985, 6, 15), 
                    Gender = "Male",
                    PhoneNumber = "555-123-4567",
                    AddressId = 1
                },
                new Patient 
                { 
                    Id = 2, 
                    FirstName = "Jane", 
                    LastName = "Smith", 
                    DateOfBirth = new DateTime(1990, 3, 22), 
                    Gender = "Female",
                    PhoneNumber = "555-234-5678",
                    AddressId = 2
                },
                new Patient 
                { 
                    Id = 3, 
                    FirstName = "Michael", 
                    LastName = "Brown", 
                    DateOfBirth = new DateTime(1978, 9, 8), 
                    Gender = "Male",
                    PhoneNumber = "555-345-6789",
                    AddressId = 3
                },
                new Patient 
                { 
                    Id = 4, 
                    FirstName = "Emily", 
                    LastName = "Johnson", 
                    DateOfBirth = new DateTime(1995, 12, 30), 
                    Gender = "Female",
                    PhoneNumber = "555-456-7890",
                    AddressId = 4
                }
            );
        }
    }
}