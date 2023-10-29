using Impensa.DTOs.UserLog;
using Impensa.Models;
using Impensa.Repositories;

namespace Impensa.Services;

public interface IUserActivityService
{
    Task LogActivityAsync(UserLogRequestDto logDto);
}

public class UserActivityService : IUserActivityService
{
    private readonly AppDbContext _context;

    public UserActivityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(UserLogRequestDto logDto)
    {
        var logEntry = new UserLog
        {
            UserId = logDto.UserId,
            Date = logDto.Date,
            IP = logDto.IP,
            Browser = logDto.Browser
        };

        _context.UserActivityLogs.Add(logEntry);
        await _context.SaveChangesAsync();
    }
}
