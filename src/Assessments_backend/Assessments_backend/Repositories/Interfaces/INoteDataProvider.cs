using Assessments_backend.Dtos;

namespace Assessments_backend.Repositories.Interfaces
{
    public interface INoteDataProvider
    {
        Task<IEnumerable<NoteDto>> GetNotesByPatientIdAsync(int patientId);
    }
}
