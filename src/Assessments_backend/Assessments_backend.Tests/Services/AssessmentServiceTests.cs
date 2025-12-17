using Assessments_backend.Dtos;
using Assessments_backend.Enums;
using Assessments_backend.Repositories.Interfaces;
using Assessments_backend.Services;
using Assessments_backend.Services.Interfaces;
using Moq;

namespace Assessments_backend.Tests.Services;

public class AssessmentServiceTests
{
    private readonly Mock<IPatientDataProvider> _mockPatientDataProvider;
    private readonly Mock<INoteDataProvider> _mockNoteDataProvider;
    private readonly Mock<IRiskEvaluator> _mockRiskEvaluator;
    private readonly AssessmentService _assessmentService;

    public AssessmentServiceTests()
    {
        _mockPatientDataProvider = new Mock<IPatientDataProvider>();
        _mockNoteDataProvider = new Mock<INoteDataProvider>();
        _mockRiskEvaluator = new Mock<IRiskEvaluator>();

        _assessmentService = new AssessmentService(
            _mockPatientDataProvider.Object,
            _mockNoteDataProvider.Object,
            _mockRiskEvaluator.Object);
    }

    #region GetAssessment Tests

    [Fact]
    public async Task GetAssessment_WhenPatientExists_ReturnsAssessmentResult()
    {
        // Arrange
        var patientId = 1;
        var patient = CreateTestPatient(patientId, 35, "Male");
        var notes = CreateTestNotes();
        var evaluationResult = new RiskEvaluationResult
        {
            RiskLevel = RiskLevel.BorderLine,
            TriggerCount = 3,
            MatchedTriggers = new List<string> { "Fumeur", "Cholestérol", "Vertiges" }
        };

        _mockPatientDataProvider.Setup(p => p.GetPatientAsync(patientId))
            .ReturnsAsync(patient);
        _mockNoteDataProvider.Setup(n => n.GetNotesByPatientIdAsync(patientId))
            .ReturnsAsync(notes);
        _mockRiskEvaluator.Setup(r => r.Evaluate(patient, notes))
            .Returns(evaluationResult);

        // Act
        var result = await _assessmentService.GetAssessment(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patientId, result.PatientId);
        Assert.Equal(RiskLevel.BorderLine, result.RiskLevel);
        Assert.Equal(3, result.TriggerCount);
        Assert.Equal(3, result.MatchedTrigers.Count);
        Assert.Contains("Fumeur", result.MatchedTrigers);
        Assert.Contains("Cholestérol", result.MatchedTrigers);
        Assert.Contains("Vertiges", result.MatchedTrigers);

        _mockPatientDataProvider.Verify(p => p.GetPatientAsync(patientId), Times.Once);
        _mockNoteDataProvider.Verify(n => n.GetNotesByPatientIdAsync(patientId), Times.Once);
        _mockRiskEvaluator.Verify(r => r.Evaluate(patient, notes), Times.Once);
    }

    [Fact]
    public async Task GetAssessment_WhenPatientDoesNotExist_ReturnsNull()
    {
        // Arrange
        var patientId = 999;

        _mockPatientDataProvider.Setup(p => p.GetPatientAsync(patientId))
            .ReturnsAsync((PatientDto?)null);

        // Act
        var result = await _assessmentService.GetAssessment(patientId);

        // Assert
        Assert.Null(result);

        _mockPatientDataProvider.Verify(p => p.GetPatientAsync(patientId), Times.Once);
        _mockNoteDataProvider.Verify(n => n.GetNotesByPatientIdAsync(It.IsAny<int>()), Times.Never);
        _mockRiskEvaluator.Verify(r => r.Evaluate(It.IsAny<PatientDto>(), It.IsAny<IEnumerable<NoteDto>>()), Times.Never);
    }

    [Fact]
    public async Task GetAssessment_WhenPatientHasNoNotes_ReturnsAssessmentWithNoRisk()
    {
        // Arrange
        var patientId = 2;
        var patient = CreateTestPatient(patientId, 40, "Female");
        var emptyNotes = new List<NoteDto>();
        var evaluationResult = new RiskEvaluationResult
        {
            RiskLevel = RiskLevel.None,
            TriggerCount = 0,
            MatchedTriggers = new List<string>()
        };

        _mockPatientDataProvider.Setup(p => p.GetPatientAsync(patientId))
            .ReturnsAsync(patient);
        _mockNoteDataProvider.Setup(n => n.GetNotesByPatientIdAsync(patientId))
            .ReturnsAsync(emptyNotes);
        _mockRiskEvaluator.Setup(r => r.Evaluate(patient, emptyNotes))
            .Returns(evaluationResult);

        // Act
        var result = await _assessmentService.GetAssessment(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patientId, result.PatientId);
        Assert.Equal(RiskLevel.None, result.RiskLevel);
        Assert.Equal(0, result.TriggerCount);
        Assert.Empty(result.MatchedTrigers);

        _mockPatientDataProvider.Verify(p => p.GetPatientAsync(patientId), Times.Once);
        _mockNoteDataProvider.Verify(n => n.GetNotesByPatientIdAsync(patientId), Times.Once);
        _mockRiskEvaluator.Verify(r => r.Evaluate(patient, emptyNotes), Times.Once);
    }

    [Theory]
    [InlineData(RiskLevel.None)]
    [InlineData(RiskLevel.BorderLine)]
    [InlineData(RiskLevel.InDanger)]
    [InlineData(RiskLevel.EarlyOnset)]
    public async Task GetAssessment_ReturnsCorrectRiskLevel(RiskLevel expectedRiskLevel)
    {
        // Arrange
        var patientId = 3;
        var patient = CreateTestPatient(patientId, 50, "Male");
        var notes = CreateTestNotes();
        var evaluationResult = new RiskEvaluationResult
        {
            RiskLevel = expectedRiskLevel,
            TriggerCount = 5,
            MatchedTriggers = new List<string> { "Fumeur", "Cholestérol" }
        };

        _mockPatientDataProvider.Setup(p => p.GetPatientAsync(patientId))
            .ReturnsAsync(patient);
        _mockNoteDataProvider.Setup(n => n.GetNotesByPatientIdAsync(patientId))
            .ReturnsAsync(notes);
        _mockRiskEvaluator.Setup(r => r.Evaluate(patient, notes))
            .Returns(evaluationResult);

        // Act
        var result = await _assessmentService.GetAssessment(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRiskLevel, result.RiskLevel);
    }

    [Fact]
    public async Task GetAssessment_SetsAssessedAtToCurrentTime()
    {
        // Arrange
        var patientId = 4;
        var patient = CreateTestPatient(patientId, 30, "Female");
        var notes = CreateTestNotes();
        var evaluationResult = new RiskEvaluationResult
        {
            RiskLevel = RiskLevel.None,
            TriggerCount = 0,
            MatchedTriggers = new List<string>()
        };

        _mockPatientDataProvider.Setup(p => p.GetPatientAsync(patientId))
            .ReturnsAsync(patient);
        _mockNoteDataProvider.Setup(n => n.GetNotesByPatientIdAsync(patientId))
            .ReturnsAsync(notes);
        _mockRiskEvaluator.Setup(r => r.Evaluate(patient, notes))
            .Returns(evaluationResult);

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _assessmentService.GetAssessment(patientId);

        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AssessedAt >= beforeCall);
        Assert.True(result.AssessedAt <= afterCall);
    }

    [Fact]
    public async Task GetAssessment_WhenMultipleNotes_PassesAllNotesToEvaluator()
    {
        // Arrange
        var patientId = 5;
        var patient = CreateTestPatient(patientId, 28, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteId = "1", NoteContent = "Note 1" },
            new NoteDto { NoteId = "2", NoteContent = "Note 2" },
            new NoteDto { NoteId = "3", NoteContent = "Note 3" }
        };
        var evaluationResult = new RiskEvaluationResult
        {
            RiskLevel = RiskLevel.InDanger,
            TriggerCount = 4,
            MatchedTriggers = new List<string> { "Fumeur", "Cholestérol", "Vertiges", "Poids" }
        };

        _mockPatientDataProvider.Setup(p => p.GetPatientAsync(patientId))
            .ReturnsAsync(patient);
        _mockNoteDataProvider.Setup(n => n.GetNotesByPatientIdAsync(patientId))
            .ReturnsAsync(notes);
        _mockRiskEvaluator.Setup(r => r.Evaluate(patient, notes))
            .Returns(evaluationResult);

        // Act
        var result = await _assessmentService.GetAssessment(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TriggerCount);

        _mockRiskEvaluator.Verify(r => r.Evaluate(
            It.Is<PatientDto>(p => p.Id == patientId),
            It.Is<IEnumerable<NoteDto>>(n => n.Count() == 3)),
            Times.Once);
    }

    [Fact]
    public async Task GetAssessment_PreservesMatchedTriggersList()
    {
        // Arrange
        var patientId = 6;
        var patient = CreateTestPatient(patientId, 45, "Female");
        var notes = CreateTestNotes();
        var expectedTriggers = new List<string> { "Hémoglobine A1C", "Microalbumine", "Cholestérol" };
        var evaluationResult = new RiskEvaluationResult
        {
            RiskLevel = RiskLevel.BorderLine,
            TriggerCount = 3,
            MatchedTriggers = expectedTriggers
        };

        _mockPatientDataProvider.Setup(p => p.GetPatientAsync(patientId))
            .ReturnsAsync(patient);
        _mockNoteDataProvider.Setup(n => n.GetNotesByPatientIdAsync(patientId))
            .ReturnsAsync(notes);
        _mockRiskEvaluator.Setup(r => r.Evaluate(patient, notes))
            .Returns(evaluationResult);

        // Act
        var result = await _assessmentService.GetAssessment(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTriggers.Count, result.MatchedTrigers.Count);
        foreach (var trigger in expectedTriggers)
        {
            Assert.Contains(trigger, result.MatchedTrigers);
        }
    }

    #endregion

    #region Helper Methods

    private PatientDto CreateTestPatient(int id, int age, string gender)
    {
        return new PatientDto
        {
            Id = id,
            DateOfBirth = DateTime.Today.AddYears(-age),
            Gender = gender
        };
    }

    private List<NoteDto> CreateTestNotes()
    {
        return new List<NoteDto>
        {
            new NoteDto
            {
                NoteId = "1",
                NoteContent = "Patient fumeur avec cholestérol élevé."
            },
            new NoteDto
            {
                NoteId = "2",
                NoteContent = "Vertiges signalés lors de la dernière visite."
            }
        };
    }

    #endregion
}
