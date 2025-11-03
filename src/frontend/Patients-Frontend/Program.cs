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

            // Récupération directe de l'URL de l'API depuis la configuration
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("ApiBaseUrl n'est pas configurée dans appsettings.json");
            }

            // Configuration HttpClient avec URL de base depuis la configuration
            builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Enregistrement des autres services
            builder.Services.AddScoped<IPatientService, PatientService>();

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