using LibraryManagement.Application.DTOs.Users;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? MobileNumber { get; set; }

    public UserRole Role { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public UserDto User { get; set; } = null!;
}

public class TokenResult
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }
}
