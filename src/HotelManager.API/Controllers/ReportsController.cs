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
    public async Task<IActionResult> GetDaily()
    {
        var report = await _reportService.GetDailyAsync();
        return Ok(report);
    }

    [HttpGet("outstanding")]
    public async Task<IActionResult> GetOutstanding()
    {
        var report = await _reportService.GetOutstandingAsync();
        return Ok(report);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeekly()
    {
        var report = await _reportService.GetWeeklyAsync();
        return Ok(report);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly()
    {
        var report = await _reportService.GetMonthlyAsync();
        return Ok(report);
    }
}
