using System.ComponentModel.DataAnnotations;

namespace Impensa.DTOs.Reports;

public class ReportRequestDto
{
    [MaxLength(200)] public required string Title { get; set; }
    [MaxLength(1000)] public required string Description { get; set; }
}
