using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using notes_backend.Controllers;
using notes_backend.Domain.Services.Interfaces;
using notes_backend.DTOs;

namespace Notes.UnitTests.Notes.Controllers
{
    public class NotesControllerTests
    {
        private readonly Mock<INoteService> _noteServiceMock;
        private readonly Mock<ILogger<NotesController>> _loggerMock;
        private readonly NotesController _notesController;

        public NotesControllerTests()
        {
            _noteServiceMock = new Mock<INoteService>();
            _loggerMock = new Mock<ILogger<NotesController>>();

            _notesController = new NotesController(
                _noteServiceMock.Object,
                _loggerMock.Object);
        }

        #region Tests GetAllNotes

        [Fact]
        public async Task GetAllNotes_WhenNotesExist_ReturnsOkWithNotes()
        {
            // Arrange
            var notes = new List<NoteDto>
            {
                new NoteDto
                {
                    Id = "1",
                    PatientId = 1,
                    DoctorName = "Dr. Test",
                    NoteContent = "Test content",
                    CreatedAt = DateTime.UtcNow
                },
                new NoteDto
                {
                    Id = "2",
                    PatientId = 2,
                    DoctorName = "Dr. Test 2",
                    NoteContent = "Test content 2",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _noteServiceMock.Setup(service => service.GetAllNotesAsync())
                .ReturnsAsync(notes);

            // Act
            var result = await _notesController.GetAllNotes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedNotes = Assert.IsAssignableFrom<IEnumerable<NoteDto>>(okResult.Value);
            Assert.Equal(2, returnedNotes.Count());
            _noteServiceMock.Verify(service => service.GetAllNotesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllNotes_WhenNoNotesExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyNotes = new List<NoteDto>();

            _noteServiceMock.Setup(service => service.GetAllNotesAsync())
                .ReturnsAsync(emptyNotes);

            // Act
            var result = await _notesController.GetAllNotes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedNotes = Assert.IsAssignableFrom<IEnumerable<NoteDto>>(okResult.Value);
            Assert.Empty(returnedNotes);
            _noteServiceMock.Verify(service => service.GetAllNotesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllNotes_WhenApplicationExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            _noteServiceMock.Setup(service => service.GetAllNotesAsync())
                .ThrowsAsync(new ApplicationException("Test error"));

            // Act
            var result = await _notesController.GetAllNotes();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _noteServiceMock.Verify(service => service.GetAllNotesAsync(), Times.Once);
        }

        #endregion

        #region Tests GetNoteById

        [Fact]
        public async Task GetNoteById_WhenNoteExists_ReturnsOkWithNote()
        {
            // Arrange
            var note = new NoteDto
            {
                Id = "test-id",
                PatientId = 1,
                DoctorName = "Dr. Test",
                NoteContent = "Test content",
                CreatedAt = DateTime.UtcNow
            };

            _noteServiceMock.Setup(service => service.GetNoteByIdAsync("test-id"))
                .ReturnsAsync(note);

            // Act
            var result = await _notesController.GetNoteById("test-id");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedNote = Assert.IsType<NoteDto>(okResult.Value);
            Assert.Equal(note.Id, returnedNote.Id);
            _noteServiceMock.Verify(service => service.GetNoteByIdAsync("test-id"), Times.Once);
        }

        [Fact]
        public async Task GetNoteById_WhenNoteDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _noteServiceMock.Setup(service => service.GetNoteByIdAsync("non-existent-id"))
                .ReturnsAsync((NoteDto?)null);

            // Act
            var result = await _notesController.GetNoteById("non-existent-id");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            _noteServiceMock.Verify(service => service.GetNoteByIdAsync("non-existent-id"), Times.Once);
        }

        [Fact]
        public async Task GetNoteById_WhenApplicationExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            _noteServiceMock.Setup(service => service.GetNoteByIdAsync("test-id"))
                .ThrowsAsync(new ApplicationException("Test error"));

            // Act
            var result = await _notesController.GetNoteById("test-id");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _noteServiceMock.Verify(service => service.GetNoteByIdAsync("test-id"), Times.Once);
        }

        #endregion

        #region Tests GetNotesByPatientId

        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        [InlineData(99, 0)]
        public async Task GetNotesByPatientId_ReturnsOkWithCorrectCount(int patientId, int expectedCount)
        {
            // Arrange
            var allNotes = new List<NoteDto>
            {
                new NoteDto { Id = "1", PatientId = 1, DoctorName = "Dr. A", NoteContent = "Note 1", CreatedAt = DateTime.UtcNow },
                new NoteDto { Id = "2", PatientId = 1, DoctorName = "Dr. B", NoteContent = "Note 2", CreatedAt = DateTime.UtcNow },
                new NoteDto { Id = "3", PatientId = 2, DoctorName = "Dr. C", NoteContent = "Note 3", CreatedAt = DateTime.UtcNow }
            };

            var notesForPatient = allNotes.Where(n => n.PatientId == patientId).ToList();

            _noteServiceMock.Setup(service => service.GetNotesByPatientIdAsync(patientId))
                .ReturnsAsync(notesForPatient);

            // Act
            var result = await _notesController.GetNotesByPatientId(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedNotes = Assert.IsAssignableFrom<IEnumerable<NoteDto>>(okResult.Value);
            Assert.Equal(expectedCount, returnedNotes.Count());
            _noteServiceMock.Verify(service => service.GetNotesByPatientIdAsync(patientId), Times.Once);
        }

        [Fact]
        public async Task GetNotesByPatientId_WhenApplicationExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            _noteServiceMock.Setup(service => service.GetNotesByPatientIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new ApplicationException("Test error"));

            // Act
            var result = await _notesController.GetNotesByPatientId(1);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _noteServiceMock.Verify(service => service.GetNotesByPatientIdAsync(1), Times.Once);
        }

        #endregion

        #region Tests CreateNote

        [Fact]
        public async Task CreateNote_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var noteCreateDto = new NoteCreateDto
            {
                PatientId = 1,
                DoctorName = "Dr. Test",
                NoteContent = "Test content"
            };

            var createdNote = new NoteDto
            {
                Id = "new-id",
                PatientId = noteCreateDto.PatientId,
                DoctorName = noteCreateDto.DoctorName,
                NoteContent = noteCreateDto.NoteContent,
                CreatedAt = DateTime.UtcNow
            };

            _noteServiceMock.Setup(service => service.CreateNoteAsync(noteCreateDto))
                .ReturnsAsync(createdNote);

            // Act
            var result = await _notesController.CreateNote(noteCreateDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_notesController.GetNoteById), createdAtActionResult.ActionName);
            var returnedNote = Assert.IsType<NoteDto>(createdAtActionResult.Value);
            Assert.Equal(createdNote.Id, returnedNote.Id);
            _noteServiceMock.Verify(service => service.CreateNoteAsync(noteCreateDto), Times.Once);
        }

        [Fact]
        public async Task CreateNote_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var noteCreateDto = new NoteCreateDto
            {
                PatientId = 1,
                DoctorName = "Dr. Test",
                NoteContent = "Test content"
            };

            _notesController.ModelState.AddModelError("DoctorName", "Required");

            // Act
            var result = await _notesController.CreateNote(noteCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
            _noteServiceMock.Verify(service => service.CreateNoteAsync(It.IsAny<NoteCreateDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateNote_WhenApplicationExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var noteCreateDto = new NoteCreateDto
            {
                PatientId = 1,
                DoctorName = "Dr. Test",
                NoteContent = "Test content"
            };

            _noteServiceMock.Setup(service => service.CreateNoteAsync(noteCreateDto))
                .ThrowsAsync(new ApplicationException("Test error"));

            // Act
            var result = await _notesController.CreateNote(noteCreateDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _noteServiceMock.Verify(service => service.CreateNoteAsync(noteCreateDto), Times.Once);
        }

        #endregion

        #region Tests UpdateNote

        [Fact]
        public async Task UpdateNote_WhenNoteExists_ReturnsNoContent()
        {
            // Arrange
            var noteUpdateDto = new NoteUpdateDto
            {
                DoctorName = "Dr. Updated",
                NoteContent = "Updated content"
            };

            _noteServiceMock.Setup(service => service.UpdateNoteAsync("test-id", noteUpdateDto))
                .ReturnsAsync(true);

            // Act
            var result = await _notesController.UpdateNote("test-id", noteUpdateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _noteServiceMock.Verify(service => service.UpdateNoteAsync("test-id", noteUpdateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateNote_WhenNoteDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var noteUpdateDto = new NoteUpdateDto
            {
                DoctorName = "Dr. Updated",
                NoteContent = "Updated content"
            };

            _noteServiceMock.Setup(service => service.UpdateNoteAsync("non-existent-id", noteUpdateDto))
                .ReturnsAsync(false);

            // Act
            var result = await _notesController.UpdateNote("non-existent-id", noteUpdateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            _noteServiceMock.Verify(service => service.UpdateNoteAsync("non-existent-id", noteUpdateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateNote_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var noteUpdateDto = new NoteUpdateDto
            {
                DoctorName = "Dr. Updated",
                NoteContent = "Updated content"
            };

            _notesController.ModelState.AddModelError("DoctorName", "Required");

            // Act
            var result = await _notesController.UpdateNote("test-id", noteUpdateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
            _noteServiceMock.Verify(service => service.UpdateNoteAsync(It.IsAny<string>(), It.IsAny<NoteUpdateDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNote_WhenApplicationExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var noteUpdateDto = new NoteUpdateDto
            {
                DoctorName = "Dr. Updated",
                NoteContent = "Updated content"
            };

            _noteServiceMock.Setup(service => service.UpdateNoteAsync("test-id", noteUpdateDto))
                .ThrowsAsync(new ApplicationException("Test error"));

            // Act
            var result = await _notesController.UpdateNote("test-id", noteUpdateDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _noteServiceMock.Verify(service => service.UpdateNoteAsync("test-id", noteUpdateDto), Times.Once);
        }

        #endregion

        #region Tests DeleteNote

        [Fact]
        public async Task DeleteNote_WhenNoteExists_ReturnsNoContent()
        {
            // Arrange
            _noteServiceMock.Setup(service => service.DeleteNoteAsync("test-id"))
                .ReturnsAsync(true);

            // Act
            var result = await _notesController.DeleteNote("test-id");

            // Assert
            Assert.IsType<NoContentResult>(result);
            _noteServiceMock.Verify(service => service.DeleteNoteAsync("test-id"), Times.Once);
        }

        [Fact]
        public async Task DeleteNote_WhenNoteDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _noteServiceMock.Setup(service => service.DeleteNoteAsync("non-existent-id"))
                .ReturnsAsync(false);

            // Act
            var result = await _notesController.DeleteNote("non-existent-id");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            _noteServiceMock.Verify(service => service.DeleteNoteAsync("non-existent-id"), Times.Once);
        }

        [Fact]
        public async Task DeleteNote_WhenApplicationExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            _noteServiceMock.Setup(service => service.DeleteNoteAsync("test-id"))
                .ThrowsAsync(new ApplicationException("Test error"));

            // Act
            var result = await _notesController.DeleteNote("test-id");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            _noteServiceMock.Verify(service => service.DeleteNoteAsync("test-id"), Times.Once);
        }

        #endregion
    }
}
