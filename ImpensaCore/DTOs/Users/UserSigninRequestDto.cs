using System.ComponentModel.DataAnnotations;

namespace ImpensaCore.DTOs.Users;

public class UserSigninRequestDto
{
    [EmailAddress] public required string Email { get; set; }

    [MaxLength(255)] public required string Password { get; set; }
}
