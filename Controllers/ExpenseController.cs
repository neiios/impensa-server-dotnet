using System.Security.Claims;
using Impensa.DTOs.ExpenseCategories;
using Impensa.DTOs.Expenses;
using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/expenses")]
public class ExpenseController(AppDbContext context) : ControllerBase
{
    private Guid GetUserIdFromJwt()
    {
        var guid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(guid)) throw new ArgumentException("User id not found in JWT token");

        return Guid.Parse(guid);
    }

    private static ExpenseResponseDto MapExpenseToResponseDto(Expense e)
    {
        return new ExpenseResponseDto
        {
            Id = e.Id,
            Amount = e.Amount,
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            SpentAt = e.SpentAt,
            ExpenseCategory = new ExpenseCategoryResponseDto
            {
                Id = e.ExpenseCategory.Id,
                Name = e.ExpenseCategory.Name
            }
        };
    }

    private static Expense MapExpenseRequestDtoToExpense(ExpenseRequestDto expenseDto, ExpenseCategory category,
        User user, DateTime createdAt, DateOnly spentAt)
    {
        return new Expense
        {
            Amount = expenseDto.Amount,
            Description = expenseDto.Description,
            User = user,
            ExpenseCategory = category,
            CreatedAt = createdAt,
            SpentAt = spentAt,
        };
    }

    [HttpGet]
    public async Task<List<ExpenseResponseDto>> GetAllExpenses()
    {
        var userId = GetUserIdFromJwt();

        var expenses = await context.Expenses
            .Include(e => e.ExpenseCategory)
            .Where(e => e.User.Id == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapExpenseToResponseDto(e))
            .ToListAsync();

        return expenses;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseResponseDto>> GetExpense(Guid id)
    {
        var userId = GetUserIdFromJwt();

        var expense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.User.Id == userId);
        if (expense == null) return NotFound();

        return MapExpenseToResponseDto(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> CreateExpense(ExpenseRequestDto dto)
    {
        var userId = GetUserIdFromJwt();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new { Message = "User not found" });

        var category = await context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.Id == dto.ExpenseCategoryId);
        if (category == null) return NotFound(new { Message = "Category not found" });

        var expense = MapExpenseRequestDtoToExpense(dto, category, user, DateTime.UtcNow, dto.SpentAt);
        expense.CreatedAt = DateTime.UtcNow;

        context.Expenses.Add(expense);
        await context.SaveChangesAsync();

        return MapExpenseToResponseDto(expense);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseResponseDto>> UpdateExpense(Guid id, ExpenseRequestDto dto)
    {
        var userId = GetUserIdFromJwt();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new { Message = "User not found" });

        var category = await context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.Id == dto.ExpenseCategoryId);
        if (category == null) return NotFound(new { Message = "Category not found" });

        var existingExpense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.User.Id == userId);
        if (existingExpense == null) return NotFound();

        var updatedExpense = MapExpenseRequestDtoToExpense(dto, category, user, existingExpense.CreatedAt, dto.SpentAt);
        existingExpense.Description = updatedExpense.Description;
        existingExpense.Amount = updatedExpense.Amount;
        existingExpense.SpentAt = updatedExpense.SpentAt;
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var userId = GetUserIdFromJwt();

        var expense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.User.Id == userId);
        if (expense == null) return NotFound();

        context.Expenses.Remove(expense);
        await context.SaveChangesAsync();

        return Ok();
    }
}