using Patients_Frontend.DTOs;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Pages
{
    public partial class Home
    {
        [Inject]
        private IPatientService PatientService { get; set; } = default!;

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

        private async Task EditerPatient(int patientId)
        {
            try
            {
                var patient = await PatientService.GetPatientByIdAsync(patientId);
                if (patient != null)
                {
                    // Logique pour naviguer vers la page d'édition
                    Console.WriteLine($"Édition du patient: {patient.FirstName} {patient.LastName}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la récupération du patient : {ex.Message}";
            }
        }

        private async Task VoirPatient(int patientId)
        {
            try
            {
                var patient = await PatientService.GetPatientByIdAsync(patientId);
                if (patient != null)
                {
                    // Logique pour naviguer vers la page de détails
                    Console.WriteLine($"Affichage du patient: {patient.FirstName} {patient.LastName}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la récupération du patient : {ex.Message}";
            }
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