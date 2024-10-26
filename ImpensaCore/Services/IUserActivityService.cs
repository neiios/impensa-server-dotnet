using ImpensaCore.DTOs.UserLog;

namespace ImpensaCore.Services;

public interface IUserActivityService
{
    Task LogActivityAsync(UserLogRequestDto logDto);
}
