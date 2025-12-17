using Microsoft.AspNetCore.Components;
using Patients_Frontend.DTOs;

namespace Patients_Frontend.Components.Pages
{
    public partial class Login
    {
        [SupplyParameterFromForm]
        private LoginDto loginModel { get; set; } = new();

        [SupplyParameterFromQuery(Name = "loggedout")]
        private bool? LoggedOut { get; set; }

        private bool IsLogging = false;
        private string? ErrorMessage;
        private bool ShowLogoutMessage => LoggedOut == true;

        private async Task HandleLogin()
        {
            IsLogging = true;
            ErrorMessage = null;

            try
            {
                var success = await AuthService.LoginAsync(loginModel);

                if (success)
                {
                    // Redirection sans forceLoad pour éviter NavigationException
                    Navigation.NavigateTo("/");
                }
                else
                {
                    ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect.";
                }
            }
            catch (Microsoft.AspNetCore.Components.NavigationException)
            {
                // Navigation réussie, ignorer l'exception
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