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
    }
}
