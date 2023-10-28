using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOS.Users;

public class UserSigninRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}