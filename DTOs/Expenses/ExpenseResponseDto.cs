using System.ComponentModel.DataAnnotations;
using Impensa.DTOs.ExpenseCategories;

namespace Impensa.DTOs.Expenses;

public class ExpenseResponseDto
{
    [Required] public Guid Id { get; set; }

    [Required] public decimal Amount { get; set; }

    [Required] public required DateTime Date { get; set; }

    public string? Description { get; set; }

    public required ExpenseCategoryResponseDto ExpenseCategory { get; set; }
}