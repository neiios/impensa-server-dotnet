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
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public UserController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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
    public async Task<ActionResult<User>> UpdateUserInfo(UserRequestEditDto dto)
    {
        var userId = GetUserIdFromJwt();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound("User not found");

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return BadRequest("The current password is incorrect.");
        }
    
        // If a new password is provided and it's different from the current password, update it
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }
    
        // Update other fields
        user.Username = dto.Username;
        user.Email = dto.Email;
        user.Currency = dto.Currency;

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
            .Where(e => e.User.Id == userId);
        var categories = _context.ExpenseCategories
            .Where(c => c.User.Id == userId);

        _context.Expenses.RemoveRange(expenses);
        _context.ExpenseCategories.RemoveRange(categories);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        await _emailService.SendDeletionEmail(user);

        return Ok();
    }

    private static User MapUserSignupRequestDtoToUser(UserInfoRequestDto dto)
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