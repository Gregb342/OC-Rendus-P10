using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiGateway.Modules.Authentication.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ApiGateway.Modules.Authentication.Services.Interfaces;
using ApiGateway.Modules.Authentication.Services;

var builder = WebApplication.CreateBuilder(args);

// Adding configuration files
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddOcelot();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuration de la base d'auth
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnection")));

// Configuration Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Enregistrement du service d'auth
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuration JWT
var jwtSection = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer("Bearer", options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidAudience = jwtSection["ValidAudience"],
            ValidIssuer = jwtSection["ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"] ?? ""))
        };
    });

var app = builder.Build();

// --- Création automatique de la base de données et seed admin ---
using (var scope = app.Services.CreateScope())
{
    var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Créer la base de données si elle n'existe pas
    authDbContext.Database.EnsureCreated();

    // Créer l'utilisateur admin par défaut
    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            Console.WriteLine("Admin user created successfully!");
        }
        else
        {
            Console.WriteLine("Failed to create admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapWhen(
    context => !context.Request.Path.StartsWithSegments("/auth"),
    appBuilder =>
    {
        appBuilder.UseOcelot().Wait();
    });

app.UseHttpsRedirection();

app.Run();
