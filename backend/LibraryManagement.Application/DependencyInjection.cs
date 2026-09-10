using FluentValidation;
using LibraryManagement.Application.Validators.Books;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookIssueService, BookIssueService>();
        services.AddScoped<IReservationService, ReservationService>();

        services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();

        return services;
    }
}
