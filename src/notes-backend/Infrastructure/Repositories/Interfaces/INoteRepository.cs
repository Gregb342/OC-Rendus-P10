using notes_backend.Domain.Entities;

namespace notes_backend.Infrastructure.Repositories.Interfaces
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetAllAsync();

        Task<Note?> GetByIdAsync(string id);

        Task<IEnumerable<Note>> GetByPatientIdAsync(int patientId);

        Task<Note> CreateAsync(Note note);

        Task<bool> UpdateAsync(string id, Note note);

        Task<bool> DeleteAsync(string id);
    }
}
