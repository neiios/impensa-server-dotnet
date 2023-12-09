using System.Security.Claims;
using Impensa.DTOs.Users;
using Impensa.Models;
using Impensa.Repositories;
using Impensa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/me")]
public class UserController(AppDbContext context, IEmailService emailService) : ControllerBase
{
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
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult<User>> UpdateUserInfo(UserRequestEditDto dto)
    {
        var userId = GetUserIdFromJwt();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)) return BadRequest("Password is incorrect.");

        if (!string.IsNullOrEmpty(dto.NewPassword)) user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.Currency = dto.Currency;

        await context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteUser(UserRequestRemoveDto dto)
    {
        var userId = GetUserIdFromJwt();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)) return BadRequest("Password is incorrect.");

        var expenses = context.Expenses
            .Where(e => e.User.Id == userId);
        var categories = context.ExpenseCategories
            .Where(c => c.User.Id == userId);

        context.Expenses.RemoveRange(expenses);
        context.ExpenseCategories.RemoveRange(categories);
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        await emailService.SendDeletionEmail(user);

        return Ok();
    }
}
