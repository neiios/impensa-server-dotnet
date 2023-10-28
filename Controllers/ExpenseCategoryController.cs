using System.Security.Claims;
using Impensa.DTOs.ExpenseCategories;
using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/categories")]
public class ExpenseCategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpenseCategoryController(AppDbContext context)
    {
        _context = context;
    }

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

        var categoryResponseDtos = await _context.ExpenseCategories
            .Where(c => c.UserId == userId)
            .Select(c => new ExpenseCategoryResponseDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

        return categoryResponseDtos;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseCategoryResponseDto>> GetCategory(Guid id)
    {
        var category = await _context.ExpenseCategories.FindAsync(id);
        if (category == null) return NotFound();

        return new ExpenseCategoryResponseDto { Id = category.Id, Name = category.Name };
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategoryResponseDto>> CreateCategory(ExpenseCategoryRequestDto dto)
    {
        var userId = GetUserIdFromJwt();

        var existingCategory = await _context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == dto.Name);
        if (existingCategory != null)
            return Conflict(new { message = "Category with this name already exists for the user." });

        var newCategory = new ExpenseCategory
        {
            Name = dto.Name,
            UserId = userId
        };

        _context.ExpenseCategories.Add(newCategory);
        await _context.SaveChangesAsync();

        return Ok(newCategory);
    }


    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, ExpenseCategoryRequestDto dto)
    {
        var userId = GetUserIdFromJwt();

        var existingCategory = await _context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (existingCategory == null) return NotFound();

        existingCategory.Name = dto.Name;
        await _context.SaveChangesAsync();

        return Ok();
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var userId = GetUserIdFromJwt();

        var category = await _context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category == null) return NotFound();

        _context.ExpenseCategories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}