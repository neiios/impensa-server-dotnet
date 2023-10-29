using Impensa.Models;
using Impensa.Repositories;

namespace Impensa.Services;

public interface IDefaultCategoriesService
{
    Task CreateDefaultCategoriesForUser(Guid userId);
}

public class DefaultCategoriesService : IDefaultCategoriesService
{
    private readonly AppDbContext _context;

    public DefaultCategoriesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateDefaultCategoriesForUser(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new ArgumentException("User not found");
        
        var defaultCategories = new List<ExpenseCategory>
        {
            new ExpenseCategory { Name = "Groceries", User = user },
            new ExpenseCategory { Name = "Utilities", User = user },
            new ExpenseCategory { Name = "Netflix", User = user },
            new ExpenseCategory { Name = "Cinema", User = user },
            new ExpenseCategory { Name = "Transportation", User = user },
            new ExpenseCategory { Name = "Gnu Foundation", User = user },
            new ExpenseCategory { Name = "Dining Out", User = user },
            new ExpenseCategory { Name = "Investments", User = user },
        };

        _context.ExpenseCategories.AddRange(defaultCategories);
        await _context.SaveChangesAsync();
    }

}