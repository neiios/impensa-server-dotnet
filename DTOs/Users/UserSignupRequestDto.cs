using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.Users;

public class UserSignupRequestDto
{
    [MaxLength(255, ErrorMessage = "Field cannot be more than 255 characters")]
    public required string Username { get; set; }

    [EmailAddress]
    public required string Email { get; set; }

    [MaxLength(255, ErrorMessage = "Field cannot be more than 255 characters")]
    public required string Currency { get; set; }

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    [MaxLength(255, ErrorMessage = "Field cannot be more than 255 characters")]
    public required string Password { get; set; }
}