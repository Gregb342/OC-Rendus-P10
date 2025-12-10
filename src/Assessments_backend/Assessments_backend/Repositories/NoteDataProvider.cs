using Assessments_backend.Dtos;
using Assessments_backend.Repositories.Interfaces;

namespace Assessments_backend.Repositories
{
    public class NoteDataProvider : INoteDataProvider
    {
        public Task<IEnumerable<NoteDto>> GetNotesByPatientIdAsync(int patientId)
        {
            throw new NotImplementedException();
        }
    }
}
