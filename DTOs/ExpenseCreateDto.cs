// Impensa/DTOs/ExpenseCreateDTO.cs

namespace Impensa.DTOs
{
    public class ExpenseCreateDTO
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public Guid ExpenseCategoryId { get; set; }
    }
}