using Impensa.Models;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public required DbSet<User> Users { get; set; }
    public required DbSet<Expense> Expenses { get; set; }
    public required DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public required DbSet<UserLog> UserActivityLogs { get; set; }
    public required DbSet<Notification> Notifications { get; set; }
    public required DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasPostgresExtension("uuid-ossp");

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
