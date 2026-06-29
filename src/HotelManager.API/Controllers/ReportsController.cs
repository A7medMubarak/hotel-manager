using HotelManager.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Owner")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetDailyAsync(cancellationToken);
        return Ok(report);
    }

    [HttpGet("outstanding")]
    public async Task<IActionResult> GetOutstanding(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetOutstandingAsync(cancellationToken);
        return Ok(report);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeekly(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetWeeklyAsync(cancellationToken);
        return Ok(report);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetMonthlyAsync(cancellationToken);
        return Ok(report);
    }
}
