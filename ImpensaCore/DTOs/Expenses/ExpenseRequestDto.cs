using System.ComponentModel.DataAnnotations;

namespace ImpensaCore.DTOs.Expenses;

public class ExpenseRequestDto
{
    [Required] public decimal Amount { get; set; }

    [Required] public DateTime SpentAt { get; set; }

    [Required] public Guid ExpenseCategoryId { get; set; }

    [MaxLength(2048)] public required string Description { get; set; }
}
