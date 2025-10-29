using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Patients_Frontend.DTOs;

namespace Patients_Frontend.Components.Pages
{
    public partial class Login
    {
        [Parameter] public EventCallback OnLoginSuccess { get; set; }

        [SupplyParameterFromForm]
        private LoginDto loginModel { get; set; } = new();

        private bool IsLogging = false;
        private string? ErrorMessage;

        private async Task HandleLogin()
        {
            try
            {
                IsLogging = true;
                ErrorMessage = null;

                var success = await AuthService.LoginAsync(loginModel);

                if (success)
                {
                    await OnLoginSuccess.InvokeAsync();
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