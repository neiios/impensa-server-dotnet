using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs;

public class CreateUserDetailsDto
{
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Currency is required")]
    public required string Currency { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public required string Password { get; set; }
}