using Microsoft.AspNetCore.Components;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] private IAuthService AuthService { get; set; } = default!;

        protected async Task HandleLogout()
        {
            try
            {
                await AuthService.LogoutAsync();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la déconnexion : {ex.Message}");
            }
        }
    }
}