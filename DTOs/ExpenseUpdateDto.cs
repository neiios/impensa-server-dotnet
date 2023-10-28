// Impensa/DTOs/ExpenseUpdateDTO.cs

namespace Impensa.DTOs
{
    public class ExpenseUpdateDTO : ExpenseCreateDTO
    {
        public Guid Id { get; set; }
    }
}