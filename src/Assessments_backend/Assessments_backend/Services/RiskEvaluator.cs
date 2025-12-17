using Assessments_backend.Dtos;
using Assessments_backend.Enums;
using Assessments_backend.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace Assessments_backend.Services
{
    public class RiskEvaluationResult
    {
        public RiskLevel RiskLevel { get; init; }
        public int TriggerCount { get; init; }
        public List<string> MatchedTriggers { get; init; } = new();
    }

    public class RiskEvaluator : IRiskEvaluator
    {
        private static readonly List<string> TriggersList = new()
        {
            "Hémoglobine A1C",
            "Microalbumine",
            "Taille",
            "Poids",
            "Fumeur",
            "Fumeuse",
            "Anormal",
            "Cholestérol",
            "Vertiges",
            "Rechute",
            "Réaction",
            "Anticorps"
        };

        /// <summary>
        /// Évalue le niveau de risque d'un patient en fonction de ses informations personnelles et de ses notes médicales.
        /// </summary>
        /// <param name="patient">Les informations du patient à évaluer.</param>
        /// <param name="notes">La collection des notes médicales du patient.</param>
        /// <returns>Un résultat d'évaluation contenant le niveau de risque, le nombre de déclencheurs et la liste des déclencheurs détectés.</returns>
        /// <exception cref="ArgumentNullException">Lancée si le paramètre patient est null.</exception>
        public RiskEvaluationResult Evaluate(PatientDto patient, IEnumerable<NoteDto> notes)
        {
            if (patient is null)
                throw new ArgumentNullException(nameof(patient));

            if (notes is null)
                notes = Enumerable.Empty<NoteDto>();

            var (triggerCount, matchedTriggers) = CountTriggers(notes);
            var riskLevel = EvaluateRisk(patient, triggerCount);

            return new RiskEvaluationResult
            {
                RiskLevel = riskLevel,
                TriggerCount = triggerCount,
                MatchedTriggers = matchedTriggers
            };
        }

        /// <summary>
        /// Compte le nombre de déclencheurs présents dans les notes médicales du patient.
        /// </summary>
        /// <param name="notes">La collection des notes médicales à analyser.</param>
        /// <returns>Un tuple contenant le nombre total de déclencheurs et la liste des déclencheurs trouvés.</returns>
        private (int triggerCount, List<string> matchedTriggers) CountTriggers(IEnumerable<NoteDto> notes)
        {
            int triggerCount = 0;
            var matchedTriggers = new HashSet<string>();

            var normalizedTriggers = TriggersList
                .Select(Normalize)
                .ToList();

            foreach (var note in notes)
            {
                if (string.IsNullOrWhiteSpace(note.NoteContent))
                    continue;

                var content = Normalize(note.NoteContent);

                for (int i = 0; i < normalizedTriggers.Count; i++)
                {
                    var trigger = normalizedTriggers[i];

                    if (content.Contains(trigger))
                    {
                        triggerCount++;
                        matchedTriggers.Add(TriggersList[i]); // version lisible pour le DTO
                    }
                }
            }

            return (triggerCount, matchedTriggers.ToList());
        }        

        /// <summary>
        /// Évalue le niveau de risque d'un patient en fonction de son âge, son genre et le nombre de déclencheurs détectés.
        /// </summary>
        /// <param name="patient">Les informations du patient.</param>
        /// <param name="triggerCount">Le nombre de déclencheurs détectés dans les notes médicales.</param>
        /// <returns>Le niveau de risque calculé pour le patient.</returns>
        private RiskLevel EvaluateRisk(PatientDto patient, int triggerCount)
        {
            var age = GetAge(patient.DateOfBirth);
            var gender = patient.Gender?.ToLowerInvariant();

            if (triggerCount == 0)
            {
                return RiskLevel.None;
            }

            const int thresholdAge = 30;

            if (age > thresholdAge)
            {
                if (triggerCount >= 8)
                    return RiskLevel.EarlyOnset;

                if (triggerCount >= 6)
                    return RiskLevel.InDanger;

                if (triggerCount >= 2 && triggerCount <= 5)
                    return RiskLevel.BorderLine;

                return RiskLevel.None;
            }
            else
            {
                // < 30 ans
                bool isMale = gender == "male" || gender == "m";
                bool isFemale = gender == "female" || gender == "f";

                if (isMale)
                {
                    if (triggerCount >= 5)
                        return RiskLevel.EarlyOnset;

                    if (triggerCount >= 3)
                        return RiskLevel.InDanger;

                    return RiskLevel.None;
                }

                if (isFemale)
                {
                    if (triggerCount >= 7)
                        return RiskLevel.EarlyOnset;

                    if (triggerCount >= 4)
                        return RiskLevel.InDanger;

                    return RiskLevel.None;
                }

                return RiskLevel.None;
            }
        }

        #region ---------- Utilitaires ----------

        /// <summary>
        /// Calcule l'âge d'une personne à partir de sa date de naissance.
        /// </summary>
        /// <param name="birthDate">La date de naissance.</param>
        /// <returns>L'âge en années.</returns>
        private static int GetAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        /// <summary>
        /// Normalise une chaîne de caractères en la convertissant en minuscules et en supprimant les accents.
        /// </summary>
        /// <param name="input">La chaîne de caractères à normaliser.</param>
        /// <returns>La chaîne normalisée sans accents et en minuscules.</returns>
        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var normalized = input
                .ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder(normalized.Length);

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        #endregion
    }
}
