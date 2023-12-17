using System.Security.Claims;
using Impensa.DTOs.UserLog;
using Impensa.DTOs.Users;
using Impensa.Models;
using Impensa.Repositories;
using Impensa.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/auth")]
public class AuthController(
    AppDbContext dbctx,
    UserActivityService userActivityService,
    IEmailService emailService,
    AuthService authService)
    : ControllerBase
{
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(UserInfoRequestDto requestDto)
    {
        try
        {
            var user = await authService.CreateLocalUser(HttpContext, requestDto);
            await userActivityService.LogActivityAsync(HttpContext, Request, User, user.Id);
            return Ok(new
            {
                user.Username,
                user.Email,
                user.Currency
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(UserSigninRequestDto dto)
    {
        var user = await dbctx.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return BadRequest(new { Message = "Invalid email or password" });

        var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
        if (!validPassword) return BadRequest(new { Message = "Invalid email or password" });

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), },
            "cookie"));
        await HttpContext.SignInAsync("cookie", claimsPrincipal);

        await userActivityService.LogActivityAsync(HttpContext, Request, User, user.Id);

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Currency
        });
    }

    [HttpGet("github")]
    public IResult Github()
    {
        return Results.Challenge(
            new AuthenticationProperties { RedirectUri = $"{Environment.GetEnvironmentVariable("CLIENT_ADDRESS")}/signin" },
            authenticationSchemes: new[] { "github" });
    }
    
    [HttpGet("reset-password")]
    public async Task<IActionResult> ResetPassword([FromQuery] string email)
    {
        var user = await dbctx.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return BadRequest(new { Message = "Invalid email" });

        var token = await authService.GeneratePasswordResetToken(user);
        await emailService.SendPasswordResetEmail(user, token);

        return Ok();
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        var user = await dbctx.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return BadRequest(new { Message = "Invalid email" });

        var validToken = authService.ValidatePasswordResetToken(user, dto.Token);
        if (!validToken) return BadRequest(new { Message = "Invalid token" });

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.PassswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        await dbctx.SaveChangesAsync();

        await emailService.SendPasswordResetConfirmationEmail(user);
        
        return Ok();
    }

    [Authorize]
    [HttpGet("verify")]
    public async Task<IActionResult> Verify()
    {
        var cookieId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(cookieId)) return BadRequest("no id in cookie");

        // if id is a guid then its our cookie and all is good
        if (Guid.TryParse(cookieId, out var userId)) return Ok();

        // work with github
        var user = await authService.CreateGithubUserOrConvertToLocalCookie(HttpContext, User, cookieId);
        await userActivityService.LogActivityAsync(HttpContext, Request, User, user.Id);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("signout")]
    public async Task<IActionResult> Signout()
    {
        await HttpContext.SignOutAsync("cookie");
        return Ok();
    }

    [Authorize]
    [HttpGet("verify-cookie")]
    public IActionResult CheckCookie()
    {
        return Ok(User.Claims.Select(x => new { x.Type, x.Value }).ToList());
    }
}
