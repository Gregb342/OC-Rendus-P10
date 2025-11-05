using Microsoft.AspNetCore.Components;
using Patients_Frontend.DTOs;

namespace Patients_Frontend.Components.Pages
{
    public partial class Login
    {
        [SupplyParameterFromForm]
        private LoginDto loginModel { get; set; } = new();

        private bool IsLogging = false;
        private string? ErrorMessage;

        private async Task HandleLogin()
        {
            IsLogging = true;
            ErrorMessage = null;

            try
            {
                var success = await AuthService.LoginAsync(loginModel);

                if (success)
                {
                    StateHasChanged();
                }
                else
                {
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur de connexion : {ex.Message}";
            }
            finally
            {
                IsLogging = false;
            }
        }
    }
}