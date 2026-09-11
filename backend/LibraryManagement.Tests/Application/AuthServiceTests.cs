using LibraryManagement.Application.Common.Security;
using LibraryManagement.Application.DTOs.Auth;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Services;
using LibraryManagement.Application.Validators.Auth;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using Xunit;

namespace LibraryManagement.Tests.Application;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_creates_student_and_returns_token()
    {
        var users = new FakeUserRepository();
        var service = new AuthService(
            users,
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new LoginRequestValidator(),
            new RegisterRequestValidator());

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "student@example.com",
            Password = "password123",
            FullName = "Student User",
            Role = UserRole.Student
        });

        Assert.Equal("token-1", result.AccessToken);
        Assert.Equal(UserRole.Student, result.User.Role);
        Assert.Equal("hashed:password123", users.Added!.PasswordHash);
    }

    [Fact]
    public async Task Register_rejects_privileged_role()
    {
        var service = new AuthService(
            new FakeUserRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new LoginRequestValidator(),
            new RegisterRequestValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            service.RegisterAsync(new RegisterRequest
            {
                Email = "admin2@example.com",
                Password = "password123",
                FullName = "Admin User",
                Role = UserRole.Admin
            }));
    }

    [Fact]
    public async Task Login_returns_token_for_valid_active_user()
    {
        var user = new User
        {
            Id = 7,
            Email = "admin@example.com",
            PasswordHash = "hashed:password123",
            FullName = "Admin User",
            Role = UserRole.Admin,
            IsActive = true
        };
        var unitOfWork = new FakeUnitOfWork();
        var service = new AuthService(
            new FakeUserRepository(user),
            unitOfWork,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new LoginRequestValidator(),
            new RegisterRequestValidator());

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@example.com",
            Password = "password123"
        });

        Assert.Equal("token-7", result.AccessToken);
        Assert.Equal(7, result.User.Id);
        Assert.Equal(UserRole.Admin, result.User.Role);
        Assert.True(unitOfWork.SaveChangesCalled);
    }
}

internal sealed class FakeJwtTokenGenerator : ITokenService
{
    public TokenResult CreateToken(User user) => new()
    {
        AccessToken = $"token-{user.Id}",
        ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
    };
}
