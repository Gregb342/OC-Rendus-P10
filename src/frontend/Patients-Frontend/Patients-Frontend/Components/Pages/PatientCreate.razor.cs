using Microsoft.AspNetCore.Components;
using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Pages
{
    public partial class PatientCreate
    {
        [Inject] private IPatientService PatientService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private PatientCreateDto NewPatient = new()
        {
            DateOfBirth = DateTime.Today.AddYears(-30) // Date par défaut
        };

        private bool IsSaving = false;
        private string? ErrorMessage;

        private async Task HandleValidSubmit()
        {
            try
            {
                IsSaving = true;
                ErrorMessage = null;

                var createdPatient = await PatientService.CreatePatientAsync(NewPatient);
                NavigationManager.NavigateTo($"/patients/details/{createdPatient.Id}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la création du patient : {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void AjouterAdresse()
        {
            NewPatient.Address = new AddressCreateDto();
        }

        private void SupprimerAdresse()
        {
            NewPatient.Address = null;
        }

        private void Annuler()
        {
            NavigationManager.NavigateTo("/");
        }
    }
}
