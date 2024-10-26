using ImpensaCore.Models;
using ImpensaCore.Repositories;

namespace ImpensaCore.Services;

public interface IDefaultCategoriesService
{
    Task CreateDefaultCategoriesForUser(Guid userId);
}

public class DefaultCategoriesService(AppDbContext context) : IDefaultCategoriesService
{
    public async Task CreateDefaultCategoriesForUser(Guid userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new ArgumentException("User not found");

        var defaultCategories = new List<ExpenseCategory>
        {
            new() { Name = "Groceries", User = user },
            new() { Name = "Utilities", User = user },
            new() { Name = "Netflix", User = user },
            new() { Name = "Cinema", User = user },
            new() { Name = "Transportation", User = user },
            new() { Name = "Gnu Foundation", User = user },
            new() { Name = "Dining Out", User = user },
            new() { Name = "Investments", User = user }
        };

        context.ExpenseCategories.AddRange(defaultCategories);
        await context.SaveChangesAsync();
    }
}
