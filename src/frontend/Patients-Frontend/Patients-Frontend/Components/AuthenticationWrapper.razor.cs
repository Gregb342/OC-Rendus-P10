using Microsoft.AspNetCore.Components;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend.Components
{
    public partial class AuthenticationWrapper : IDisposable
    {
        [Inject] private IAuthService AuthService { get; set; } = default!;
        [Parameter] public RenderFragment ChildContent { get; set; } = default!;

        private bool IsAuthenticated = false;
        private bool IsLoading = true;

        protected override async Task OnInitializedAsync()
        {
            // S'abonner aux changements d'état d'authentification
            AuthService.AuthenticationStateChanged += OnAuthenticationStateChanged;

            IsAuthenticated = await AuthService.IsAuthenticatedAsync();
            IsLoading = false;
        }

        private async void OnAuthenticationStateChanged()
        {
            IsAuthenticated = await AuthService.IsAuthenticatedAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task HandleLoginSuccess()
        {
            IsAuthenticated = await AuthService.IsAuthenticatedAsync();
            StateHasChanged();
        }

        public void Dispose()
        {
            AuthService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}