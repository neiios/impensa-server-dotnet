using Impensa.DTOs;
using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(CreateUserDetailsDto userDetailsDto)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDetailsDto.Password);
        var user = new User
        {
            Username = userDetailsDto.Username,
            Email = userDetailsDto.Email,
            Currency = userDetailsDto.Currency,
            Password = hashedPassword
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new { Message = "Email is already taken" });
        }

        return Ok(new ReturnUserDetailsDto
        {
            Username = user.Username,
            Email = user.Email,
            Currency = user.Currency
        });
    }


    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(LoginUserDetailsDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
        {
            return BadRequest(new { Message = "Invalid email or password" });
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
        if (!validPassword)
        {
            return BadRequest(new { Message = "Invalid email or password" });
        }

        // Here you'd generate a JWT or another authentication token and return it.
        // For the sake of simplicity, I'll just return an "Authenticated" message.
        return Ok();
    }

    [HttpGet("verify")]
    public IActionResult Verify()
    {
        return Ok();
    }
}