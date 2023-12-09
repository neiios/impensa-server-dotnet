using Impensa.DTOs.UserLog;
using Impensa.Models;
using Impensa.Repositories;

namespace Impensa.Services;

public class UserActivityService(AppDbContext context) : IUserActivityService
{
    public async Task LogActivityAsync(UserLogRequestDto logDto)
    {
        var logEntry = new UserLog
        {
            UserId = logDto.UserId,
            Date = logDto.Date,
            IP = logDto.IP,
            Browser = logDto.Browser
        };

        context.UserActivityLogs.Add(logEntry);
        await context.SaveChangesAsync();
    }
}
