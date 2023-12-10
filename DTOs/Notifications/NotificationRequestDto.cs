using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.Notifications;

public class NotificationRequestDto
{
    [MaxLength(2048)] public required string Title { get; set; }
    [MaxLength(2048)] public required string Description { get; set; }
}