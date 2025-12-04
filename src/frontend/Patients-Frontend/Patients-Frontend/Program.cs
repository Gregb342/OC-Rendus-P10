using Microsoft.AspNetCore.Components.Authorization;
using Patients_Frontend.Components;
using Patients_Frontend.Services;
using Patients_Frontend.Services.Interfaces;

namespace Patients_Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddAuthorizationCore();

            builder.Services.AddHttpContextAccessor();

            // Ajouter les services de session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Configuration HttpClient pour AuthService
            builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7109";
                client.BaseAddress = new Uri(apiUrl);
            });

            // Configuration HttpClient pour ApiService
            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7109";
                client.BaseAddress = new Uri(apiUrl);
            });

            // Enregistrement des services
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<INoteService, NoteService>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();
            app.UseSession();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}