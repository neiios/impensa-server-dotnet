using System.ComponentModel.DataAnnotations;

namespace ImpensaCore.DTOs.ExpenseCategories;

public class ExpenseCategoryResponseDto
{
    [Required] public Guid Id { get; set; }

    public required string Name { get; set; }
}
