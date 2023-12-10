using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.Users;

public class UserInfoRequestDto
{
    [MaxLength(255)] public required string Username { get; set; }

    [EmailAddress] public required string Email { get; set; }

    [MaxLength(255)] public required string Currency { get; set; }

    [MinLength(6)] [MaxLength(255)] public required string Password { get; set; }
}
