using Microsoft.AspNetCore.Components;
using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Pages
{
    public partial class PatientEdit
    {
        [Parameter] public int Id { get; set; }

        [Inject] private IPatientService PatientService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private PatientUpdateDto? PatientUpdate;
        private bool IsLoading = true;
        private bool IsSaving = false;
        private string? ErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadPatientAsync();
        }

        private async Task LoadPatientAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var patient = await PatientService.GetPatientByIdAsync(Id);

                if (patient == null)
                {
                    ErrorMessage = "Patient non trouvé";
                    return;
                }

                // Convertir PatientDto en PatientUpdateDto
                PatientUpdate = new PatientUpdateDto
                {
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    Address = patient.Address != null ? new AddressCreateDto
                    {
                        Street = patient.Address.Street,
                        City = patient.Address.City,
                        PostalCode = patient.Address.PostalCode,
                        Country = patient.Address.Country
                    } : null
                };
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

        private async Task HandleValidSubmit()
        {
            if (PatientUpdate == null) return;

            try
            {
                IsSaving = true;
                ErrorMessage = null;

                await PatientService.UpdatePatientAsync(Id, PatientUpdate);
                NavigationManager.NavigateTo($"/patients/details/{Id}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la mise à jour du patient : {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void AjouterAdresse()
        {
            if (PatientUpdate != null)
            {
                PatientUpdate.Address = new AddressCreateDto();
            }
        }

        private void SupprimerAdresse()
        {
            if (PatientUpdate != null)
            {
                PatientUpdate.Address = null;
            }
        }

        private void Annuler()
        {
            NavigationManager.NavigateTo($"/patients/details/{Id}");
        }
    }
}
