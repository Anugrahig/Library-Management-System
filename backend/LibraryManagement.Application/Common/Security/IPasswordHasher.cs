namespace LibraryManagement.Application.Common.Security;

using LibraryManagement.Application.DTOs.Auth;
using LibraryManagement.Domain.Entities;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}

public interface ITokenService
{
    TokenResult CreateToken(User user);
}
