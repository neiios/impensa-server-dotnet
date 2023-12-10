namespace Impensa.DTOs.Users;

public class UserResponseDto
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string Currency { get; set; }

    public required string JwtToken { get; set; }
}
