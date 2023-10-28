namespace Impensa.DTOs;

public class ReturnUserDetailsDto
{
    public required string Username { get; set; }
    
    public required string Email { get; set; }
    
    public required string Currency { get; set; }
}