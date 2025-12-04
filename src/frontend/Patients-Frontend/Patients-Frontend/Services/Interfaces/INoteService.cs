using Patients_Frontend.DTOs;

namespace Patients_Frontend.Services.Interfaces
{
    public interface INoteService
    {
        Task<List<NoteDto>> GetNotesByPatientIdAsync(int patientId);
        Task<NoteDto?> GetNoteByIdAsync(int noteId);
        Task<NoteDto> CreateNoteAsync(NoteCreateDto noteCreateDto);

    }
}
