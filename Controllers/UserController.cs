using System.Security.Claims;
using Impensa.DTOs.Users;
using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/me")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
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
    public async Task<ActionResult<User>> GetUserInfo()
    {
        var userId = GetUserIdFromJwt();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult<User>> UpdateUserInfo(UserSignupRequestDto dto)
    {
        var userId = GetUserIdFromJwt();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        var updatedUser = MapUserSignupRequestDtoToUser(dto);
        updatedUser.Id = user.Id;
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteUser()
    {
        var userId = GetUserIdFromJwt();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        var expenses = _context.Expenses
            .Where(e => e.UserId == userId);
        var categories = _context.ExpenseCategories
            .Where(c => c.UserId == userId);

        _context.Expenses.RemoveRange(expenses);
        _context.ExpenseCategories.RemoveRange(categories);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private static User MapUserSignupRequestDtoToUser(UserSignupRequestDto dto)
    {
        return new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = dto.Password,
            Currency = dto.Currency
        };
    }
}