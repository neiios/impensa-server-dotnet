using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Impensa.Configuration;
using Impensa.DTOs;
using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = new JwtSettings();
        _configuration.Bind("JwtSettings", jwtSettings);

        var key = Encoding.ASCII.GetBytes(jwtSettings.Key);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.DurationInMinutes),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            Token = token,
            UserDetails = new ReturnUserDetailsDto
            {
                Username = user.Username,
                Email = user.Email,
                Currency = user.Currency
            }
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

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Token = token,
            UserDetails = new ReturnUserDetailsDto
            {
                Username = user.Username,
                Email = user.Email,
                Currency = user.Currency
            }
        });
    }

    [Authorize]
    [HttpGet("verify")]
    public IActionResult Verify()
    {
        return Ok();
    }
}