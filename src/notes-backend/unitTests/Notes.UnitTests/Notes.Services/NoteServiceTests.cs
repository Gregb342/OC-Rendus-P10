using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using notes_backend.Infrastructure.Repositories.Interfaces;
using notes_backend.Domain.Services;
using notes_backend.DTOs;
using notes_backend.Domain.Entities;
using MongoDB.Driver;

namespace Notes.UnitTests.Notes.Services
{
    public class NoteServiceTests
    {
        private readonly Mock<INoteRepository> _noteRepositoryMock;
        private readonly NoteService _noteService;

        public NoteServiceTests()
        {
            _noteRepositoryMock = new Mock<INoteRepository>();

            _noteService = new NoteService(
                _noteRepositoryMock.Object);
        }

        #region Tests GetAllNotesAsync
        [Fact] 
        public async Task GetAllNotesAsync_WhenNotesExists_GetAllNotes()
        {
            // Arrange 
            List<Note> notes = new List<Note>
            {
                new Note
                {
                    Id = "blabla",
                    PatientId = 1,
                    DoctorName = "Docteur Maboul",
                    NoteContent = "Blablablou",
                    CreatedAt = DateTime.UtcNow
                },
                new Note
                {
                    Id = "blibli",
                    PatientId = 2,
                    DoctorName = "Docteur Maboul",
                    NoteContent = "tatata",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _noteRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(notes);

            // Act
            var result = await _noteService.GetAllNotesAsync();            

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _noteRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllNotesAsync_WhenNoNotesExists_ReturnsEmptyList()
        {
            // Arrange
            List<Note> emptyNoteList = new();

            _noteRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(emptyNoteList);

            // Act
            var result = await _noteService.GetAllNotesAsync();

            // Assert
            Assert.Empty(result);
            _noteRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllNotesAsync_WhenMongoExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            _noteRepositoryMock.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new MongoException("Erreur à la récupération des notes dans mongoDb"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(async () => await _noteService.GetAllNotesAsync());
            _noteRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        #endregion

        #region Tests GetNoteByIdAsync

        [Fact]
        public async Task GetNoteByIdAsync_WhenCorrectId_ReturnsNote()
        {
            // Arrange
            Note mockNote = new()
            {
                Id = "blabla",
                PatientId = 1,
                DoctorName = "Docteur Maboul",
                NoteContent = "Blablablou",
                CreatedAt = DateTime.UtcNow
            };

            _noteRepositoryMock.Setup(repo => repo.GetByIdAsync(mockNote.Id))
                .ReturnsAsync(mockNote);

            // Act
            var result = await _noteService.GetNoteByIdAsync("blabla");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, mockNote.Id);
            Assert.Equal(result.NoteContent, mockNote.NoteContent);
            _noteRepositoryMock.Verify(repo => repo.GetByIdAsync("blabla"), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetNoteByIdAsync_WhenIncorrectId_ReturnsNull(string invalidId)
        {
            // Arrange
            _noteRepositoryMock.Setup(repo => repo.GetByIdAsync("blibli"))
                .ReturnsAsync(() => null);

            // Act
            var result = await _noteService.GetNoteByIdAsync("blibli");

            // Assert
            Assert.Null(result);
            _noteRepositoryMock.Verify(repo => repo.GetByIdAsync("blibli"), Times.Once);
        }

        [Fact]
        public async Task GetNoteByIdAsync_WhenMongoExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            _noteRepositoryMock.Setup(repo => repo.GetByIdAsync("blabla")).ThrowsAsync(new MongoException(""));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(async () => await _noteService.GetNoteByIdAsync("blabla"));
            _noteRepositoryMock.Verify(repo => repo.GetByIdAsync("blabla"), Times.Once);
        }

        #endregion

        #region Tests GetNotesByPatientIdAsync

        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        [InlineData(99, 0)]
        public async Task GetNotesByPatientIdAsync_ReturnsCorrectCount(int patientId, int expectedCount)
        {
            // Arrange
            var allNotes = new List<Note>
            {
                new Note { Id = "1", PatientId = 1, DoctorName = "Dr. A", NoteContent = "Note 1", CreatedAt = DateTime.UtcNow },
                new Note { Id = "2", PatientId = 1, DoctorName = "Dr. B", NoteContent = "Note 2", CreatedAt = DateTime.UtcNow },
                new Note { Id = "3", PatientId = 2, DoctorName = "Dr. C", NoteContent = "Note 3", CreatedAt = DateTime.UtcNow }
            };

            var notesForPatient = allNotes.Where(n => n.PatientId == patientId).ToList();

            _noteRepositoryMock.Setup(repo => repo.GetByPatientIdAsync(patientId))
                .ReturnsAsync(notesForPatient);

            // Act
            var result = await _noteService.GetNotesByPatientIdAsync(patientId);

            // Assert
            Assert.Equal(expectedCount, result.Count());
            _noteRepositoryMock.Verify(repo => repo.GetByPatientIdAsync(patientId), Times.Once);
        }

        [Fact]
        public async Task GetNotesByPatientIdAsync_WhenMongoExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            _noteRepositoryMock.Setup(repo => repo.GetByPatientIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new MongoException(""));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(async () => await _noteService.GetNotesByPatientIdAsync(1));
            _noteRepositoryMock.Verify(repo => repo.GetByPatientIdAsync(1), Times.Once);
        }

        #endregion

        #region Tests CreateNoteAsync

        [Fact]
        public async Task CreateNoteAsync_WithValidData_ReturnsCreatedNote()
        {
            // Arrange
            var noteCreateDto = new NoteCreateDto
            {
                PatientId = 1,
                DoctorName = "Dr. Test",
                NoteContent = "Test content"
            };

            var createdNote = new Note
            {
                Id = "new-id",
                PatientId = noteCreateDto.PatientId,
                DoctorName = noteCreateDto.DoctorName,
                NoteContent = noteCreateDto.NoteContent,
                CreatedAt = DateTime.UtcNow
            };

            _noteRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Note>()))
                .ReturnsAsync(createdNote);

            // Act
            var result = await _noteService.CreateNoteAsync(noteCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdNote.Id, result.Id);
            Assert.Equal(noteCreateDto.PatientId, result.PatientId);
            Assert.Equal(noteCreateDto.DoctorName, result.DoctorName);
            Assert.Equal(noteCreateDto.NoteContent, result.NoteContent);
            _noteRepositoryMock.Verify(repo => repo.CreateAsync(It.Is<Note>(n => 
                n.PatientId == noteCreateDto.PatientId && 
                n.DoctorName == noteCreateDto.DoctorName && 
                n.NoteContent == noteCreateDto.NoteContent)), Times.Once);
        }

        [Fact]
        public async Task CreateNoteAsync_WhenMongoExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            var noteCreateDto = new NoteCreateDto
            {
                PatientId = 1,
                DoctorName = "Dr. Test",
                NoteContent = "Test content"
            };

            _noteRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Note>()))
                .ThrowsAsync(new MongoException(""));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(async () => await _noteService.CreateNoteAsync(noteCreateDto));
            _noteRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Note>()), Times.Once);
        }

        #endregion

        #region Tests UpdateNoteAsync

        [Fact]
        public async Task UpdateNoteAsync_WhenNoteExists_ReturnsTrue()
        {
            // Arrange
            var existingNote = new Note
            {
                Id = "test-id",
                PatientId = 1,
                DoctorName = "Dr. Old",
                NoteContent = "Old content",
                CreatedAt = DateTime.UtcNow
            };

            var noteUpdateDto = new NoteUpdateDto
            {
                DoctorName = "Dr. New",
                NoteContent = "New content"
            };

            _noteRepositoryMock.Setup(repo => repo.GetByIdAsync("test-id"))
                .ReturnsAsync(existingNote);

            _noteRepositoryMock.Setup(repo => repo.UpdateAsync("test-id", It.IsAny<Note>()))
                .ReturnsAsync(true);

            // Act
            var result = await _noteService.UpdateNoteAsync("test-id", noteUpdateDto);

            // Assert
            Assert.True(result);
            _noteRepositoryMock.Verify(repo => repo.GetByIdAsync("test-id"), Times.Once);
            _noteRepositoryMock.Verify(repo => repo.UpdateAsync("test-id", It.Is<Note>(n => 
                n.DoctorName == noteUpdateDto.DoctorName && 
                n.NoteContent == noteUpdateDto.NoteContent)), Times.Once);
        }

        [Fact]
        public async Task UpdateNoteAsync_WhenNoteDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var noteUpdateDto = new NoteUpdateDto
            {
                DoctorName = "Dr. Test",
                NoteContent = "Test content"
            };

            _noteRepositoryMock.Setup(repo => repo.GetByIdAsync("non-existent-id"))
                .ReturnsAsync((Note?)null);

            // Act
            var result = await _noteService.UpdateNoteAsync("non-existent-id", noteUpdateDto);

            // Assert
            Assert.False(result);
            _noteRepositoryMock.Verify(repo => repo.GetByIdAsync("non-existent-id"), Times.Once);
            _noteRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNoteAsync_WhenMongoExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            var noteUpdateDto = new NoteUpdateDto
            {
                DoctorName = "Dr. Test",
                NoteContent = "Test content"
            };

            _noteRepositoryMock.Setup(repo => repo.GetByIdAsync("test-id"))
                .ThrowsAsync(new MongoException(""));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(async () => await _noteService.UpdateNoteAsync("test-id", noteUpdateDto));
            _noteRepositoryMock.Verify(repo => repo.GetByIdAsync("test-id"), Times.Once);
        }

        #endregion

        #region Tests DeleteNoteAsync

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteNoteAsync_ReturnsRepositoryResult(bool expectedResult)
        {
            // Arrange
            _noteRepositoryMock.Setup(repo => repo.DeleteAsync("test-id"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _noteService.DeleteNoteAsync("test-id");

            // Assert
            Assert.Equal(expectedResult, result);
            _noteRepositoryMock.Verify(repo => repo.DeleteAsync("test-id"), Times.Once);
        }

        [Fact]
        public async Task DeleteNoteAsync_WhenMongoExceptionOccurs_ThrowsApplicationException()
        {
            // Arrange
            _noteRepositoryMock.Setup(repo => repo.DeleteAsync("test-id"))
                .ThrowsAsync(new MongoException(""));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(async () => await _noteService.DeleteNoteAsync("test-id"));
            _noteRepositoryMock.Verify(repo => repo.DeleteAsync("test-id"), Times.Once);
        }

        #endregion
    }
}
