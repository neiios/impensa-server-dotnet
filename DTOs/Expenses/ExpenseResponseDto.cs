using Impensa.DTOs.ExpenseCategories;

namespace Impensa.DTOs.Expenses;

public class ExpenseResponseDto
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Description { get; set; }

    public required ExpenseCategoryResponseDto ExpenseCategory { get; set; }
}
