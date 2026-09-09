using FluentValidation;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.Common.Security;
using LibraryManagement.Application.DTOs.Users;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Application.Services;

public class UserService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IValidator<CreateUserRequest> validator) : IUserService
{
    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            MobileNumber = request.MobileNumber,
            Role = request.Role,
            IsActive = true
        };

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    private static UserDto Map(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        MobileNumber = user.MobileNumber,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        LastLoginAt = user.LastLoginAt
    };
}
