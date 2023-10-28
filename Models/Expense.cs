using System.ComponentModel.DataAnnotations.Schema;

namespace Impensa.Models;

public class Expense
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    
    public Guid ExpenseCategoryId { get; set; }
    public ExpenseCategory Category { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
}