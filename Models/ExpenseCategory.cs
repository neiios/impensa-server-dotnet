namespace Impensa.Models;

public class ExpenseCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
}