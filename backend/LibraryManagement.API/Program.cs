using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using LibraryManagement.API;
using LibraryManagement.API.Security;
using LibraryManagement.API.Workers;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.Common.Security;
using LibraryManagement.Application;
using LibraryManagement.Infrastructure.Data;
using LibraryManagement.Infrastructure;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

if (File.Exists(envFilePath))
{
    Env.Load(envFilePath);
}
else
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

ApplyDotEnvConfiguration(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
}

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
var jwtSettings = new JwtOptions
{
    Key = builder.Configuration["Jwt:Key"] ?? string.Empty,
    Issuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty,
    Audience = builder.Configuration["Jwt:Audience"] ?? string.Empty,
    ExpiryMinutes = int.TryParse(builder.Configuration["Jwt:ExpiryMinutes"], out var expiryMinutes)
        ? expiryMinutes
        : 60
};

if (jwtSettings.Key.Length < 32 || string.IsNullOrWhiteSpace(jwtSettings.Issuer) || string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException("Jwt:Key, Jwt:Issuer, and Jwt:Audience must be configured.");
}

builder.Services.AddSingleton(Options.Create(jwtSettings));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHostedService<ReservationExpirationWorker>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var statusCode = exception switch
    {
        ValidationException => StatusCodes.Status400BadRequest,
        NotFoundException => StatusCodes.Status404NotFound,
        ConflictException => StatusCodes.Status409Conflict,
        BusinessRuleException => StatusCodes.Status400BadRequest,
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };

    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new
    {
        success = false,
        message = exception?.Message ?? "An unexpected error occurred.",
        errors = Array.Empty<string>()
    });
}));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await InitialAdminSeeder.SeedAsync(app.Services, builder.Configuration);

app.Run();

static void ApplyDotEnvConfiguration(IConfigurationManager configuration)
{
    SetFromDotEnv(configuration, "ConnectionStrings:DefaultConnection", "ConnectionStrings__DefaultConnection");
    SetFromDotEnv(configuration, "Jwt:Key", "Jwt__Key");
    SetFromDotEnv(configuration, "Jwt:Issuer", "Jwt__Issuer");
    SetFromDotEnv(configuration, "Jwt:Audience", "Jwt__Audience");
    SetFromDotEnv(configuration, "Jwt:ExpiryMinutes", "Jwt__ExpiryMinutes");
    SetFromDotEnv(configuration, "InitialAdmin:Email", "InitialAdmin__Email");
    SetFromDotEnv(configuration, "InitialAdmin:Password", "InitialAdmin__Password");
    SetFromDotEnv(configuration, "InitialAdmin:FullName", "InitialAdmin__FullName");
}

static void SetFromDotEnv(IConfigurationManager configuration, string configurationKey, string environmentKey)
{
    try
    {
        var value = Env.GetString(environmentKey);
        if (!string.IsNullOrWhiteSpace(value))
        {
            configuration[configurationKey] = value;
        }
    }
    catch (KeyNotFoundException)
    {
        // Optional dotenv settings are allowed to be absent.
    }
}
