using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.Users;

public class UserRequestRemoveDto
{
    [MinLength(6)] [MaxLength(255)] public required string Password { get; set; }
}
