using System.Security.Claims;
using ImpensaCore.DTOs.ExpenseCategories;
using ImpensaCore.Models;
using ImpensaCore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImpensaCore.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/categories")]
public class ExpenseCategoryController(AppDbContext context) : ControllerBase
{
    private Guid GetUserIdFromJwt()
    {
        var guid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(guid)) throw new ArgumentException("User id not found in JWT token");

        return Guid.Parse(guid);
    }

    [HttpGet]
    public async Task<List<ExpenseCategoryResponseDto>> GetCategories()
    {
        var userId = GetUserIdFromJwt();

        var categoryResponseDtos = await context.ExpenseCategories
            .Where(c => c.User.Id == userId)
            .Select(c => new ExpenseCategoryResponseDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

        return categoryResponseDtos;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseCategoryResponseDto>> GetCategory(Guid id)
    {
        var userId = GetUserIdFromJwt();

        var category = await context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == userId);
        if (category == null) return NotFound();

        return new ExpenseCategoryResponseDto { Id = category.Id, Name = category.Name };
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategoryResponseDto>> CreateCategory(ExpenseCategoryRequestDto dto)
    {
        var userId = GetUserIdFromJwt();

        var existingCategory = await context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.User.Id == userId && c.Name == dto.Name);
        if (existingCategory != null)
            return Conflict(new { message = "Category with this name already exists for this user" });

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound("User not found");

        var newCategory = new ExpenseCategory
        {
            Name = dto.Name,
            User = user
        };

        context.ExpenseCategories.Add(newCategory);
        await context.SaveChangesAsync();

        var responseDto = new ExpenseCategoryResponseDto
        {
            Id = newCategory.Id,
            Name = newCategory.Name
        };

        return Ok(responseDto);
    }


    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, ExpenseCategoryRequestDto dto)
    {
        var userId = GetUserIdFromJwt();

        var existingCategory = await context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == userId);
        if (existingCategory == null) return NotFound();

        var sameNameCategory = await context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.User.Id == userId && c.Name == dto.Name);
        if (sameNameCategory != null)
            return Conflict(new { message = "Category with this name already exists for this user" });

        existingCategory.Name = dto.Name;
        await context.SaveChangesAsync();

        return Ok(existingCategory);
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var userId = GetUserIdFromJwt();

        var category = await context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == userId);
        if (category == null) return NotFound();

        context.ExpenseCategories.Remove(category);
        await context.SaveChangesAsync();

        return Ok(category);
    }
}
