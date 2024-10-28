using ImpensaCore.Models;
using Microsoft.EntityFrameworkCore;

namespace ImpensaCore.Repositories;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Users = Set<User>();
        Expenses = Set<Expense>();
        ExpenseCategories = Set<ExpenseCategory>();
        UserActivityLogs = Set<UserLog>();
        GithubUsers = Set<GithubUser>();
        Notifications = Set<Notification>();
        Reports = Set<Report>();
    }

    public DbSet<User> Users { get; init; }
    public DbSet<Expense> Expenses { get; init; }
    public DbSet<ExpenseCategory> ExpenseCategories { get; init; }
    public DbSet<UserLog> UserActivityLogs { get; init; }
    public DbSet<GithubUser> GithubUsers { get; init; }
    public DbSet<Notification> Notifications { get; init; }
    public DbSet<Report> Reports { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
