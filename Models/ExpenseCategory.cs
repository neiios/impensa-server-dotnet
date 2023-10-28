namespace Impensa.Models;

public class ExpenseCategory
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required Guid UserId { get; set; }
    public User? User { get; set; }
}