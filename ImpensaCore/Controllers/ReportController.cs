using System.Security.Claims;
using ImpensaCore.DTOs.Reports;
using ImpensaCore.Models;
using ImpensaCore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImpensaCore.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext context;

    public ReportsController(AppDbContext context)
    {
        this.context = context;
    }

    private Guid GetUserIdFromJwt()
    {
        var guid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(guid)) throw new ArgumentException("User id not found in JWT token");

        return Guid.Parse(guid);
    }

    [HttpPost]
    public async Task<ActionResult<ReportResponseDto>> CreateReport(ReportRequestDto dto)
    {
        var report = new Report
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            isSolved = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync();

        return new ReportResponseDto
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            IsSolved = report.isSolved,
            CreatedAt = report.CreatedAt
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportResponseDto>>> GetAllReports(bool? isSolved)
    {
        IQueryable<Report> query = context.Reports;

        if (isSolved.HasValue)
        {
            query = query.Where(r => r.isSolved == isSolved.Value);
        }

        var reports = await query.Select(r => new ReportResponseDto
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            IsSolved = r.isSolved,
            CreatedAt = r.CreatedAt
        }).ToListAsync();

        return Ok(reports);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReport(Guid id)
    {
        var report = await context.Reports.FindAsync(id);
        if (report == null) return NotFound();

        context.Reports.Remove(report);
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{id:guid}/toggle-solved")]
    public async Task<IActionResult> ToggleReportSolvedStatus(Guid id)
    {
        var report = await context.Reports.FindAsync(id);
        if (report == null) return NotFound();

        report.isSolved = !report.isSolved;
        await context.SaveChangesAsync();

        return Ok();
    }
}
