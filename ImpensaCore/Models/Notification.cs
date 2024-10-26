namespace ImpensaCore.Models;

public class Notification
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public bool IsRead {get; set; }
    public string? Description { get; set; }
    public required User User { get; set; }
}