namespace ImpensaCore.DTOs.Notifications;

public class NotificationResponseDto
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool IsRead { get; set; } 
}