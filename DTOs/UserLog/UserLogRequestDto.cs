namespace Impensa.DTOs.UserLog;

public class UserLogRequestDto
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public string IP { get; set; }
    public string Browser { get; set; }
}