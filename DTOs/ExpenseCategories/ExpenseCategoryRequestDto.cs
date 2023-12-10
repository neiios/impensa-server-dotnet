using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.ExpenseCategories;

public class ExpenseCategoryRequestDto
{
    [MaxLength(255)] public required string Name { get; set; }
}
