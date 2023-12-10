using Impensa.Models;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public required DbSet<User> Users { get; init; }
    public required DbSet<Expense> Expenses { get; init; }
    public required DbSet<ExpenseCategory> ExpenseCategories { get; init; }
    public required DbSet<UserLog> UserActivityLogs { get; init; }
    public required DbSet<GithubUser> GithubUsers { get; init; }
    public required DbSet<Notification> Notifications { get; init; }
    public required DbSet<Report> Reports { get; init; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasPostgresExtension("uuid-ossp");

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
