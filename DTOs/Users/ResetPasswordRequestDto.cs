namespace Impensa.DTOs.Users;

public class ResetPasswordRequestDto
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
