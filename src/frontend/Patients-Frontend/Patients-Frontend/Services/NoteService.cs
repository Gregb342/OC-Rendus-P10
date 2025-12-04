using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Services
{
    public class NoteService : INoteService
    {
        private readonly IApiService _apiService;

        public NoteService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<NoteDto>> GetNotesByPatientIdAsync(int patientId)
        {
            var notes = await _apiService.GetAsync<List<NoteDto>>($"/notes/patient/{patientId}");
            return notes ?? new List<NoteDto>();
        }

        public async Task<NoteDto?> GetNoteByIdAsync(int noteId)
        {
            var note = await _apiService.GetAsync<NoteDto>($"/notes/{noteId}");
            return note;
        }

        public async Task<NoteDto> CreateNoteAsync(NoteCreateDto noteCreateDto)
        {
            var result = await _apiService.PostAsync<NoteDto>("/notes", noteCreateDto);
            return result ?? throw new InvalidOperationException("Erreur lors de la création de la note");
        }

    }
}
