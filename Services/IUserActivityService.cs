using Impensa.DTOs.UserLog;

namespace Impensa.Services;

public interface IUserActivityService
{
    Task LogActivityAsync(UserLogRequestDto logDto);
}
