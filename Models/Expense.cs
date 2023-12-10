namespace Impensa.Models;

public class Expense
{
    public Guid Id { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime SpentAt {get; set; }

    public required ExpenseCategory ExpenseCategory { get; set; }
    public required User User { get; set; }
}