using FluentValidation;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.Common.Security;
using LibraryManagement.Application.DTOs.Users;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;

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

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(UserSearchRequest request, CancellationToken cancellationToken = default)
    {
        var userRecords = await users.SearchAsync(request.Role, request.IsActive, request.SearchTerm, cancellationToken);
        return userRecords.Select(Map).ToList();
    }

    public async Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 150)
        {
            throw new BusinessRuleException("Full name is required and must be at most 150 characters.");
        }

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The user was not found.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var existing = await users.GetByEmailAsync(email, cancellationToken);
            if (existing is not null && existing.Id != userId)
            {
                throw new ConflictException("A user with this email already exists.");
            }

            user.Email = email;
        }

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin && await users.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            throw new BusinessRuleException("The last active Admin cannot be demoted.");
        }

        user.FullName = request.FullName.Trim();
        user.MobileNumber = request.MobileNumber;
        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    public async Task<UserDto> DeactivateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The user was not found.");

        if (user.Role == UserRole.Admin && await users.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            throw new BusinessRuleException("The last active Admin cannot be deactivated.");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
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
