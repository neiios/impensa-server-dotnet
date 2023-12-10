namespace Impensa.Models;

public class Report
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool isSolved {get; set; }
    public DateTime CreatedAt { get; set; }
}