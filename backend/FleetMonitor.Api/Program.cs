using DotNetEnv;
using FleetMonitor.Api.Infrastructure.Auth;
using FleetMonitor.Api.Infrastructure.Data;
using FleetMonitor.Api.Infrastructure.Data.Extensions;
using FleetMonitor.Api.Hubs;
using FleetMonitor.Api.Middleware;
using FleetMonitor.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
var parentEnvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");

if (File.Exists(envPath))
{
    Env.Load(envPath);
}
else if (File.Exists(parentEnvPath))
{
    Env.Load(parentEnvPath);
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

var secureConnectionString = ReplacePlaceholders(connectionString);
builder.Configuration["ConnectionStrings:DefaultConnection"] = secureConnectionString;

builder.Configuration["Jwt:SecretKey"] = ReplacePlaceholders(
    builder.Configuration["Jwt:SecretKey"] ?? string.Empty);
builder.Configuration["Jwt:Issuer"] = ReplacePlaceholders(
    builder.Configuration["Jwt:Issuer"] ?? string.Empty);
builder.Configuration["Jwt:Audience"] = ReplacePlaceholders(
    builder.Configuration["Jwt:Audience"] ?? string.Empty);
builder.Configuration["Jwt:ExpirationMinutes"] = ReplacePlaceholders(
    builder.Configuration["Jwt:ExpirationMinutes"] ?? "60");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<FuelAlertService>();
builder.Services.AddScoped<SensorIngestService>();
builder.Services.AddScoped<AlertNotificationService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(secureConnectionString));

var app = builder.Build();

await app.ApplyMigrationsAndSeedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseMiddleware<JwtAuthMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AlertsHub>("/hubs/alerts");

app.Run();

static string ReplacePlaceholders(string value)
{
    return value
        .Replace("{DB_HOST}", Environment.GetEnvironmentVariable("DB_HOST") ?? string.Empty)
        .Replace("{DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT") ?? string.Empty)
        .Replace("{DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? string.Empty)
        .Replace("{DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? string.Empty)
        .Replace("{DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty)
        .Replace("{DB_HOST_DEV}", Environment.GetEnvironmentVariable("DB_HOST_DEV") ?? string.Empty)
        .Replace("{DB_PORT_DEV}", Environment.GetEnvironmentVariable("DB_PORT_DEV") ?? string.Empty)
        .Replace("{DB_NAME_DEV}", Environment.GetEnvironmentVariable("DB_NAME_DEV") ?? string.Empty)
        .Replace("{DB_USER_DEV}", Environment.GetEnvironmentVariable("DB_USER_DEV") ?? string.Empty)
        .Replace("{DB_PASSWORD_DEV}", Environment.GetEnvironmentVariable("DB_PASSWORD_DEV") ?? string.Empty)
        .Replace("{JWT_SECRET_KEY}", Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? string.Empty)
        .Replace("{JWT_ISSUER}", Environment.GetEnvironmentVariable("JWT_ISSUER") ?? string.Empty)
        .Replace("{JWT_AUDIENCE}", Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? string.Empty)
        .Replace("{JWT_EXPIRATION_MINUTES}", Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "60");
}
