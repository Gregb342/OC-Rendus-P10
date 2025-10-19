using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Patients.Data;
using Patients.Domain.Services;
using Patients.Domain.Services.Interfaces;
using Patients.Infrastructure.Repositories;
using Patients.Infrastructure.Repositories.Interfaces;
using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using System.Text;

namespace Patients
{
    public class Program
    {
        public static void Main(string[] args)
        {
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

            // --- Database ---
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // --- Identity ---
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // --- Authentication / JWT ---
            var jwtSection = configuration.GetSection("JWT");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
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

            // --- Swagger ---
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Patients API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // --- CORS ---
            var corsSection = configuration.GetSection("CORS");
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ConfiguredCors", policy =>
                {
                    policy.WithOrigins(corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" })
                          .WithMethods(corsSection.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST" })
                          .WithHeaders(corsSection.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" });
                });
            });

            // --- Custom services ---
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<IPatientService, PatientService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("ConfiguredCors");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // --- Seed admin user ---
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var adminConfig = configuration.GetSection("AdminUser");
                var adminUser = new IdentityUser { UserName = adminConfig["Username"], Email = adminConfig["Email"] };

                if (userManager.FindByNameAsync(adminUser.UserName).Result == null)
                {
                    userManager.CreateAsync(adminUser, adminConfig["Password"] ?? "ChangeMe123!").Wait();
                }
            }

            app.Run();
        }
    }
}
