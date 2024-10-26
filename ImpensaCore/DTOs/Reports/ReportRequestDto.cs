using System.ComponentModel.DataAnnotations;

namespace ImpensaCore.DTOs.Reports;

public class ReportRequestDto
{
    [MaxLength(200)] public required string Title { get; set; }
    [MaxLength(1000)] public required string Description { get; set; }
}
