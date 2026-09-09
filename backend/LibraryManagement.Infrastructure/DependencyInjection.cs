using LibraryManagement.Application.Common.Security;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Infrastructure.Repositories;
using LibraryManagement.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IMemberRepository, EfMemberRepository>();
        services.AddScoped<IBookRepository, EfBookRepository>();
        services.AddScoped<IBookIssueRepository, EfBookIssueRepository>();
        services.AddScoped<IReservationRepository, EfReservationRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<Data.LibraryDbContext>());
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
