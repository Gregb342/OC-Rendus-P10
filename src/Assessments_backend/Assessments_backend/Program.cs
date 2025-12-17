using Assessments_backend.Repositories;
using Assessments_backend.Repositories.Interfaces;
using Assessments_backend.Services;
using Assessments_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --- Configure Serilog ---
var graylogSection = configuration.GetSection("Logging:Serilog:Graylog");
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Graylog(new GraylogSinkOptions
    {
        HostnameOrAddress = graylogSection["HostnameOrAddress"],
        Port = int.Parse(graylogSection["Port"] ?? "12201"),
        Facility = graylogSection["Facility"],
        TransportType = Enum.TryParse(graylogSection["TransportType"], out TransportType transport)
            ? transport
            : TransportType.Udp,
        ShortMessageMaxLength = int.Parse(graylogSection["ShortMessageMaxLength"] ?? "5000")
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Ajouter IHttpContextAccessor pour accéder au contexte HTTP
builder.Services.AddHttpContextAccessor();

// Configurer l'authentification JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };
});

builder.Services.AddAuthorization();

// Configurer HttpClient pour PatientDataProvider avec l'URL de l'API Gateway
var apiGatewayUrl = builder.Configuration["ApiGateway:BaseUrl"] ?? "http://api-gateway:8080";
builder.Services.AddHttpClient<IPatientDataProvider, PatientDataProvider>(client =>
{
    client.BaseAddress = new Uri(apiGatewayUrl);
});

// Configurer HttpClient pour NoteDataProvider avec l'URL de l'API Gateway
builder.Services.AddHttpClient<INoteDataProvider, NoteDataProvider>(client =>
{
    client.BaseAddress = new Uri(apiGatewayUrl);
});

// Enregistrer les services
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IRiskEvaluator, RiskEvaluator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
