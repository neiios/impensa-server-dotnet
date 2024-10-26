using System.ComponentModel.DataAnnotations;

namespace ImpensaCore.DTOs.Users;

public class UserRequestRemoveDto
{
    [MinLength(6)] [MaxLength(255)] public required string Password { get; set; }
}
