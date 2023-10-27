using System.ComponentModel.DataAnnotations.Schema;

namespace Impensa.Models;

public class Expense
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public List<ExpenseCategory> Categories { get; set; }
    public DateTime Date { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
}