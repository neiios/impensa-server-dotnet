namespace ImpensaCore.Models;

public class ExpenseCategory
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required User User { get; set; }
}
