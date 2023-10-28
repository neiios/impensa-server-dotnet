using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.Expenses;

public class ExpenseRequestDto
{
    [Required] public decimal Amount { get; set; }

    [Required] public DateTime Date { get; set; }

    [Required] public Guid ExpenseCategoryId { get; set; }

    [MaxLength(2048)] public required string Description { get; set; }
}