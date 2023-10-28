using Impensa.Models;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Repositories;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public required DbSet<User> Users { get; set; }
    public required DbSet<Expense> Expenses { get; set; }
    public required DbSet<ExpenseCategory> ExpenseCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasPostgresExtension("uuid-ossp");

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}