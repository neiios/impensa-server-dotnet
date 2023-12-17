using System.ComponentModel.DataAnnotations;

namespace Impensa.Models;

public class User
{
    public Guid Id { get; init; }
    [MaxLength(255)] public required string Username { get; set; }
    [MaxLength(255)] public required string Email { get; set; }
    [MaxLength(255)] public string? Password { get; set; }
    [MaxLength(255)] public required string Currency { get; set; }
    [MaxLength(255)] public string? PassswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
}
