using MongoDB.Bson;
using MongoDB.Driver;
using notes_backend.Domain.Entities;
using notes_backend.Domain.Services.Interfaces;
using notes_backend.DTOs;
using notes_backend.Infrastructure.Repositories.Interfaces;

namespace notes_backend.Domain.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;

        public NoteService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<IEnumerable<NoteDto>> GetAllNotesAsync()
        {
            try
            {
                var notes = await _noteRepository.GetAllAsync();
                return notes.Select(MapToDto);
            }
            catch (MongoException ex)
            {
                throw new ApplicationException($"Erreur à la récupération des notes dans mongoDb", ex);
            }

        }

        public async Task<NoteDto?> GetNoteByIdAsync(string id)
        {
            try
            {
                var note = await _noteRepository.GetByIdAsync(id);

                if (note == null) return null;

                NoteDto noteDto = MapToDto(note);
                return noteDto;
            }
            catch (MongoException ex)
            {
                throw new ApplicationException($"Erreur à la récupération de la note {id} dans mongoDb", ex);
            }
        }

        public async Task<IEnumerable<NoteDto>> GetNotesByPatientIdAsync(int patientId)
        {
            try
            {
                var notes = await _noteRepository.GetByPatientIdAsync(patientId);
                return notes.Select(MapToDto);
            }
            catch (MongoException ex)
            {
                throw new ApplicationException($"Erreur à la récupération de la note {patientId} dans mongoDb", ex);
            }
        }

        public async Task<NoteDto> CreateNoteAsync(NoteCreateDto noteCreateDto)
        {
            try
            {
                var note = new Note
                {
                    PatientId = noteCreateDto.PatientId,
                    DoctorName = noteCreateDto.DoctorName,
                    NoteContent = noteCreateDto.NoteContent,
                    CreatedAt = DateTime.UtcNow
                };

                var createdNote = await _noteRepository.CreateAsync(note);
                return MapToDto(createdNote);
            }
            catch (MongoException ex)
            {
                throw new ApplicationException($"Erreur à la création de la note dans mongoDb", ex);
            }
        }

        public async Task<bool> UpdateNoteAsync(string id, NoteUpdateDto noteUpdateDto)
        {
            try
            {
                var existingNote = await _noteRepository.GetByIdAsync(id);

                if (existingNote == null) return false;

                existingNote.DoctorName = noteUpdateDto.DoctorName;
                existingNote.NoteContent = noteUpdateDto.NoteContent;

                return await _noteRepository.UpdateAsync(id, existingNote);
            }
            catch (MongoException ex)
            {
                throw new ApplicationException($"Erreur à la mise à jour de la note {id} dans mongoDb", ex);
            }
        }

        public async Task<bool> DeleteNoteAsync(string id)
        {
            try
            {
                return await _noteRepository.DeleteAsync(id);
            }
            catch (MongoException ex)
            {
                throw new ApplicationException($"Erreur à la suppression de la note {id} dans mongoDb", ex);
            }

        }

        private static NoteDto MapToDto(Note note)
        {
            return new NoteDto
            {
                Id = note.Id,
                PatientId = note.PatientId,
                DoctorName = note.DoctorName,
                NoteContent = note.NoteContent,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }

    }
}
