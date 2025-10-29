using Microsoft.AspNetCore.Components;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] private IAuthService AuthService { get; set; } = default!;

        private async Task HandleLogout()
        {
            try
            {
                await AuthService.LogoutAsync();
                // Le AuthenticationWrapper va automatiquement d�tecter le changement
                // et afficher la page de login
            }
            catch (Exception ex)
            {
                // Log l'erreur ou afficher un message � l'utilisateur
                Console.WriteLine($"Erreur lors de la d�connexion : {ex.Message}");
            }
        }
    }
}