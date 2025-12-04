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
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private PatientDto? Patient;
        private List<NoteDto> noteList = new();
        private bool IsLoading = true;
        private string? ErrorMessage;
        private string? NoteErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadPatientAsync();
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
    }
}
