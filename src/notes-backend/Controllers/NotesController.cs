using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using notes_backend.Domain.Services.Interfaces;
using notes_backend.DTOs;

namespace notes_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NotesController> _logger;

        public NotesController(INoteService noteService, ILogger<NotesController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }

        // GET: api/notes
        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            try
            {
                var notes = await _noteService.GetAllNotesAsync();
                return Ok(notes);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de toutes les notes");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/notes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(string id)
        {
            try
            {
                var note = await _noteService.GetNoteByIdAsync(id);
                if (note == null)
                    return NotFound(new { message = $"Note avec l'ID {id} introuvable" });

                return Ok(note);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la note {NoteId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/notes/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetNotesByPatientId(int patientId)
        {
            try
            {
                var notes = await _noteService.GetNotesByPatientIdAsync(patientId);
                return Ok(notes);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des notes du patient {PatientId}", patientId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: api/notes
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreateDto noteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var note = await _noteService.CreateNoteAsync(noteDto);
                return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Erreur lors de la création d'une note");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT: api/notes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(string id, [FromBody] NoteUpdateDto noteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updated = await _noteService.UpdateNoteAsync(id, noteDto);
                if (!updated)
                    return NotFound(new { message = $"Note avec l'ID {id} introuvable" });

                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la note {NoteId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/notes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            try
            {
                var deleted = await _noteService.DeleteNoteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Note avec l'ID {id} introuvable" });

                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la note {NoteId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}