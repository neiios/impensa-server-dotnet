using System.ComponentModel.DataAnnotations.Schema;

namespace Impensa.Models;

public class Expense
{
    public Guid Id { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }
    public required DateTime Date { get; set; }

    public required Guid ExpenseCategoryId { get; set; }
    public ExpenseCategory? Category { get; set; }

    public required Guid UserId { get; set; }
    public User? User { get; set; }
}