using System.Security.Claims;
using Impensa.DTOs.UserLog;
using Impensa.Models;
using Impensa.Repositories;

namespace Impensa.Services;

public class UserActivityService(AppDbContext dbctx)
{
    public async Task LogActivityAsync(HttpContext ctx, HttpRequest req, ClaimsPrincipal principal, Guid userId)
    {
        var ip = ctx.Connection.RemoteIpAddress!.ToString();
        var browser = req.Headers.UserAgent.ToString();
        
        dbctx.UserActivityLogs.Add(new UserLog
        {
            UserId = userId,
            Date = DateTime.UtcNow,
            IP = ip,
            Browser = browser
        });
        
        await dbctx.SaveChangesAsync();
    }
}
