using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.Users;

public class UserSigninRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MaxLength(255, ErrorMessage = "Field cannot be more than 255 characters")]
    public required string Password { get; set; }
}