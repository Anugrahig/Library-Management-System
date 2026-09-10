using LibraryManagement.Application.Common.Security;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.API;

public static class InitialAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];
        var fullName = configuration["InitialAdmin:FullName"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
        {
            return;
        }

        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (await users.HasAnyAsync())
        {
            return;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = hasher.Hash(password),
            FullName = fullName.Trim(),
            Role = UserRole.Admin,
            IsActive = true
        };

        await users.AddAsync(user);
        await unitOfWork.SaveChangesAsync();
    }
}
