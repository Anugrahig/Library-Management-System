using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Member> Members => Set<Member>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<BookIssue> BookIssues => Set<BookIssue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
