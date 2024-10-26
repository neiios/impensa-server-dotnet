using System.ComponentModel.DataAnnotations;

namespace ImpensaCore.DTOs.ExpenseCategories;

public class ExpenseCategoryRequestDto
{
    [MaxLength(255)] public required string Name { get; set; }
}
