using HotelManager.Application.DTOs.Reports;

namespace HotelManager.Application.Services.Interfaces;

public interface IReportService
{
    Task<DailyReportDto> GetDailyAsync();
    Task<List<OutstandingBalanceDto>> GetOutstandingAsync();
    Task<PeriodReportDto> GetWeeklyAsync();
    Task<PeriodReportDto> GetMonthlyAsync();
}
