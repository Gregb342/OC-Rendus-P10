using Patients.Domain.Entities;

namespace Patients.Infrastructure.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllAsync();
        Task<Address?> GetByIdAsync(int id);
        Task<Address> AddAsync(Address address);
        Task UpdateAsync(Address address);
        Task<bool> DeleteAsync(int id);
    }
}