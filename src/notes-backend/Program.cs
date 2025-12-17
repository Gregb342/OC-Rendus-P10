using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using notes_backend.Data;
using notes_backend.Domain.Services;
using notes_backend.Domain.Services.Interfaces;
using notes_backend.Infrastructure.Repositories;
using notes_backend.Infrastructure.Repositories.Interfaces;
using notes_backend.Infrastructure.Settings;
using System.Text;

namespace notes_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration MongoDB
            builder.Services.Configure<MongoDbSettings>(
                builder.Configuration.GetSection("MongoDbSettings"));

            // Enregistrement des services
            builder.Services.AddSingleton<MongoDbContext>();
            builder.Services.AddScoped<INoteRepository, NoteRepository>();
            builder.Services.AddScoped<INoteService, NoteService>();

            builder.Services.AddControllers();

            // --- Authentication / JWT ---
            var jwtSection = builder.Configuration.GetSection("JWT");
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

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            SeedDatabase(app.Configuration);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void SeedDatabase(IConfiguration configuration)
        {
            var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            if (mongoDbSettings == null)
            {
                throw new InvalidOperationException("MongoDbSettings configuration is missing");
            }
            
            using (var client = new MongoClient(mongoDbSettings.ConnectionString))
            {
                var database = client.GetDatabase(mongoDbSettings.DatabaseName);
                NotesSeed.SeedAsync(database).GetAwaiter().GetResult();
            }
        }
    }
}
