using Microsoft.AspNetCore.Components;
using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Pages
{
    public partial class Home
    {
        [Inject] private IPatientService PatientService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private List<PatientDto> Patients = new();
        private bool IsLoading = true;
        private string? ErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadPatientsAsync();
        }

        private async Task LoadPatientsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                Patients = await PatientService.GetAllPatientsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors du chargement des patients : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AjouterPatient()
        {
            NavigationManager.NavigateTo("/patients/create");
        }

        private void EditerPatient(int patientId)
        {
            NavigationManager.NavigateTo($"/patients/edit/{patientId}");
        }

        private void VoirPatient(int patientId)
        {
            NavigationManager.NavigateTo($"/patients/details/{patientId}");
        }

        private async Task SupprimerPatient(int patientId)
        {
            try
            {
                var success = await PatientService.DeletePatientAsync(patientId);
                if (success)
                {
                    await LoadPatientsAsync(); // Recharger la liste
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
    }
}