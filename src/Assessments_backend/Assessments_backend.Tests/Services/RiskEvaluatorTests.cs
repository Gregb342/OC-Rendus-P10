using Assessments_backend.Dtos;
using Assessments_backend.Enums;
using Assessments_backend.Services;

namespace Assessments_backend.Tests.Services;

public class RiskEvaluatorTests
{
    private readonly RiskEvaluator _riskEvaluator;

    public RiskEvaluatorTests()
    {
        _riskEvaluator = new RiskEvaluator();
    }

    #region Evaluate Tests

    [Fact]
    public void Evaluate_WhenPatientIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        PatientDto? patient = null;
        var notes = new List<NoteDto>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _riskEvaluator.Evaluate(patient!, notes));
    }

    [Fact]
    public void Evaluate_WhenNotesIsNull_ReturnsNoneRiskLevel()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        IEnumerable<NoteDto>? notes = null;

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RiskLevel.None, result.RiskLevel);
        Assert.Equal(0, result.TriggerCount);
        Assert.Empty(result.MatchedTriggers);
    }

    [Fact]
    public void Evaluate_WhenNoTriggersFound_ReturnsNoneRiskLevel()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Le patient se porte bien." },
            new NoteDto { NoteContent = "Aucun symptôme particulier." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.None, result.RiskLevel);
        Assert.Equal(0, result.TriggerCount);
        Assert.Empty(result.MatchedTriggers);
    }

    #endregion

    #region Risk Level Tests - Over 30 years old

    [Fact]
    public void Evaluate_WhenPatientOver30WithTwoTriggers_ReturnsBorderLine()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Patient fumeur avec du cholestérol élevé." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.BorderLine, result.RiskLevel);
        Assert.Equal(2, result.TriggerCount);
        Assert.Contains("Fumeur", result.MatchedTriggers);
        Assert.Contains("Cholestérol", result.MatchedTriggers);
    }

    [Fact]
    public void Evaluate_WhenPatientOver30WithSixTriggers_ReturnsInDanger()
    {
        // Arrange
        var patient = CreateTestPatient(40, "Female");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeur avec cholestérol élevé, hémoglobine A1C anormale." },
            new NoteDto { NoteContent = "Vertiges fréquents, microalbumine détectée, poids en surcharge." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.InDanger, result.RiskLevel);
        Assert.True(result.TriggerCount >= 6);
    }

    [Fact]
    public void Evaluate_WhenPatientOver30WithEightTriggers_ReturnsEarlyOnset()
    {
        // Arrange
        var patient = CreateTestPatient(45, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeur avec cholestérol anormal, hémoglobine A1C élevée." },
            new NoteDto { NoteContent = "Vertiges, microalbumine, poids et taille mesurés." },
            new NoteDto { NoteContent = "Réaction allergique, anticorps détectés." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.EarlyOnset, result.RiskLevel);
        Assert.True(result.TriggerCount >= 8);
    }

    [Fact]
    public void Evaluate_WhenPatientOver30WithOneTrigger_ReturnsNone()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Patient fumeur." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.None, result.RiskLevel);
        Assert.Equal(1, result.TriggerCount);
    }

    #endregion

    #region Risk Level Tests - Under 30 years old - Male

    [Fact]
    public void Evaluate_WhenMaleUnder30WithThreeTriggers_ReturnsInDanger()
    {
        // Arrange
        var patient = CreateTestPatient(25, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeur avec cholestérol et vertiges." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.InDanger, result.RiskLevel);
        Assert.Equal(3, result.TriggerCount);
    }

    [Fact]
    public void Evaluate_WhenMaleUnder30WithFiveTriggers_ReturnsEarlyOnset()
    {
        // Arrange
        var patient = CreateTestPatient(28, "M");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeur avec cholestérol anormal, hémoglobine A1C." },
            new NoteDto { NoteContent = "Vertiges fréquents." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.EarlyOnset, result.RiskLevel);
        Assert.True(result.TriggerCount >= 5);
    }

    [Fact]
    public void Evaluate_WhenMaleUnder30WithTwoTriggers_ReturnsNone()
    {
        // Arrange
        var patient = CreateTestPatient(25, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeur avec cholestérol." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.None, result.RiskLevel);
        Assert.Equal(2, result.TriggerCount);
    }

    #endregion

    #region Risk Level Tests - Under 30 years old - Female

    [Fact]
    public void Evaluate_WhenFemaleUnder30WithFourTriggers_ReturnsInDanger()
    {
        // Arrange
        var patient = CreateTestPatient(27, "Female");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeuse, cholestérol, vertiges, poids." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.InDanger, result.RiskLevel);
        Assert.Equal(4, result.TriggerCount);
    }

    [Fact]
    public void Evaluate_WhenFemaleUnder30WithSevenTriggers_ReturnsEarlyOnset()
    {
        // Arrange
        var patient = CreateTestPatient(29, "F");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeuse avec cholestérol anormal, hémoglobine A1C élevée." },
            new NoteDto { NoteContent = "Vertiges, microalbumine, poids et taille mesurés." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.EarlyOnset, result.RiskLevel);
        Assert.True(result.TriggerCount >= 7);
    }

    [Fact]
    public void Evaluate_WhenFemaleUnder30WithThreeTriggers_ReturnsNone()
    {
        // Arrange
        var patient = CreateTestPatient(25, "Female");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeuse avec cholestérol et vertiges." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.None, result.RiskLevel);
        Assert.Equal(3, result.TriggerCount);
    }

    #endregion

    #region Trigger Detection Tests

    [Fact]
    public void Evaluate_DetectsTriggersCaseInsensitive()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Patient FUMEUR avec CHOLESTÉROL élevé." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(2, result.TriggerCount);
        Assert.Contains("Fumeur", result.MatchedTriggers);
        Assert.Contains("Cholestérol", result.MatchedTriggers);
    }

    [Fact]
    public void Evaluate_DetectsTriggersWithAccents()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Hemoglobine A1C et cholesterol mesurés." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(2, result.TriggerCount);
        Assert.Contains("Hémoglobine A1C", result.MatchedTriggers);
        Assert.Contains("Cholestérol", result.MatchedTriggers);
    }

    [Fact]
    public void Evaluate_CountsMultipleOccurrencesOfSameTrigger()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Patient fumeur." },
            new NoteDto { NoteContent = "Toujours fumeur." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(2, result.TriggerCount);
        Assert.Single(result.MatchedTriggers);
        Assert.Contains("Fumeur", result.MatchedTriggers);
    }

    [Fact]
    public void Evaluate_IgnoresEmptyNotes()
    {
        // Arrange
        var patient = CreateTestPatient(35, "Male");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "" },
            new NoteDto { NoteContent = "   " },
            new NoteDto { NoteContent = "Patient fumeur." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(1, result.TriggerCount);
        Assert.Contains("Fumeur", result.MatchedTriggers);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Evaluate_WhenPatientExactly30YearsOld_UsesOver30Rules()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-30);
        var patient = new PatientDto
        {
            Id = 1,
            DateOfBirth = birthDate,
            Gender = "Male"
        };
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeur avec cholestérol." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.None, result.RiskLevel);
    }

    [Fact]
    public void Evaluate_WhenGenderUnknown_ReturnsNoneForUnder30()
    {
        // Arrange
        var patient = CreateTestPatient(25, "Unknown");
        var notes = new List<NoteDto>
        {
            new NoteDto { NoteContent = "Fumeur avec cholestérol et vertiges." }
        };

        // Act
        var result = _riskEvaluator.Evaluate(patient, notes);

        // Assert
        Assert.Equal(RiskLevel.None, result.RiskLevel);
        Assert.Equal(3, result.TriggerCount);
    }

    #endregion

    #region Helper Methods

    private PatientDto CreateTestPatient(int age, string gender)
    {
        return new PatientDto
        {
            Id = 1,
            DateOfBirth = DateTime.Today.AddYears(-age),
            Gender = gender
        };
    }

    #endregion
}
