// Impensa/DTOs/ExpenseDisplayDTO.cs

namespace Impensa.DTOs;

public class ExpenseDisplayDTO
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; }
}