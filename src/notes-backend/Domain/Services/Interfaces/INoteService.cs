using notes_backend.Domain.Entities;
using notes_backend.DTOs;

namespace notes_backend.Domain.Services.Interfaces
{
    public interface INoteService
    {
        Task<IEnumerable<NoteDto>> GetAllNotesAsync();

        Task<NoteDto?> GetNoteByIdAsync(string id);

        Task<IEnumerable<NoteDto>> GetNotesByPatientIdAsync(int patientId);

        Task<NoteDto> CreateNoteAsync(NoteCreateDto noteCreateDto);

        Task<bool> UpdateNoteAsync(string id, NoteUpdateDto noteUpdateDto);

        Task<bool> DeleteNoteAsync(string id);
    }
}
