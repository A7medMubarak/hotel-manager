using HotelManager.Application.DTOs.Reports;

namespace HotelManager.Application.Services.Interfaces;

public interface IReportService
{
    Task<DailyReportDto> GetDailyAsync(CancellationToken cancellationToken = default);
    Task<List<OutstandingBalanceDto>> GetOutstandingAsync(CancellationToken cancellationToken = default);
    Task<PeriodReportDto> GetWeeklyAsync(CancellationToken cancellationToken = default);
    Task<PeriodReportDto> GetMonthlyAsync(CancellationToken cancellationToken = default);
}
