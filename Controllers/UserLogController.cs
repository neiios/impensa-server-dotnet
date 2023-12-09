using System.Security.Claims;
using Impensa.DTOs.UserLog;
using Impensa.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Impensa.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/logs")]
public class UserLogController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUserLogs()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var logs = await context.UserActivityLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Date)
            .Select(log => new UserLogResponseDto
            {
                Date = log.Date,
                IP = log.IP,
                Browser = log.Browser
            })
            .ToListAsync();

        return Ok(logs);
    }
}
