using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Components;
using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Pages
{
    public partial class PatientDetails
    {
        [Parameter] public int Id { get; set; }

        [Inject] private IPatientService PatientService { get; set; } = default!;
        [Inject] private INoteService NoteService { get; set; } = default!;
        [Inject] private IAssessmentService AssessmentService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private PatientDto? Patient;
        private AssessmentResultDto? Assessment;
        private List<NoteDto> noteList = new();
        private bool IsLoading = true;
        private bool IsLoadingAssessment = true;
        private string? ErrorMessage;
        private string? NoteErrorMessage;
        private string? AssessmentErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadPatientAsync();
            await LoadAssessmentAsync();
            await LoadNoteListByPatientAsync(Id);
        }

        private async Task LoadPatientAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                Patient = await PatientService.GetPatientByIdAsync(Id);

                if (Patient == null)
                {
                    ErrorMessage = "Patient non trouvé";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors du chargement du patient : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task LoadNoteListByPatientAsync(int patientId)
        {
            NoteErrorMessage = null;

            var notes = await NoteService.GetNotesByPatientIdAsync(patientId);

            if (notes == null || !notes.Any())
            {
                noteList = new List<NoteDto>();
                NoteErrorMessage = "Pas de notes trouvées pour ce patient";
            }
            else
            {
                noteList = notes.ToList();
            }
        }

        private async Task LoadAssessmentAsync()
        {
            try
            {
                IsLoadingAssessment = true;
                AssessmentErrorMessage = null;
                Assessment = await AssessmentService.GetPatientAssessmentAsync(Id);

                if (Assessment == null)
                {
                    AssessmentErrorMessage = "Impossible de calculer le risque de diabète";
                }
            }
            catch (Exception ex)
            {
                AssessmentErrorMessage = $"Erreur lors du calcul du risque : {ex.Message}";
            }
            finally
            {
                IsLoadingAssessment = false;
            }
        }

        private string GetRiskLevelText()
        {
            if (Assessment == null) return "Inconnu";

            return Assessment.RiskLevel switch
            {
                RiskLevel.None => "Aucun risque",
                RiskLevel.Borderline => "Risque limité",
                RiskLevel.InDanger => "En danger",
                RiskLevel.EarlyOnset => "Apparition précoce",
                _ => "Inconnu"
            };
        }

        private string GetRiskLevelClass()
        {
            if (Assessment == null) return "secondary";

            return Assessment.RiskLevel switch
            {
                RiskLevel.None => "success",
                RiskLevel.Borderline => "warning",
                RiskLevel.InDanger => "danger",
                RiskLevel.EarlyOnset => "danger",
                _ => "secondary"
            };
        }

        private int CalculerAge()
        {
            if (Patient == null) return 0;

            var today = DateTime.Today;
            var age = today.Year - Patient.DateOfBirth.Year;

            if (Patient.DateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        private async Task SupprimerPatient()
        {
            try
            {
                var success = await PatientService.DeletePatientAsync(Id);
                if (success)
                {
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    ErrorMessage = "Erreur lors de la suppression du patient";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la suppression : {ex.Message}";
            }
        }

        private void EditerPatient()
        {
            NavigationManager.NavigateTo($"/patients/edit/{Id}");
        }

        private void RetourListe()
        {
            NavigationManager.NavigateTo("/");
        }

        private void AjouterNote()
        {
            NavigationManager.NavigateTo($"/patients/{Id}/notes/create");
        }

    }
}
