using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Impensa.Configuration;
using Impensa.DTOs.UserLog;
using Impensa.DTOs.Users;
using Impensa.Models;
using Impensa.Repositories;
using Impensa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/auth")]
public class AuthController(
    AppDbContext context,
    IConfiguration configuration,
    IEmailService emailService,
    IDefaultCategoriesService defaultCategoriesService,
    IUserActivityService userActivityService)
    : ControllerBase
{
    private string GenerateJwtToken(string guid)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind("JwtSettings", jwtSettings);
        var key = Encoding.ASCII.GetBytes(jwtSettings.Key!);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, guid)
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
    public async Task<IActionResult> SignUp(UserInfoRequestDto requestDto)
    {
        var user = new User
        {
            Username = requestDto.Username,
            Email = requestDto.Email,
            Currency = requestDto.Currency,
            Password = BCrypt.Net.BCrypt.HashPassword(requestDto.Password)
        };

        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            await defaultCategoriesService.CreateDefaultCategoriesForUser(user.Id);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new { Message = "Email is already taken" });
        }

        var returnDto = new UserResponseDto
        {
            Username = user.Username,
            Email = user.Email,
            Currency = user.Currency,
            JwtToken = GenerateJwtToken(user.Id.ToString())
        };

        await emailService.SendWelcomeEmail(user);

        return Ok(returnDto);
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(UserSigninRequestDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return BadRequest(new { Message = "Invalid email or password" });

        var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
        if (!validPassword) return BadRequest(new { Message = "Invalid email or password" });

        var ip = HttpContext.Connection.RemoteIpAddress!.ToString();
        var browser = Request.Headers.UserAgent.ToString();

        var logDto = new UserLogRequestDto
        {
            UserId = user.Id,
            Date = DateTime.UtcNow,
            IP = ip,
            Browser = browser
        };

        await userActivityService.LogActivityAsync(logDto);

        var returnDto = new UserResponseDto
        {
            Username = user.Username,
            Email = user.Email,
            Currency = user.Currency,
            JwtToken = GenerateJwtToken(user.Id.ToString())
        };

        return Ok(returnDto);
    }


    [Authorize]
    [HttpGet("verify")]
    public IActionResult Verify()
    {
        var userId = User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { userId });
    }
}
