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

        private readonly PatientUpdateDto? PatientUpdate = new();

        private bool IsLoading = true;
        private bool IsSaving = false;
        private string? ErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("OnInitializedAsync START");
            await LoadPatientAsync();
            Console.WriteLine($"PatientUpdate chargé : {PatientUpdate?.FirstName}");
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

                PatientUpdate.FirstName = patient.FirstName;
                PatientUpdate.LastName = patient.LastName;
                PatientUpdate.DateOfBirth = patient.DateOfBirth;
                PatientUpdate.Gender = patient.Gender;
                PatientUpdate.PhoneNumber = patient.PhoneNumber;

                if (patient.Address != null)
                {
                    PatientUpdate.Address ??= new AddressCreateDto();
                    PatientUpdate.Address.Street = patient.Address.Street;
                    PatientUpdate.Address.City = patient.Address.City;
                    PatientUpdate.Address.PostalCode = patient.Address.PostalCode;
                    PatientUpdate.Address.Country = patient.Address.Country;
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

        private async Task HandleValidSubmit()
        {
            if (PatientUpdate == null) return;
            Console.WriteLine($"HandleValidSubmit : {PatientUpdate?.FirstName}");
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
