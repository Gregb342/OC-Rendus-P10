using MongoDB.Driver;
using notes_backend.Data;
using notes_backend.Domain.Entities;
using notes_backend.Infrastructure.Repositories.Interfaces;

namespace notes_backend.Infrastructure.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly IMongoCollection<Note> _notes;

        public NoteRepository(MongoDbContext context)
        {
            _notes = context.Notes;
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            return await _notes.Find(_ => true).ToListAsync();
        }

        public async Task<Note?> GetByIdAsync(string id)
        {
            return await _notes.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Note>> GetByPatientIdAsync(int patientId)
        {
            return await _notes.Find(n => n.PatientId == patientId).ToListAsync();
        }

        public async Task<Note> CreateAsync(Note note)
        {
            await _notes.InsertOneAsync(note);
            return note;
        }

        public async Task<bool> UpdateAsync(string id, Note note)
        {
            note.UpdatedAt = DateTime.UtcNow;
            var result = await _notes.ReplaceOneAsync(n => n.Id == id, note);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _notes.DeleteOneAsync(n => n.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
