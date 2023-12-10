namespace Impensa.DTOs.Reports;

public class ReportResponseDto
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool IsSolved { get; set; } 
}